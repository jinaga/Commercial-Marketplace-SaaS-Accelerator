using Jinaga;
using System;

namespace Marketplace.SaaS.Accelerator.CustomerSite.Integration;

[FactType("Azure.Environment")]
public record Environment(User creator, string environmentId);

[FactType("Azure.Administrator")]
public record Administrator(User user, Environment environment, DateTime createdAt);

[FactType("Azure.Administrator.Revoke")]
public record RevokeAdministrator(Administrator administrator);

[FactType("Azure.ServicePrincipal")]
public record ServicePrincipal(User user, Environment environment, DateTime createdAt);

[FactType("Azure.ServicePrincipal.Revoke")]
public record RevokeServicePrincipal(ServicePrincipal servicePrincipal);

[FactType("Azure.Subscription")]
public record Subscription(Environment environment, Guid subscriptionId);

[FactType("Azure.Subscription.Unsubscribe")]
public record Unsubscribe(Subscription subscription);

[FactType("Azure.UserIdentity")]
public record UserIdentity(Environment environment, string userId);

[FactType("Azure.Subscription.UserIdentity")]
public record SubscriptionUserIdentity(Subscription subscription, UserIdentity userIdentity, SubscriptionUserIdentity[] prior);

[FactType("Azure.Plan")]
public record Plan(Environment environment, string planId);

[FactType("Azure.SubscriptionPlan")]
public record SubscriptionPlan(Subscription subscription, Plan plan, SubscriptionPlan[] prior);

[FactType("Azure.Subscription.Activate")]
public record Activate(Subscription subscription, DateTime activatedAt);

[FactType("Azure.Subscription.Deactivate")]
public record Deactivate(Activate activate, DateTime deactivatedAt);

[FactType("Azure.Subscription.Suspend")]
public record Suspend(Subscription subscription, DateTime suspendedAt);

[FactType("Azure.Subscription.Reinstate")]
public record Reinstate(Suspend suspend, DateTime reinstatedAt);

[FactType("Azure.Month")]
public record Month(Environment environment, int year, int month);

[FactType("Azure.Subscription.Renew")]
public record Renew(Subscription subscription, Month month, DateTime renewedAt);