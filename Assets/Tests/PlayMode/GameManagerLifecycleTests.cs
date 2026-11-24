using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace TechStoreSimulator.Tests
{
    public class GameManagerLifecycleTests
    {
        GameObject owner;
        GameObject duplicate;

        [UnityTest]
        public IEnumerator RuntimeLifecycle_DestroyDuplicatePreservesInstance_DestroyOwnerClearsInstance()
        {
            // An asmdef cannot reference the predefined Assembly-CSharp assembly.
            // Reflection keeps the production assembly layout unchanged.
            Type managerType = Type.GetType("TechStoreSimulator.GameManager, Assembly-CSharp", true);
            PropertyInfo instance = managerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            Assert.That(instance, Is.Not.Null);
            Assert.That((Object)instance.GetValue(null) == null, Is.True,
                "Run this test in the Test Runner's isolated Play Mode scene.");

            owner = new GameObject("GameManager lifecycle owner");
            Component manager = owner.AddComponent(managerType);
            // No manual Awake or OnDestroy: Unity must deliver both callbacks.
            Assert.That(ReferenceEquals(instance.GetValue(null), manager), Is.True);

            duplicate = new GameObject("GameManager lifecycle duplicate");
            duplicate.AddComponent(managerType);
            yield return null;

            Assert.That(duplicate == null, Is.True, "Awake must destroy the duplicate.");
            Assert.That(ReferenceEquals(instance.GetValue(null), manager), Is.True,
                "Destroying the duplicate must preserve the owner singleton.");

            Object.Destroy(owner);
            yield return null;

            Assert.That(owner == null, Is.True, "Unity must destroy the native object.");
            Assert.That(ReferenceEquals(instance.GetValue(null), null), Is.True,
                "Runtime OnDestroy must clear the managed singleton, not leave fake-null.");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (duplicate != null) Object.Destroy(duplicate);
            if (owner != null) Object.Destroy(owner);
            yield return null;
            duplicate = null;
            owner = null;
        }
    }
}
