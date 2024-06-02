using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Jinaga;

namespace Marketplace.SaaS.Accelerator.CustomerSite.Integration;

public class MarketplaceIntegrator
{
    private readonly JinagaClient jinagaClient;
    private readonly IOptions<JinagaConfiguration> configuration;
    private readonly ILogger<MarketplaceIntegrator> logger;

    public MarketplaceIntegrator(JinagaClient jinagaClient, IOptions<JinagaConfiguration> configuration, ILogger<MarketplaceIntegrator> logger)
    {
        this.jinagaClient = jinagaClient;
        this.configuration = configuration;
        this.logger = logger;
    }

    public async Task<string> GetServicePrincipalPublicKey()
    {
        var (user, profile) = await jinagaClient.Login();
        var jsonPublicKey = System.Text.Json.JsonSerializer.Serialize(user.publicKey);
        logger.LogInformation($"Service principal public key: {jsonPublicKey}");
        return jsonPublicKey;
    }

    public async Task Subscribe(Guid subscriptionId, string planId, int quantity, string userEmailAddress)
    {
        logger.LogInformation($"Creating subscription {subscriptionId}");

        var subscription = await CreateSubscription(subscriptionId);
        await SetPlan(subscription, planId);
        await SetUser(subscription, userEmailAddress);

        logger.LogInformation($"Created subscription {subscriptionId}");
    }

    public async Task Activate(Guid subscriptionId)
    {
        logger.LogInformation($"Activating subscription {subscriptionId}");

        var subscription = await CreateSubscription(subscriptionId);

        var activate = new Activate(subscription, DateTime.UtcNow);
        await jinagaClient.Fact(activate);

        logger.LogInformation($"Activated subscription {subscriptionId}");
    }

    public async Task Deactivate(Guid subscriptionId)
    {
        logger.LogInformation($"Deactivating subscription {subscriptionId}");

        var subscription = await CreateSubscription(subscriptionId);

        var activationsForSubscription = Given<Subscription>.Match((subscription, facts) =>
            from activate in facts.OfType<Activate>()
            where activate.subscription == subscription &&
                !facts.Any<Deactivate>(d => d.activate == activate)
            select activate
        );

        var activations = await jinagaClient.Query(activationsForSubscription, subscription);

        foreach (var activation in activations)
        {
            var deactivate = new Deactivate(activation, DateTime.UtcNow);
            await jinagaClient.Fact(deactivate);

            logger.LogInformation($"Deactivated subscription {subscriptionId} from {activation.activatedAt}");
        }
    }

    public async Task ChangePlan(Guid subscriptionId, string planId)
    {
        logger.LogInformation($"Changing plan for subscription {subscriptionId} to {planId}");

        var subscription = await CreateSubscription(subscriptionId);

        var changed = await SetPlan(subscription, planId);
        if (changed)
        {
            logger.LogInformation($"Changed plan for subscription {subscriptionId} to {planId}");
        }
        else
        {
            logger.LogInformation($"Plan for subscription {subscriptionId} is already {planId}");
        }
    }

    public async Task ChangeUser(Guid subscriptionId, string userEmailAddress)
    {
        logger.LogInformation($"Changing user for subscription {subscriptionId} to {userEmailAddress}");

        var subscription = await CreateSubscription(subscriptionId);

        bool changed = await SetUser(subscription, userEmailAddress);
        if (changed)
        {
            logger.LogInformation($"Changed user for subscription {subscriptionId} to {userEmailAddress}");
        }
        else
        {
            logger.LogInformation($"User for subscription {subscriptionId} is already {userEmailAddress}");
        }
    }

    public async Task Suspend(Guid subscriptionId)
    {
        logger.LogInformation($"Suspending subscription {subscriptionId}");

        var subscription = await CreateSubscription(subscriptionId);

        var suspend = new Suspend(subscription, DateTime.UtcNow);
        await jinagaClient.Fact(suspend);

        logger.LogInformation($"Suspended subscription {subscriptionId}");
    }

