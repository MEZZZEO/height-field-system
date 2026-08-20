using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Utilities.Lifetimes;
using Utilities.Reactive;
using Object = UnityEngine.Object;

namespace Utilities.Tests
{
    public class ViewableCollectionsTests
    {
        [Test]
        public void ViewableListClearEmitsRemoveEventsAndTerminatesItemLifetimes()
        {
            var lifetimeDefinition = new LifetimeDefinition();
            var list = new ViewableList<int> { 1, 2 };
            var events = new List<(AddRemove Change, int Item)>();
            var terminatedItems = new List<int>();

            list.Advise(lifetimeDefinition.Lifetime, (change, item) => events.Add((change, item)));
            list.View(lifetimeDefinition.Lifetime, (itemLifetime, _, item) =>
            {
                itemLifetime.OnTermination(() => terminatedItems.Add(item));
            });

            list.Clear();

            Assert.That(list.Count, Is.EqualTo(0));
            Assert.That(events, Is.EqualTo(new[]
            {
                (AddRemove.Remove, 1),
                (AddRemove.Remove, 2)
            }));
            Assert.That(terminatedItems, Is.EquivalentTo(new[] { 1, 2 }));

            lifetimeDefinition.Terminate();
        }

        [Test]
        public void ViewableListViewProcessesAdditionsWithoutRebuildingExistingItems()
        {
            var lifetimeDefinition = new LifetimeDefinition();
            var list = new ViewableList<int>();
            var handledItems = new List<int>();
            var terminatedItems = new List<int>();

            list.View(lifetimeDefinition.Lifetime, (itemLifetime, _, item) =>
            {
                handledItems.Add(item);
                itemLifetime.OnTermination(() => terminatedItems.Add(item));
            });

            list.Add(1);
            list.Add(2);
            list.Add(3);

            Assert.That(handledItems, Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(handledItems.Count, Is.EqualTo(3));

            lifetimeDefinition.Terminate();

            Assert.That(terminatedItems, Is.EquivalentTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void ViewableListRemoveTerminatesOnlyMatchingItem()
        {
            var lifetimeDefinition = new LifetimeDefinition();
            var list = new ViewableList<int>();
            var terminatedItems = new List<int>();

            list.View(lifetimeDefinition.Lifetime, (itemLifetime, _, item) =>
            {
                itemLifetime.OnTermination(() => terminatedItems.Add(item));
            });

            list.Add(1);
            list.Add(2);
            list.Add(3);

            terminatedItems.Clear();

            var removed = list.Remove(2);

            Assert.That(removed, Is.True);
            Assert.That(terminatedItems, Is.EqualTo(new[] { 2 }));

            lifetimeDefinition.Terminate();
        }

        [Test]
        public void ViewableSetClearEmitsRemoveEventsAndTerminatesItemLifetimes()
        {
            var lifetimeDefinition = new LifetimeDefinition();
            var set = new ViewableSet<int> { 1, 2 };
            var events = new List<(AddRemove Change, int Item)>();
            var terminatedItems = new List<int>();

            set.Advise(lifetimeDefinition.Lifetime, (change, item) => events.Add((change, item)));
            set.View(lifetimeDefinition.Lifetime, (itemLifetime, item) =>
            {
                itemLifetime.OnTermination(() => terminatedItems.Add(item));
            });

            set.Clear();

            Assert.That(set.Count, Is.EqualTo(0));
            Assert.That(events, Is.EquivalentTo(new[]
            {
                (AddRemove.Remove, 1),
                (AddRemove.Remove, 2)
            }));
            Assert.That(terminatedItems, Is.EquivalentTo(new[] { 1, 2 }));

            lifetimeDefinition.Terminate();
        }

        [Test]
        public void ViewableMapIndexerEmitsAddForNewKeyAndRemoveAddForReplacement()
        {
            var lifetimeDefinition = new LifetimeDefinition();
            var map = new ViewableMap<string, string>();
            var events = new List<ViewableMapEvent<string, string>>();

            map.Advise(lifetimeDefinition.Lifetime, events.Add);

            map["bot"] = "zone-a";
            map["bot"] = "zone-b";

            Assert.That(events.Count, Is.EqualTo(3));
            Assert.That(events[0].Change, Is.EqualTo(AddRemove.Add));
            Assert.That(events[0].Key, Is.EqualTo("bot"));
            Assert.That(events[0].Value, Is.EqualTo("zone-a"));
            Assert.That(events[1].Change, Is.EqualTo(AddRemove.Remove));
            Assert.That(events[1].Key, Is.EqualTo("bot"));
            Assert.That(events[1].Value, Is.EqualTo("zone-a"));
            Assert.That(events[2].Change, Is.EqualTo(AddRemove.Add));
            Assert.That(events[2].Key, Is.EqualTo("bot"));
            Assert.That(events[2].Value, Is.EqualTo("zone-b"));

            lifetimeDefinition.Terminate();
        }

        [Test]
        public void ViewableMapViewProcessesAdditionsWithoutRebuildingExistingKeys()
        {
            var lifetimeDefinition = new LifetimeDefinition();
            var map = new ViewableMap<string, string>();
            var handled = new List<(string Key, string Value)>();
            var terminatedKeys = new List<string>();

            map.View(lifetimeDefinition.Lifetime, (itemLifetime, key, value) =>
            {
                handled.Add((key, value));
                itemLifetime.OnTermination(() => terminatedKeys.Add(key));
            });

            map["bot-a"] = "zone-a";
            map["bot-b"] = "zone-b";

            Assert.That(handled, Is.EqualTo(new[]
            {
                ("bot-a", "zone-a"),
                ("bot-b", "zone-b")
            }));
            Assert.That(handled.Count(entry => entry.Key == "bot-a"), Is.EqualTo(1));
            Assert.That(terminatedKeys, Is.Empty);

            lifetimeDefinition.Terminate();

            Assert.That(terminatedKeys, Is.EquivalentTo(new[] { "bot-a", "bot-b" }));
        }

        [Test]
        public void ViewableMapViewReplacementTerminatesOnlyReplacedKey()
        {
            var lifetimeDefinition = new LifetimeDefinition();
            var map = new ViewableMap<string, string>();
            var handled = new List<(string Key, string Value)>();
            var terminatedKeys = new List<string>();

            map.View(lifetimeDefinition.Lifetime, (itemLifetime, key, value) =>
            {
                handled.Add((key, value));
                itemLifetime.OnTermination(() => terminatedKeys.Add(key));
            });

            map["bot-a"] = "zone-a";
            map["bot-b"] = "zone-b";
            terminatedKeys.Clear();

            map["bot-a"] = "zone-new";

            Assert.That(handled, Is.EqualTo(new[]
            {
                ("bot-a", "zone-a"),
                ("bot-b", "zone-b"),
                ("bot-a", "zone-new")
            }));
            Assert.That(terminatedKeys, Is.EqualTo(new[] { "bot-a" }));

            lifetimeDefinition.Terminate();
        }

        [Test]
        public void ViewableMapViewRemoveTerminatesOnlyRemovedKey()
        {
            var lifetimeDefinition = new LifetimeDefinition();
            var map = new ViewableMap<string, string>();
            var terminatedKeys = new List<string>();

            map.View(lifetimeDefinition.Lifetime, (itemLifetime, key, _) =>
            {
                itemLifetime.OnTermination(() => terminatedKeys.Add(key));
            });

            map["bot-a"] = "zone-a";
            map["bot-b"] = "zone-b";
            terminatedKeys.Clear();

            var removed = map.Remove("bot-a");

            Assert.That(removed, Is.True);
            Assert.That(terminatedKeys, Is.EqualTo(new[] { "bot-a" }));

            lifetimeDefinition.Terminate();

            Assert.That(terminatedKeys, Is.EquivalentTo(new[] { "bot-a", "bot-b" }));
        }

        [Test]
        public void ZoneReleaseTerminatesZoneLifetimeAndClearsBlocks()
        {
            var zoneType = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly => assembly.GetType("Game.Gameplay.Zones.Core.Zone"))
                .FirstOrDefault(type => type != null);

            Assert.That(zoneType, Is.Not.Null);

            var rootLifetimeDefinition = new LifetimeDefinition();
            var zoneObject = new GameObject("ZoneReleaseTest");
            var ownerSignTriggerObject = new GameObject("ZoneReleaseTestOwnerSignTrigger");

            try
            {
                var zone = zoneObject.AddComponent(zoneType);
                zoneType
                    .GetField("_ownerSignTriggerObject", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.SetValue(zone, ownerSignTriggerObject);

                zoneType.GetMethod("Initialize")?.Invoke(zone, new object[] { rootLifetimeDefinition.Lifetime });

                var zoneLifetime = (Lifetime)zoneType.GetProperty("ZoneLifetime")?.GetValue(zone);
                Assert.That(zoneLifetime, Is.Not.Null);

                var zoneLifetimeTerminated = false;
                zoneLifetime.OnTermination(() => zoneLifetimeTerminated = true);

                var blocks = zoneType.GetProperty("Blocks")?.GetValue(zone);
                Assert.That(blocks, Is.Not.Null);

                blocks.GetType().GetMethod("Add")?.Invoke(blocks, new object[] { null });

                zoneType.GetMethod("ReleaseZone")?.Invoke(zone, null);

                var blockCount = (int)blocks.GetType().GetProperty("Count")?.GetValue(blocks);
                Assert.That(zoneLifetimeTerminated, Is.True);
                Assert.That(blockCount, Is.EqualTo(0));
            }
            finally
            {
                rootLifetimeDefinition.Terminate();
                Object.DestroyImmediate(zoneObject);
                Object.DestroyImmediate(ownerSignTriggerObject);
            }
        }
    }
}
