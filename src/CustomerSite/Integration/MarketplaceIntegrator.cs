using System;
using System.Linq;
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

    public async Task Activate(Guid subscriptionId)
    {
        var subscription = await CreateSubscription(subscriptionId);

        var activate = new Activate(subscription, DateTime.UtcNow);
        await jinagaClient.Fact(activate);

        logger.LogInformation($"Activated subscription {subscriptionId}");
    }

    public async Task Deactivate(Guid subscriptionId)
    {
        var subscription = await CreateSubscription(subscriptionId);

        var activationsForSubscription = Given<Subscription>.Match((subscription, facts) =>
            from activate in facts.OfType<Activate>()
            where activate.subscription == subscription
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
        var environment = CreateEnvironment();
        var plan = new Plan(environment, planId);
        var subscription = await CreateSubscription(subscriptionId);

        var plansForSubscription = Given<Subscription>.Match((subscription, facts) =>
            from subscriptionPlan in facts.OfType<SubscriptionPlan>()
            where subscriptionPlan.subscription == subscription
            select subscriptionPlan
        );

        var existingPlans = await jinagaClient.Query(plansForSubscription, subscription);
        if (existingPlans.Count() != 1 || existingPlans.Single().plan.planId != planId)
        {
            var subscriptionPlan = new SubscriptionPlan(subscription, plan, existingPlans.ToArray());
            await jinagaClient.Fact(subscriptionPlan);

            logger.LogInformation($"Changed plan for subscription {subscriptionId} to {planId}");
        }
    }

    public async Task ChangeQuantity(Guid subscriptionId, int quantity)
    {
        var subscription = await CreateSubscription(subscriptionId);

        var quantitiesForSubscription = Given<Subscription>.Match((subscription, facts) =>
            from subscriptionQuantity in facts.OfType<SubscriptionQuantity>()
            where subscriptionQuantity.subscription == subscription
            select subscriptionQuantity
        );

        var existingQuantities = await jinagaClient.Query(quantitiesForSubscription, subscription);
        if (existingQuantities.Count() != 1 || existingQuantities.Single().quantity != quantity)
        {
            var subscriptionQuantity = new SubscriptionQuantity(subscription, quantity, existingQuantities.ToArray());
            await jinagaClient.Fact(subscriptionQuantity);

            logger.LogInformation($"Changed quantity for subscription {subscriptionId} to {quantity}");
        }
    }

    public async Task Suspend(Guid subscriptionId)
    {
        var subscription = await CreateSubscription(subscriptionId);

        var suspend = new Suspend(subscription, DateTime.UtcNow);
        await jinagaClient.Fact(suspend);

        logger.LogInformation($"Suspended subscription {subscriptionId}");
    }

    public async Task Reinstate(Guid subscriptionId)
    {
        var subscription = await CreateSubscription(subscriptionId);

        var suspensionsForSubscription = Given<Subscription>.Match((subscription, facts) =>
            from suspend in facts.OfType<Suspend>()
            where suspend.subscription == subscription
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
        var subscription = await CreateSubscription(subscriptionId);

        var utcNow = DateTime.UtcNow;
        var month = new Month(subscription.environment, utcNow.Year, utcNow.Month);

        var renew = new Renew(subscription, month, utcNow);
        await jinagaClient.Fact(renew);

        logger.LogInformation($"Renewed subscription {subscriptionId}");
    }

    public async Task Unsubscribe(Guid subscriptionId)
    {
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
        var creator = new User(configuration.Value.CreatorPublicKey);
        var environment = new Environment(creator, configuration.Value.EnvironmentName);
        return environment;
    }
}