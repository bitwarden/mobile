---
sidebar_position: 4
---

# .NET MAUI (legacy)

:::warning Legacy

Getting started the **legacy** mobile app done in .NET MAUI.

:::

## Configure Git blame

We recommend that you configure git to ignore the Prettier revision:

```bash
git config blame.ignoreRevsFile .git-blame-ignore-revs
```

## Android Development

See the [Android Mobile app](./android/index.md) page to set up an Android development environment.

## iOS Development

<Bitwarden>

See the [iOS Mobile app](./ios/index.mdx) page to set up an iOS development environment.

</Bitwarden>

<Community>

Unfortunately, iOS development requires provisioning profiles and other capabilities only available
to internal team members. We do not have any documentation for community developers at this time.

</Community>

## watchOS Development

<Bitwarden>

See the [watchOS app](./watchos) page to set up an watchOS development environment.

</Bitwarden>

<Community>

Unfortunately, watchOS development requires provisioning profiles and other capabilities only
available to internal team members. We do not have any documentation for community developers at
this time.

</Community>

## Unit tests

:::info TL;DR;

In order to run unit tests add the argument `/p:CustomConstants=UT` on the `dotnet` command for
building/running. To work on Unit testing or use a Test runner uncomment the `CustomConstants` line
on the `Directory.Build.props`

:::

Given that the `Core.csproj` is a MAUI project with `net8.0-android;net8.0-ios` target frameworks
and we need `net8.0` for the tests we need a way to add that. The `Core.Test.csproj` has `net8.0` as
a target so by adding the the argument `/p:CustomConstants=UT` we add `UT` as a constant to use in
the projects. With that in place the next things happen:

- `UT` is added as a constant to use by precompiler directives
- `Core.csproj` is changed to add `net8.0` as a target framework for unit tests
- `FFImageLoading` is removed as a reference given that it doesn't support `net8.0`. Because of
  this, now we have a wrapped `CachedImage` that uses the library one if it's not `UT` and a custom
  one with NOOP implementation for `UT`

So if one wants to build the test project, one needs to go to `test/Core.Test` and run:

```bash
dotnet build -f net8.0 /p:CustomConstants=UT
```

and to run the tests go to the same folder and run:

```bash
dotnet test -f net8.0 /p:CustomConstants=UT
```

Finally, when working on the `Core.Test` project or when wanting to use a Test runner, go to the
`Directory.Build.props` (located in the root) and uncomment the line referencing `CustomConstants`
so that everything is loaded accordingly in the project. Because of some issues, the referenced
projects, e.g. `Core`, are only included when the `UT` constant is in place. By uncommenting this
line the projects will be referenced and one can work on that project or run the tests from a Test
runner.

## Custom constants

There are custom constants to be used by the parameter `/p:CustomConstants={Value}` when
building/running/releasing:

- `FDROID`: This is used to indicate that it's and F-Droid build/release
  ([want to know more?](./android/index.md#f-droid))
- `UT`: This is used when building/running the test projects or when working on one of them
  ([want to know more?](#unit-tests))

These constants are added to the defined ones, so anyone can use them in the code with precompiler
directives.
