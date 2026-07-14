# What is C#?

## Summary

C# is a statically typed language that runs on a managed runtime. That sentence is useless until you have
seen what happens without one — so this topic starts with a bug that C# makes impossible to write, and
works backwards to the machinery that makes it impossible.

## Learning Objectives

- Write the memory bug C# removes, and say what it costs you when it fires.
- Explain what a program is handed to when you run it, and why that is not the CPU.
- Say what `Managed Code` buys and what it takes away.
- Decide when the trade is wrong for you.

## Why This Topic Matters

Every later question — why a `.dll` runs on Linux, why nothing frees memory, why a type error stops the
build instead of the program — has the same answer. Get it once and a dozen topics stop being surprising.

## Definition

C# is a general-purpose language, compiled to `Intermediate Language` and executed by the
`Common Language Runtime`, which manages memory and enforces `Type Safety` while the program runs.

## Why It Exists

Start with the code, in C:

```c
char* name = malloc(64);
strcpy(name, "Ada");

free(name);                 // done with it

printf("%s\n", name);       // ...one line too late
```

What does this print?

Most days: `Ada`. The memory is freed, but nothing has overwritten it yet, so the bytes are still sitting
there. The program runs. The tests pass. It ships.

Some days: garbage. Some days: **whatever the allocator handed to the next caller** — which, in a web
server, is somebody else's session token.

And it does not crash. That is the part worth sitting with. A bug that crashes is a bug you find. This one
runs perfectly for a year and then prints another user's data into a log.

## Problem It Solves

The bug above has a name — *use-after-free* — and a family: double free, dangling pointer, buffer
overrun, leak. Between them they are the most-exploited class of defect in the history of software.

They all come from one privilege: **you decide when memory is released.**

So C# takes the privilege away. There is no `free`. There is nothing to call too early, nothing to call
twice, and nothing to forget. The code above cannot be written.

Which raises the obvious question, and it is the question this topic is really about:

> If you never free memory, who does?

## Core Mental Model

Something has to be watching. Not a library you call — something *underneath* your program, that knows
which objects are still reachable and which are not.

That something is the `Common Language Runtime`. Your code does not run on the CPU. It runs on the CLR,
and the CLR runs on the CPU:

```text
   your C# source
        │  compiler
        ▼
   Intermediate Language        ← what a .dll actually contains
        │  Common Language Runtime  (Just-In-Time)
        ▼
   machine code                 ← produced at the last possible moment
```

The compiler does **not** produce machine code. It produces `Intermediate Language` — CPU-independent
instructions the runtime understands.

Two things fall out of this immediately.

**Memory can be managed**, because the runtime is standing between your code and the machine. It knows
every reference you hold, so it knows what you can no longer reach — and it can reclaim that.

**The same `.dll` runs on an x64 server and an ARM laptop**, because the machine code did not exist until
the program started. The `Just-In-Time` compiler produced it for the CPU it actually found.

Almost every "how can that possibly work" question about .NET is answered by remembering that last
sentence.

## Core Concepts

Now the words have somewhere to land.

**Managed Code** — code that runs under the CLR's supervision rather than directly on the CPU. You give up
some control; you get back an entire class of bug you can no longer write.

**Garbage Collector** — the part of the runtime that reclaims memory once nothing can reach it. It answers
the question the `free` above asked. It answers it *later than you would have*, and that is the price.

**Type Safety** — a value is only ever used as the kind of thing it is. Checked by the compiler, and
checked again by the runtime. `int x = "hello";` does not fail when it runs. It fails to build.

**Intermediate Language** — the CPU-independent instruction set inside every `.dll`. Not source, not
machine code. The thing the JIT eats.

## Basic Example

```csharp
var name = "Ada";

Console.WriteLine(name);

// There is no line you can add here that frees `name`.
// Not "you shouldn't". You *can't*. The API does not exist.
```

`name` becomes unreachable when the method ends. The Garbage Collector reclaims it at some point after
that — you are not told when, and you are not meant to care.

## Real-World Scenario

An API taking ten thousand requests a minute. Every request allocates: a request object, parsed JSON, a
response.

In C, every one of those is a `free` you must not forget and must not do twice — ten thousand times a
minute, in code five people have touched.

In C# you allocate and stop thinking about it. The GC is optimised for exactly this shape, because most
objects die young: the ones from the request that finished are collected almost for free.

What you *do* think about is the pause. A collection stops your threads. For most services that is
microseconds and nobody notices. For a trading system it might not be — and that is the moment the
trade-off stops being a slogan and becomes an engineering decision.

## Best Practices

- Let the compiler take work off you: nullable reference types, `readonly`, `sealed`. Each one turns a
  run-time possibility into a build error.
- Learn what the standard library already has before writing it. Most of it is there.
- Do not fight the Garbage Collector until you have measured a reason to.

## Common Mistakes

**Calling `GC.Collect()`.** Almost always wrong. It forces a full collection, usually at a worse moment
than the runtime would have chosen — and it hides the real problem, which is nearly always an object being
kept alive by a reference somebody forgot about.

**Thinking `Managed Code` means "no memory problems".** You cannot *corrupt* memory. You can absolutely
*leak* it. An event handler that is never unsubscribed keeps its entire object graph alive forever, and
the Garbage Collector will faithfully preserve every byte — because from where it stands, you can still
reach it.

**Reading `var` as "dynamic".** The type is fixed at compile time. `var` only means "you already said it on
the right; do not make me say it twice."

## Trade-Offs

| You get | You give up |
|---|---|
| No use-after-free, ever | Deciding when memory is released |
| Type errors before the program runs | Some freedom of a dynamic language |
| One binary, many CPUs | A little startup time (JIT) |
| A large library in the box | A large runtime to deploy |

The first row is not a preference. It is a category of security vulnerability you can no longer write, and
it is the reason the language exists. The other three are taste.

:::warning
The GC does not mean "no memory problems". It means "no memory *corruption*". A leak is still yours.
:::

## Further Reading

- The .NET runtime documentation on the CLR execution model.
- The C# language specification, for the questions where "roughly" is not good enough.
