---
sidebar_position: 1
---

# Overview

:::warning Legacy

This represents the **legacy** mobile app overview architecture done in .NET MAUI.

:::

The overall architecture of the mobile applications is pretty similar to the
[web clients](../../clients/overview.md) one following a layered architecture:

- State
- Services
- Presentation

Even though the State and Services layers are pretty similar to the web ones the Presentation layer
differs:

## Presentation

The presentation layer is implemented using .NET MAUI for the mobile apps, except for the watchOS
one which uses SwiftUI [see ADR](../../adr/0017-watchOS-use-swift.md)