    public async Task Reinstate(Guid subscriptionId)
    {
        logger.LogInformation($"Reinstating subscription {subscriptionId}");

        var subscription = await CreateSubscription(subscriptionId);

        var suspensionsForSubscription = Given<Subscription>.Match((subscription, facts) =>
            from suspend in facts.OfType<Suspend>()
            where suspend.subscription == subscription &&
                !facts.Any<Reinstate>(r => r.suspend == suspend)
            select suspend
        );

        var suspensions = await jinagaClient.Query(suspensionsForSubscription, subscription);

        foreach (var suspension in suspensions)
        {
            var reinstate = new Reinstate(suspension, DateTime.UtcNow);
            await jinagaClient.Fact(reinstate);

            logger.LogInformation($"Reinstated subscription {subscriptionId} suspended at {suspension.suspendedAt}");
        }
    }

    public async Task Renew(Guid subscriptionId)
    {
        logger.LogInformation($"Renewing subscription {subscriptionId}");

        var subscription = await CreateSubscription(subscriptionId);

        var utcNow = DateTime.UtcNow;
        var month = new Month(subscription.environment, utcNow.Year, utcNow.Month);

        var renew = new Renew(subscription, month, utcNow);
        await jinagaClient.Fact(renew);

        logger.LogInformation($"Renewed subscription {subscriptionId}");
    }

    public async Task Unsubscribe(Guid subscriptionId)
    {
        logger.LogInformation($"Unsubscribing subscription {subscriptionId}");

        var subscription = await CreateSubscription(subscriptionId);

        var unsubscribe = new Unsubscribe(subscription);
        await jinagaClient.Fact(unsubscribe);

        logger.LogInformation($"Unsubscribed subscription {subscriptionId}");
    }

    public async Task<Subscription> CreateSubscription(Guid subscriptionId)
    {
        var environment = CreateEnvironment();
        var subscription = new Subscription(environment, subscriptionId);
        return await jinagaClient.Fact(subscription);
    }

    private Environment CreateEnvironment()
    {
        string encoded = configuration.Value.CreatorPublicKey;
        logger.LogInformation("Decoding string length {length}: {value}", encoded.Length, encoded);
        var publicKey = Regex.Unescape(encoded.Trim('"'));
        logger.LogInformation("Decoded to length {length}: {value}", publicKey.Length, publicKey);
        var creator = new User(publicKey);
        var environment = new Environment(creator, configuration.Value.EnvironmentName);
        return environment;
    }

    private async Task<bool> SetPlan(Subscription subscription, string planId)
    {
        var plan = new Plan(subscription.environment, planId);

        var plansForSubscription = Given<Subscription>.Match((subscription, facts) =>
            from subscriptionPlan in facts.OfType<SubscriptionPlan>()
            where subscriptionPlan.subscription == subscription &&
                !facts.Any<SubscriptionPlan>(next => next.prior.Contains(subscriptionPlan))
            select subscriptionPlan
        );

        var existingPlans = await jinagaClient.Query(plansForSubscription, subscription);
        if (existingPlans.Count() != 1 || existingPlans.Single().plan.planId != planId)
        {
            var subscriptionPlan = new SubscriptionPlan(subscription, plan, existingPlans.ToArray());
            await jinagaClient.Fact(subscriptionPlan);
            return true;
        }
        return false;
    }

    private async Task<bool> SetUser(Subscription subscription, string userEmailAddress)
    {
        userEmailAddress = userEmailAddress.ToLowerInvariant();

        var usersForSubscription = Given<Subscription>.Match((subscription, facts) =>
            from subscriptionUser in facts.OfType<SubscriptionUserIdentity>()
            where subscriptionUser.subscription == subscription &&
                !facts.Any<SubscriptionUserIdentity>(next => next.prior.Contains(subscriptionUser))
            select subscriptionUser
        );

        var existingUsers = await jinagaClient.Query(usersForSubscription, subscription);
        if (existingUsers.Count() != 1 || existingUsers.Single().userIdentity.userId != userEmailAddress)
        {
            var user = new UserIdentity(subscription.environment, userEmailAddress);
            var subscriptionUser = new SubscriptionUserIdentity(subscription, user, existingUsers.ToArray());
            await jinagaClient.Fact(subscriptionUser);

            return true;
        }

        return false;
    }
}