# What is C#?

## Summary

C# is a statically typed, object-oriented language that runs on .NET. You write C#, the compiler turns
it into Intermediate Language, and the Common Language Runtime turns that into machine code while the
program runs. Understanding those three steps explains most of what is otherwise mysterious about the
language.

## Learning Objectives

- Explain what C# compiles to, and what actually executes.
- Describe the role of the Common Language Runtime.
- Say why Managed Code exists and what it buys you.
- Recognise which problems C# was designed to remove, and which it was not.

## Why This Topic Matters

Nearly every later confusion — why a `.dll` runs on Linux, why memory is not freed by hand, why a type
error is caught before the program starts — traces back to what happens between the code you write and
the CPU that runs it. Get this once and a dozen other topics stop being surprising.

## Definition

C# is a general-purpose programming language, compiled to Intermediate Language and executed by the
Common Language Runtime. It is statically typed: the type of every expression is known and checked
before the program runs.

## Why It Exists

C# was created because the alternatives forced an unwelcome trade.

C and C++ gave you speed and control, and made you responsible for memory — which meant an entire
category of security bugs was yours to prevent, forever, in every line. The languages that took that
burden away gave up performance and, often, static types.

C# was designed to take the memory-management burden without giving up speed or type checking. That is
the deal, and it is worth naming because everything else follows from it.

## Problem It Solves

The concrete problem is *use-after-free and its family*: dangling pointers, double frees, leaks, buffer
overruns. These are not exotic. They are the most-exploited class of bug in the history of software.

C# removes them by not letting you free memory at all. The Garbage Collector reclaims what the program
can no longer reach. You lose direct control of when that happens; you gain the guarantee that it
cannot happen wrongly.

## Historical Context

C# appeared in 2000, from Microsoft, alongside .NET. Java had already shown that a managed runtime with
a Garbage Collector was viable for serious work. C# started from that premise and has since absorbed
ideas from functional languages — pattern matching, records, immutability by default in places — while
staying a language a C or Java programmer can read on the first day.

## Core Mental Model

Hold three layers in your head, in order:

```text
C# source  ──compiler──▶  Intermediate Language  ──JIT──▶  machine code
   (.cs)                        (.dll)                     (what the CPU runs)
```

The compiler does **not** produce machine code. It produces Intermediate Language, which is
CPU-independent. That is why the same `.dll` runs on an x64 server and an ARM laptop: the machine code
is produced at the last possible moment, by the Just-In-Time compiler, for the CPU it actually found.

Almost every "how can that possibly work" question about .NET is answered by remembering that the
machine code did not exist until the program started.

## Core Concepts

**Static typing.** Every expression has a type the compiler knows. `int x = "hello";` does not fail at
run time — it fails to build. The class of bug is gone before the program exists.

**Managed Code.** Your code runs under the Common Language Runtime rather than directly on the CPU. The
runtime provides memory management, Type Safety and exception handling. You give up some control; you
get back a large category of bugs you can no longer write.

**Garbage Collection.** Memory is reclaimed when it is unreachable, not when you say so. This is the
single biggest difference from C++, and the source of both C#'s safety and its occasional latency
surprises.

**A large standard library.** Collections, HTTP, JSON, cryptography, asynchrony — in the box. Much of
becoming productive in C# is learning what is already there.

## Basic Example

```csharp
// The type is checked at compile time. The memory is managed at run time.
// Neither of those sentences is true of C.
var greeting = "Hello";
var length = greeting.Length;   // int, and the compiler knows it

Console.WriteLine($"{greeting} has {length} characters.");
```

`greeting` is a `string` — the compiler infers it, it does not guess. `var` is not dynamic typing; the
type is fixed at compile time and cannot change afterwards. And nothing here frees the string: it
becomes unreachable when the method ends, and the Garbage Collector deals with it later.

## Real-World Scenario

An API receives ten thousand requests a minute. Each one allocates request objects, parses JSON, builds
a response.

In C, every one of those allocations is a `free()` you must not forget and must not do twice. In C#,
you allocate and stop thinking about it. The Garbage Collector reclaims the short-lived objects
cheaply, because it is optimised for exactly this shape: most objects die young.

What you *do* think about is the pauses. A collection stops your threads. For most services that is
microseconds and nobody notices. For a trading system it might not be, and that is where the trade-off
becomes a real engineering decision rather than a slogan.

## Best Practices

- Prefer explicit types over `var` when the type is not obvious from the right-hand side. `var` is for
  removing noise, not for hiding information.
- Let the compiler help you. Nullable reference types, `readonly`, `sealed` — each one turns a run-time
  possibility into a compile-time error.
- Learn what the standard library already contains before writing it. Most of it is there.
- Do not fight the Garbage Collector until you have measured a reason to.

## Common Mistakes

**Believing `var` means "dynamic".** It does not. The type is fixed at compile time; `var` only means
"you already said it on the right, do not make me say it twice".

**Calling `GC.Collect()`.** Almost always wrong. It forces a full collection, usually at a worse moment
than the runtime would have chosen, and hides the real problem — which is usually an object being kept
alive by a reference somebody forgot about.

**Assuming Managed Code means "no memory problems".** You cannot corrupt memory. You can absolutely
leak it — an event handler that is never unsubscribed keeps its whole object graph alive forever, and
the Garbage Collector will faithfully preserve every byte of it.

## Trade-Offs

| You get | You give up |
|---|---|
| No use-after-free, no double free, no buffer overrun | Deterministic control of when memory is released |
| Type errors caught before the program runs | Some of the freedom of a dynamic language |
| The same binary runs on many CPUs | A little startup time, while Just-In-Time compilation happens |
| A large library in the box | A large runtime to deploy alongside your code |

The row that matters most is the first. Every other row is a preference; that one is a category of
security vulnerability you can no longer write, and it is the reason the language exists.

## Further Reading

- The .NET runtime documentation on the Common Language Runtime execution model.
- The C# language specification, for the questions where "roughly" is not good enough.
