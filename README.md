# About FAI
FAI (or FAI lang) stands for **F**unctional **A**legbraic, and **I**nterpreted language, and that's what it strives to be. **FAI is a purely functional language with algebraic syntax.** That said, FAI is not just a functional calculator. While FAI borrows syntax from algebra, it in itself is not algebra, and the user must implement any non-basic algebraic functions. For example, if one wanted to solve a trinomial, they could implement the quadratic formula.

<p align="center"><img src="https://rawgit.com/TheUnlocked/FAI-Language/master/svgs//aaf78c77fd22cac9c585c960351835ef.svg?invert_in_darkmode" align=middle width=145.45792469999998pt height=36.7526973pt/></p>

```fai
plus_minus(a, b) = (a + b | a - b)
quadratic(a, b, c) = plus_minus(-b, sqrt(b^2 - 4*a*c)) / 2*a
```

<p align="center"><img src="https://rawgit.com/TheUnlocked/FAI-Language/master/svgs//969a4afec608b081de4df21cda2729d0.svg?invert_in_darkmode" align=middle width=395.32508564999995pt height=36.7526973pt/></p>

```fai
quadratic(1, -1, -2)
> (2 | -1)
```

### Purpose
Other programming languages, especially functional ones, may look intimidating or confusing to new programmers. For example, this scheme code:

```scheme
(define (factorial x)
	(if (= x 1) 1 (* x (factorial (- x 1)))))
```

A programmer familiar with Scheme (or another LISP-like language) might have no problem reading that, but to someone with no exposure to programming, how would they know what's going on there? Compare that to the FAI code:

```fai
factorial(x) = {1 if x = 1; x * factorial(x - 1) otherwise;
```

That's already much more readable to someone who knows algebra but after being processed through a yet-to-be-written FAI TeXer, that function could be automatically represented like this:

<p align="center"><img src="https://rawgit.com/TheUnlocked/FAI-Language/master/svgs//cd9c1c25bd8c66552c6358ed2b7a70d1.svg?invert_in_darkmode" align=middle width=335.719296pt height=49.315569599999996pt/></p>

Connecting that to the algebraic notation isn't hard, because it _is_ the algebraic notation.

So as a complete answer to what the purpose of FAI is, **FAI exists to serve as a programming language that allows for teaching a functional paradigm within the comfort of algebraic syntax.**

# Specification
A draft specification is currently being worked on at the [wiki page](https://github.com/TheUnlocked/FAI-Language/wiki).

# Plans
For plans with language features, look in the [issues](https://github.com/TheUnlocked/FAI-Language/issues) tab. If you have an idea, feel free to open up a new issue with your proposal.
### Other plans
* Develop a FAI to TeX "TeXer"
* Create a FAI IDE for easier usage
