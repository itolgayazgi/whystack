# Variables and Data Types

## Summary

Every C# variable has a type, and that type decides one thing above all others: whether the variable
holds the data itself or a reference to it. That single distinction — Value Type versus Reference Type
— explains most of the surprises beginners meet, and a fair number that catch experienced developers
too.

## Learning Objectives

- Say what a variable actually holds.
- Distinguish a Value Type from a Reference Type, and predict what assignment does to each.
- Explain where each one lives, and who cleans it up.
- Recognise when `null` is possible and when the compiler has ruled it out.

## Why This Topic Matters

"I changed the copy and the original changed too" is one of the first genuinely confusing bugs anybody
writes. It is not a bug in the language. It is the Value Type / Reference Type distinction, met for the
first time, without a model to explain it.

## Definition

A variable is a named location holding a value of a declared type. In C# the type is fixed at compile
time and cannot change: the variable may take a new value, never a new type.

## Why It Exists

The distinction exists because two different things are cheap in two different situations.

An `int` is four bytes. Copying it is nearly free, and giving every one of them an identity — an
address, a lifetime, a collector that must track it — would cost far more than the data itself.

A `Customer` with fifteen fields and a list of orders is not cheap to copy, and usually you do not want
to: two parts of the program are meant to be looking at *the same customer*, not at two customers that
happen to agree.

So C# offers both. A Value Type is the data. A Reference Type is a way to reach the data.

## Problem It Solves

Without the distinction you would pick one behaviour for everything, and both choices are bad.

Copy everything, and passing an object to a method becomes expensive and loses identity — you could
never modify "the" order, only your own copy of it.

Reference everything, and an `int` in a tight loop pays for an allocation, an indirection and a visit
from the Garbage Collector, for four bytes of data.

## Core Mental Model

Two boxes, and what is inside them:

```text
Value Type                     Reference Type

  int x = 42                     var a = new Customer()

  ┌────────┐                     ┌────────┐        ┌──────────────┐
  │   42   │                     │  ref ──┼───────▶│  Customer    │
  └────────┘                     └────────┘        │  Name: "Ada" │
   the data                       a reference      └──────────────┘
                                                     the data

  Stack                          Stack               Heap
  gone when the method ends      gone when the       collected when nothing
                                 method ends         can reach it any more
```

Assignment always copies **what is in the box**. For a Value Type, that is the data. For a Reference
Type, that is the arrow — and both variables now point at the same object.

That is the whole thing. Every "why did the original change" question is that arrow.

## Core Concepts

**Value Type.** `int`, `double`, `bool`, `char`, `DateTime`, any `struct`, any `enum`. The variable
holds the data. Assignment copies it. It lives on the Stack when it is a local — and it disappears when
the method returns, without the Garbage Collector being involved at all.

**Reference Type.** `class`, `string`, arrays, `List<T>`, `record` (unless declared as a `record
struct`). The variable holds a reference. Assignment copies the reference. The object lives on the Heap
and is reclaimed by the Garbage Collector once nothing can reach it.

**`null`.** Only a Reference Type can be `null` — a reference to nothing. A Value Type always has a
value, which is why `int x;` is zero and not "empty". If you want an `int` that might be missing, say
so: `int?`.

**Type inference.** `var` asks the compiler to write the type for you. It is still static: the type is
fixed at compile time, and it is not `dynamic`.

## Basic Example

```csharp
// Value Type: the copy is a copy.
int a = 1;
int b = a;
b = 2;
// a is still 1.

// Reference Type: the copy is a second arrow to the same object.
var first = new List<string> { "x" };
var second = first;
second.Add("y");
// first now has TWO items — because there was only ever one list.
```

Nothing unusual happened in the second block. `second = first` copied what was in the box, exactly as
it did for the `int`. What was in the box was a reference.

## Real-World Scenario

A method takes a `Customer`, "validates" it, and — helpfully — trims the whitespace from the name
before checking it.

The caller never asked for its customer to be modified. But `Customer` is a Reference Type, the method
was handed a reference, and the object it edited is the caller's object. The name is now trimmed
everywhere, in code that never mentioned trimming.

This is the most common form of the bug in real systems, and it is not exotic: it is one arrow,
followed by a method that assumed it had a copy.

## Best Practices

- Ask what you want to happen on assignment *before* choosing `class` or `struct`. Identity → class.
  Interchangeable data → struct.
- Keep `struct`s small and immutable. A large mutable struct gets copied on every assignment and every
  method call, and each copy can be modified independently — which is almost never what anybody meant.
- Turn on nullable reference types and take the warnings seriously. They turn a run-time
  `NullReferenceException` into a compile-time error, and that trade is always worth taking.
- Do not mutate an object a caller handed you unless the method's name says it will.

## Common Mistakes

**Thinking `string` behaves like a Value Type.** It is a Reference Type. It only *feels* like a value
because it is immutable — you cannot change it in place, so nobody ever sees a shared modification. The
arrow is still there.

**A mutable `struct`.** Every copy can be changed independently, and copies are made everywhere —
assignment, arguments, `foreach` iteration variables. The modification lands on a copy and vanishes,
and it does so silently.

**Assuming `null` means "empty".** A `null` list is not an empty list. One of them has zero items; the
other has no list, and iterating it throws.

**Believing `var` hides the type.** It does not; the type is still fixed and still checked. It only
hides the type *from the reader* — which is why an unclear `var` is a readability problem, not a
correctness one.

## Trade-Offs

| Value Type | Reference Type |
|---|---|
| No allocation, no Garbage Collector pressure | Allocated on the Heap; collected later |
| Copied on every assignment — cheap for 4 bytes, not for 400 | Copying is one reference, whatever the object's size |
| Cannot be `null` unless you ask (`int?`) | Can be `null`, and will be, at the worst moment |
| Two variables can never affect each other | Two variables can be the same object — which is often the point |

The trade is between *identity* and *independence*, and neither is the right default. Reach for a
Reference Type when two parts of the program genuinely mean the same thing; reach for a Value Type when
they merely mean the same amount.
