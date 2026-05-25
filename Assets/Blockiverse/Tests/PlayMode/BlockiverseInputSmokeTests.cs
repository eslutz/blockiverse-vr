using System.Collections;
using Blockiverse.VR;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Blockiverse.Tests.PlayMode
{
    public sealed class BlockiverseInputSmokeTests
    {
        const string BootSceneName = "Boot";

        [UnityTest]
        public IEnumerator BootSceneEnablesInputRig()
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(BootSceneName, LoadSceneMode.Single);
            while (!operation.isDone)
                yield return null;

            BlockiverseInputRig inputRig = Object.FindFirstObjectByType<BlockiverseInputRig>();
            Assert.That(inputRig, Is.Not.Null);
            Assert.That(inputRig.InputActions, Is.Not.Null);
            Assert.That(inputRig.InputActions.enabled, Is.True);
        }

        [UnityTest]
        public IEnumerator BootSceneHasTrackedControllerAnchors()
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(BootSceneName, LoadSceneMode.Single);
            while (!operation.isDone)
                yield return null;

            BlockiverseControllerAnchor[] anchors = Object.FindObjectsByType<BlockiverseControllerAnchor>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            Assert.That(anchors, Has.Length.EqualTo(2));
            Assert.That(anchors, Has.Some.Matches<BlockiverseControllerAnchor>(anchor => anchor.Role == BlockiverseControllerRole.Left));
            Assert.That(anchors, Has.Some.Matches<BlockiverseControllerAnchor>(anchor => anchor.Role == BlockiverseControllerRole.Right));
        }
    }
}
