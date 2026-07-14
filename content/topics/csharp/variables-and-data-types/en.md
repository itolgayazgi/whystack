# Variables and Data Types

## Summary

Every C# variable holds one of two things: the data, or a way to reach the data. Which one decides what
happens when you assign it — and getting that wrong produces the first bug that will genuinely confuse
you. This topic starts with that bug.

## Learning Objectives

- Predict what assignment does, before running it.
- Say what a variable actually holds, for any type.
- Explain why a method you called changed an object you never asked it to change.
- Choose `class` or `struct` for a reason you can state.

## Why This Topic Matters

"I changed the copy and the original changed too" is the first bug that makes a beginner doubt the
language. It is not a bug in the language. It is one arrow, and nobody drew it for them.

## Definition

A variable is a named location holding a value of a declared type. The type is fixed at compile time: the
variable may take a new value, never a new type.

## Why It Exists

Run this in your head. What does it print?

```csharp
var first  = new List<string> { "x" };
var second = first;

second.Add("y");

Console.WriteLine(first.Count);
```

Most people say **1**. `second` is a copy, you added to the copy, so `first` should still have one item.

It prints **2**.

Now the same shape, with an `int`:

```csharp
int a = 1;
int b = a;

b = 2;

Console.WriteLine(a);      // 1 — exactly what you expected.
```

Same syntax. Same assignment. Opposite result.

This is not an inconsistency and it is not something to memorise. It is one rule, applied twice — and
until you see the rule, C# will keep surprising you.

## Problem It Solves

Assignment copies **what is in the variable**.

For the `int`, what is in the variable is `1`. Copy it, and you have two independent ones.

For the `List`, what is in the variable is **not the list**. It is a reference — an arrow, pointing at a
list that lives somewhere else. Copy the arrow and you have two arrows pointing at **one list**.

Nothing unusual happened in the first snippet. Assignment did exactly the same thing both times.

So why does C# have both? Because a single rule would be wrong for one of them.

Copy everything, and passing a `Customer` to a method means copying fifteen fields and a list of orders.
Worse: you could never modify *the* customer, only your own copy of one.

Reference everything, and an `int` in a tight loop pays for an allocation, an indirection and a visit from
the `Garbage Collector` — for four bytes of data.

## Core Mental Model

Two boxes. What is inside them is the whole topic.

```text
Value Type                        Reference Type

  int x = 42                        var a = new Customer()

  ┌────────┐                        ┌────────┐        ┌──────────────┐
  │   42   │                        │  ref ──┼───────▶│  Customer    │
  └────────┘                        └────────┘        │  Name: "Ada" │
   the data itself                   an arrow         └──────────────┘
                                                        the data

  Stack                             Stack               Heap
  gone when the method ends         gone when the       collected once nothing
                                    method ends         can reach it
```

Assignment copies **the box's contents**. For a `Value Type` that is the data. For a `Reference Type` that
is the arrow.

Every "why did the original change" question is that arrow.

## Core Concepts

**Value Type** — `int`, `double`, `bool`, `DateTime`, every `struct`, every `enum`. The variable holds the
data. Assignment copies it. As a local it lives on the `Stack` and disappears when the method returns — the
`Garbage Collector` is not involved at all.

**Reference Type** — `class`, `string`, arrays, `List<T>`, `record`. The variable holds a reference.
Assignment copies the reference. The object lives on the `Heap` and is reclaimed once nothing can reach it.

**`null`** — only a `Reference Type` can be `null`: an arrow pointing at nothing. A `Value Type` always has
a value, which is why `int x;` is zero and not "empty". If you want an `int` that might be missing, say so:
`int?`.

## Basic Example

```csharp
// Value Type: the copy really is a copy.
int a = 1;
int b = a;
b = 2;
// a is still 1.

// Reference Type: the copy is a second arrow to the same object.
var first  = new List<string> { "x" };
var second = first;
second.Add("y");
// first has TWO items — there was only ever one list.
```

## Real-World Scenario

A method takes a `Customer`, validates it, and — helpfully — trims the whitespace from the name first.

The caller never asked for its customer to be modified. But `Customer` is a `Reference Type`, the method
was handed an arrow, and the object it edited is the caller's object.

The name is now trimmed everywhere, in code that never mentions trimming. This is the most common form of
the bug in real systems: one arrow, and a method that assumed it had a copy.

## Best Practices

- Ask what you want assignment to *do* before choosing `class` or `struct`. Identity → class.
  Interchangeable data → struct.
- Keep `struct`s small and immutable.
- Turn on nullable reference types. They turn a run-time `NullReferenceException` into a build error, and
  that trade is always worth taking.
- Do not mutate an object a caller handed you unless the method's name says it will.

## Common Mistakes

**Thinking `string` is a `Value Type`.** It is a `Reference Type`. It only *feels* like a value because it
is immutable — you cannot change it in place, so nobody ever sees a shared modification. The arrow is still
there.

**A mutable `struct`.** Copies are made everywhere: assignment, arguments, `foreach` variables. The
modification lands on a copy and vanishes — silently.

**Reading `null` as "empty".** A `null` list is not an empty list. One has zero items; the other has no
list, and iterating it throws.

## Trade-Offs

| Value Type | Reference Type |
|---|---|
| No allocation, no GC pressure | Allocated on the Heap |
| Copied on every assignment | One reference copied, whatever the size |
| Cannot be `null` unless you ask | Can be `null` — at the worst moment |
| Two variables never affect each other | Two variables can be one object |

The choice is between **identity** and **independence**. Reach for a `Reference Type` when two parts of the
program genuinely mean the same thing; reach for a `Value Type` when they merely mean the same amount.

:::note
`string` is a `Reference Type` that behaves like a value. That is immutability doing the work — not a
special case in the language.
:::
