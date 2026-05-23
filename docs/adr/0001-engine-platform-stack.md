# ADR 0001: Engine And Platform Stack

## Status

Accepted

## Decision

Use Unity 6 Personal, C#, URP, OpenXR, Unity OpenXR: Meta, Meta XR Core SDK, Unity Input System, and Netcode for GameObjects.

## Context

The project targets Meta Quest 3 and Quest 3S, uses C#, and needs mature XR, Android, testing, profiling, and CI/CD support.

## Consequences

Unity becomes the primary build and test environment. Core gameplay logic should remain in pure C# assemblies where practical so voxel storage, generation, commands, inventory, crafting, save/load, and networking validation can be tested without VR hardware.
