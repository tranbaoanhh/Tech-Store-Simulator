using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace TechStoreSimulator.Tests
{
    public class GameManagerTests
    {
        GameObject owner;
        GameManager manager;
        readonly List<int> moneyEvents = new List<int>();
        readonly List<int> purchaseEvents = new List<int>();
        readonly List<string> statusEvents = new List<string>();

        [SetUp]
        public void SetUp()
        {
            // Unity's == also recognizes destroyed native objects (fake-null).
            // Still reject a live singleton; never destroy scene-owned objects.
            Assert.That(GameManager.Instance == null, Is.True,
                "Run in Edit Mode without an existing initialized GameManager.");
            owner = new GameObject("GameManager test fixture");
            owner.hideFlags = HideFlags.HideAndDontSave;
            manager = owner.AddComponent<GameManager>();
            // Ordinary MonoBehaviours do not run their normal runtime lifecycle
            // in Edit Mode. Pair explicit Awake with explicit OnDestroy below.
            if (!ReferenceEquals(GameManager.Instance, manager))
            {
                MethodInfo awake = typeof(GameManager).GetMethod("Awake",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(awake, Is.Not.Null);
                awake.Invoke(manager, null);
            }
            Assert.That(ReferenceEquals(GameManager.Instance, manager), Is.True,
                "Awake must register the fixture's GameManager.");
            manager.MoneyChanged += moneyEvents.Add;
            manager.ItemsPurchasedChanged += purchaseEvents.Add;
            manager.StatusChanged += statusEvents.Add;
        }

        [TearDown]
        public void TearDown()
        {
            bool createdOwner = !ReferenceEquals(owner, null);
            try
            {
                if (!ReferenceEquals(manager, null))
                {
                    manager.MoneyChanged -= moneyEvents.Add;
                    manager.ItemsPurchasedChanged -= purchaseEvents.Add;
                    manager.StatusChanged -= statusEvents.Add;
                }
                if (manager != null)
                {
                    MethodInfo onDestroy = typeof(GameManager).GetMethod("OnDestroy",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    Assert.That(onDestroy, Is.Not.Null);
                    onDestroy.Invoke(manager, null);
                }
                if (createdOwner)
                {
                    // This checks the cleanup method's effect, not Unity's callback
                    // delivery. Automatic delivery is covered by the Play Mode test.
                    Assert.That(ReferenceEquals(GameManager.Instance, null), Is.True,
                        "OnDestroy must release the fixture's managed singleton reference.");
                }
            }
            finally
            {
                if (owner != null) Object.DestroyImmediate(owner);
                owner = null;
                manager = null;
                ClearEvents();
            }
        }

        void ClearEvents()
        {
            moneyEvents.Clear();
            purchaseEvents.Clear();
            statusEvents.Clear();
        }

        void AssertEvents(int[] money, int[] purchases, string[] statuses)
        {
            CollectionAssert.AreEqual(money, moneyEvents);
            CollectionAssert.AreEqual(purchases, purchaseEvents);
            CollectionAssert.AreEqual(statuses, statusEvents);
        }

        [TestCase(500000, 3)]
        [TestCase(0, 0)]
        [TestCase(int.MaxValue, int.MaxValue)]
        public void RestoreValidState_UpdatesValuesAndPublishesNewValues(int money, int purchases)
        {
            // Seed both fields so every case changes both values.
            Assert.That(manager.TryRestoreState(1, 1), Is.True);
            ClearEvents();

            Assert.That(manager.TryRestoreState(money, purchases), Is.True);

            Assert.That(manager.Money, Is.EqualTo(money));
            Assert.That(manager.ItemsPurchased, Is.EqualTo(purchases));
            AssertEvents(new[] { money }, new[] { purchases }, new string[0]);
            Assert.That(manager.Status, Is.EqualTo("Da khoi phuc tien va so luot mua."));
        }

        [TestCase(-1, 3)]
        [TestCase(500000, -1)]
        [TestCase(-1, -1)]
        [TestCase(int.MinValue, 0)]
        public void RestoreNegativeState_RejectsWithoutChangingFinancialState(int money, int purchases)
        {
            manager.TryRestoreState(500000, 3);
            ClearEvents();

            Assert.That(manager.TryRestoreState(money, purchases), Is.False);

            Assert.That(manager.Money, Is.EqualTo(500000));
            Assert.That(manager.ItemsPurchased, Is.EqualTo(3));
            AssertEvents(new int[0], new int[0], new[] { "Du lieu khoi phuc khong hop le!" });
            Assert.That(manager.Status, Is.EqualTo(statusEvents[0]));
        }

        [Test]
        public void RestoreSameState_DoesNotPublishDuplicateEvents()
        {
            manager.TryRestoreState(500000, 3);
            ClearEvents();

            Assert.That(manager.TryRestoreState(500000, 3), Is.True);

            AssertEvents(new int[0], new int[0], new string[0]);
        }

        [TestCase(600000, 3, true, false)]
        [TestCase(500000, 4, false, true)]
        public void RestoreOneChangedField_OnlyPublishesThatField(
            int money, int purchases, bool moneyChanged, bool purchasesChanged)
        {
            manager.TryRestoreState(500000, 3);
            ClearEvents();

            Assert.That(manager.TryRestoreState(money, purchases), Is.True);

            AssertEvents(moneyChanged ? new[] { money } : new int[0],
                purchasesChanged ? new[] { purchases } : new int[0], new string[0]);
        }

        [TestCase(350000, 150000)]
        [TestCase(500000, 0)]
        public void SuccessfulPurchase_PublishesBalanceCountAndStatus(int price, int remaining)
        {
            manager.TryRestoreState(500000, 3);
            ClearEvents();

            Assert.That(manager.TrySpend(price), Is.True);

            Assert.That(manager.Money, Is.EqualTo(remaining));
            Assert.That(manager.ItemsPurchased, Is.EqualTo(4));
            AssertEvents(new[] { remaining }, new[] { 4 }, new[] { "Mua thanh cong!" });
            Assert.That(manager.Status, Is.EqualTo(statusEvents[0]));
        }

        [TestCase(0, "Gia mua phai lon hon 0!")]
        [TestCase(-1, "Gia mua phai lon hon 0!")]
        [TestCase(int.MinValue, "Gia mua phai lon hon 0!")]
        [TestCase(500001, "Khong du tien!")]
        public void FailedPurchase_OnlyPublishesChangedStatus(int price, string expectedStatus)
        {
            manager.TryRestoreState(500000, 3);
            ClearEvents();

            Assert.That(manager.TrySpend(price), Is.False);
            Assert.That(manager.TrySpend(price), Is.False, "Repeated failure must not duplicate status events.");

            Assert.That(manager.Money, Is.EqualTo(500000));
            Assert.That(manager.ItemsPurchased, Is.EqualTo(3));
            Assert.That(manager.Status, Is.EqualTo(expectedStatus));
            AssertEvents(new int[0], new int[0], new[] { expectedStatus });
        }

        [Test]
        public void PurchaseAtMaximumCount_RejectsWithoutOverflow()
        {
            manager.TryRestoreState(500000, int.MaxValue);
            ClearEvents();

            Assert.That(manager.TrySpend(1), Is.False);

            Assert.That(manager.Money, Is.EqualTo(500000));
            Assert.That(manager.ItemsPurchased, Is.EqualTo(int.MaxValue));
            AssertEvents(new int[0], new int[0], new[] { "Da dat gioi han so luot mua!" });
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Events_ObserveFullyUpdatedState(bool restore)
        {
            manager.TryRestoreState(500000, 3);
            int expectedMoney = restore ? 700000 : 400000;
            int expectedPurchases = restore ? 5 : 4;
            string expectedStatus = restore ? "Da khoi phuc tien va so luot mua." : "Mua thanh cong!";
            if (restore) manager.TrySpend(1); // Ensure the restoration also changes status.
            int callbacks = 0;
            System.Action checkState = () =>
            {
                Assert.That(manager.Money, Is.EqualTo(expectedMoney));
                Assert.That(manager.ItemsPurchased, Is.EqualTo(expectedPurchases));
                Assert.That(manager.Status, Is.EqualTo(expectedStatus));
                callbacks++;
            };
            manager.MoneyChanged += value => checkState();
            manager.ItemsPurchasedChanged += value => checkState();
            manager.StatusChanged += value => checkState();

            bool result = restore ? manager.TryRestoreState(700000, 5) : manager.TrySpend(100000);

            Assert.That(result, Is.True);
            Assert.That(callbacks, Is.EqualTo(3));
        }
    }
}
