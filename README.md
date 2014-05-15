Grammar Definition Language
===========================

This is a language that can be used to define the formal grammar for other languages.

The purpose is to allow for conversion between a text in a described language, to an AST representing that text.

The gramdel language must be abled to define itself.

Also, I plan that it will allow the reverse way... that is, converting an AST back to text.
This could be the way to make converters between languages, compilers, decompilers and so on.

2013-08-05
----------

At this time, my research on how to produce a recursive descent parser has led me to the conclusion that it
cannot handle certain cases, most notably recursions at the same position of the input. To solve this problem
the parser must know when a recursion happens. Another problem is that of endless recursions, that is, a condition
that can be represented by endless levels of parsing. This can only be solved by analysing the rule mathematically
and determine if there is a tendency when doing something so many times.

2014-05-15
----------

A recursive descent parser can be built with memoization. Unfortunatelly, there are some very difficult problems to solve, in order to make a correct parsing architecture:

- infinite recursion of parsing productions that accept zero-length (epsilon)
- correct alternative returning order: it is easyer to return them in order of dependency (dependency order)
  - recursive ascent parsers also have ordering problems: they return alternatives as characters arrive (length order)

The tools to make parsers that are sistematically correct just don't exist.
I thought of some possibilities:

- create a language with support to a call-tree, instead of a call-stack
  - each method can return multiple times
  - each return creates a new branch of the caller stack-frame by cloning it
  - a method can chose not to return, killing the call-tree branch
  - a method can await for something, letting other branches to continue execution

This allows for the kind of execution starategy required by a recursive descent parser.
Ordering is granted, by the call-tree. Results are in the same order as the call-tree items.
Calls to parse things at the same position, with the same method can be memoized, if they wish to.

Callers of methods using a call-tree, can break the chain of async execution, by enumerating the results.
When this is done, the caller stalls, until results are available, and in the correct order...
that means that, if the second return is ready, but the first is not, nothing is returned, until
the first return is ready, then it immediatelly returns the first and the second in a row.
When enumerating these methods, a scheduler can be passed in, to allow async execution of the method pieces.

I named these methods as *"poly-methods"*... that is, methods that represent multiple execution paths.
