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
