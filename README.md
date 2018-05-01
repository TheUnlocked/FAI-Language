# About FAI
FAI (or FAI lang) stands for **F**unctional **A**legbraic, and **I**nterpreted language, and that's what it strives to be. **FAI is a purely functional language with algebraic syntax.** That said, FAI is not just a functional calculator. While FAI borrows syntax from algebra, it in itself is not algebra, and the user must implement any non-basic algebraic functions. For example, if one wanted to solve a trinomial, they could implement the quadratic formula.

<div align="center">
	<img
	alt="x = \frac{-b \pm \sqrt{b^{2}-4ac}}{2a}"
	src="https://latex.codecogs.com/gif.latex?x%20%3D%20%5Cfrac%7B-b%20%5Cpm%20%5Csqrt%7Bb%5E%7B2%7D-4ac%7D%7D%7B2a%7D">
</div>

```fai
quadratic(a, b, c) = (-b +- sqrt(b^2 - 4a*c)) / 2a
```

<div align="center">
	<img
	alt="\text{for} \;\; x^2-x-2=0, \;\; x=\frac{-1 \pm \sqrt{1^{2}-4\cdot 1\cdot -2}}{2\cdot 1}=2,-1"
	src="https://latex.codecogs.com/gif.latex?%5Ctext%7Bfor%7D%20%5C%3B%5C%3B%20x%5E2-x-2%3D0%2C%20%5C%3B%5C%3B%20x%3D%5Cfrac%7B-1%20%5Cpm%20%5Csqrt%7B1%5E%7B2%7D-4%5Ccdot%201%5Ccdot%20-2%7D%7D%7B2%5Ccdot%201%7D%3D2%2C-1">
</div>

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

That's already much more readable to someone who knows algebra but after being processed through TeXiFAI, a FAI TeXer (on hold until grammar is finalized), that function could be automatically represented like this:

<div align="center">
	<img
	alt="\text{factorial}(x)= \begin{cases} 1 & \text{if} \; x = 1; \\ x \cdot \text{factorial}(x-1) & \text{otherwise}; \end{cases}" src="https://latex.codecogs.com/gif.latex?%5Ctext%7Bfactorial%7D%28x%29%3D%20%5Cbegin%7Bcases%7D%201%20%26%20%5Ctext%7Bif%7D%20%5C%3B%20x%20%3D%201%3B%20%5C%5C%20x%20%5Ccdot%20%5Ctext%7Bfactorial%7D%28x-1%29%20%26%20%5Ctext%7Botherwise%7D%3B%20%5Cend%7Bcases%7D">
</div>

Connecting that to the algebraic notation isn't hard, because it _is_ the algebraic notation.

So as a complete answer to what the purpose of FAI is, **FAI exists to serve as a programming language that allows for teaching a functional paradigm within the comfort of algebraic syntax.**

# Specification
A draft specification is currently being worked on at the [wiki page](https://github.com/TheUnlocked/FAI-Language/wiki).

# Plans
For plans with language features, look in the [issues](https://github.com/TheUnlocked/FAI-Language/issues) tab. If you have an idea, feel free to open up a new issue with your proposal.
### Other plans
* Finish TeXiFAI, a FAI TeXer
* Create FAIDE, a FAI IDE
