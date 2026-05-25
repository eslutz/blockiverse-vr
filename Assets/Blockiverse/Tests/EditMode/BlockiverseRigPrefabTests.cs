using System.Linq;
using Blockiverse.Core;
using Blockiverse.VR;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Blockiverse.Tests.EditMode
{
    public sealed class BlockiverseRigPrefabTests
    {
        [Test]
        public void XrRigPrefabIsWiredForQuestControllerAnchorsAndHaptics()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BlockiverseProject.XrRigPrefabPath);

            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponent<BlockiverseInputRig>(), Is.Not.Null);
            Assert.That(prefab.transform.Find("Camera Offset/Left Controller"), Is.Not.Null);
            Assert.That(prefab.transform.Find("Camera Offset/Right Controller"), Is.Not.Null);

            BlockiverseControllerAnchor[] anchors = prefab
                .GetComponentsInChildren<BlockiverseControllerAnchor>(true);
            BlockiverseControllerHaptics[] haptics = prefab
                .GetComponentsInChildren<BlockiverseControllerHaptics>(true);

            Assert.That(anchors, Has.Length.EqualTo(2));
            Assert.That(haptics, Has.Length.EqualTo(2));
            Assert.That(anchors.Select(anchor => anchor.Role), Is.EquivalentTo(new[]
            {
                BlockiverseControllerRole.Left,
                BlockiverseControllerRole.Right
            }));
            Assert.That(haptics.Select(controllerHaptics => controllerHaptics.Role), Is.EquivalentTo(new[]
            {
                BlockiverseControllerRole.Left,
                BlockiverseControllerRole.Right
            }));

            AssertController(prefab, "Left Controller", BlockiverseControllerRole.Left);
            AssertController(prefab, "Right Controller", BlockiverseControllerRole.Right);
        }

        static void AssertController(GameObject prefab, string controllerName, BlockiverseControllerRole expectedRole)
        {
            Transform controller = prefab.transform.Find($"Camera Offset/{controllerName}");

            Assert.That(controller, Is.Not.Null);
            Assert.That(controller.GetComponent<BlockiverseControllerAnchor>()?.Role, Is.EqualTo(expectedRole));
            Assert.That(controller.GetComponent<BlockiverseControllerHaptics>()?.Role, Is.EqualTo(expectedRole));
        }
    }
}
