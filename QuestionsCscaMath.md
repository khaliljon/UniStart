**Chapter 1: Sets**

**I. Definition of a Set**

**Definition 1:** The objects under study are called **elements**. A whole made of some elements is called a **set**.

**Example:** {Apple, Banana, Watermelon} is a set. Apple, Banana, and Watermelon are elements of this set. {x | -1 < x < 3} is also a set. -1/2, 0, 2/3, 1 are elements of this set.

We use capital letters A, B, C, ... for sets, and small letters a, b, c, ... for elements. For example, A = {Apple, Banana, Watermelon}, b = Apple.

A set has three features:
1. Elements are **determinate**.
   Example: "Numbers >2 and <3" form a set. "Beautiful people" do not form a set.
2. Elements are **different** from each other.
   Example: Write {1, 2, 4, 6}, not {1, 2, 4, 4, 6}.
3. Elements have **no order**.
   Example: {1, 2, 4, 6} = {2, 1, 6, 4}.

We use these symbols:
- **N** = set of natural numbers: 0, 1, 2, 3, ...
- **Z** = set of integers: ..., -3, -2, -1, 0, 1, 2, 3, ...
- **Z^+** = set of positive integers: 1, 2, 3, ...
- **Q** = set of rational numbers (integers, finite decimals, repeating decimals). Examples: 0.2, -3.5, 1/2, -2/3.
- **R** = set of real numbers (rational + irrational numbers like π, e, √2).

**II. Relationship Between Sets and Elements**

**Definition 2:** If \(a\) is in set \(A\), we say \(a\) **belongs to** \(A\), written \(a \in A\). If \(a\) is not in \(A\), we say \(a\) **does not belong to** \(A\), written \(a \notin A\).

**Example:** Let A = {x | -1 < x ≤ 4}. Then 0 ∈ A, 1/2 ∈ A, -2 ∉ A, 5 ∉ A.
Let A = {x | x² - 4 = 0}. Then 2 ∈ A, -2 ∈ A, 3 ∉ A.

**Summary**
1. Objects = elements. Group of elements = set. Use capital letters for sets, small letters for elements.
2. Set features: determinate, elements are different, no order.
3. Special sets: N, Z, Z^+, Q, R.
4. Belongs to: ∈. Does not belong to: ∉.

**Exercises 1.1**

1. Fill with "∈" or "∉".
   (1) 5 _ N, -3 _ N, 5.3 _ N, 0.25 _ N, √2/3 _ N.
   (2) 8 _ Z, 3/2 _ Z, 3 _ Z, -8 _ Z, √4 _ Z, √3 _ Z, (√5)² _ Z, π/3 _ Z.
   (3) 3.14 _ Q, 2/5 _ Q, -7/4 _ Q, 3^0 _ Q, -7 _ Q, √16 _ Q, √3 _ Q, (√3)² _ Q, -3π _ Q.
   (4) 2 _ R, 2/5 _ R, -8/3 _ R, 0.56 _ R, -5 _ R, -√4 _ R, √5 _ R, (√7)² _ R, π/5 _ R.

2. Let A = {x | -5 < x ≤ 5}. Which is correct? ( )
   A. 5/3 ∉ A
   B. -7/3 ∉ A
   C. 4.3 ∈ A
   D. -5 ∈ A

3. Let B = {x ∈ Z | -4 ≤ x < 2}. Which is correct? ( )
   A. -4 ∉ B
   B. -2.5 ∉ B
   C. 2 ∈ B
   D. 1.5 ∈ B

4. {x ∈ Z^+ | x ≤ 4} = ( )
   A. {0,1,2,3,4}
   B. {1,2,3,4}
   C. {0,1,2,3,4,5}
   D. {1,2,3,4,5}

5. Which statement is correct? ( )
   A. If a ∈ N, b ∈ N, then a-b ∈ N
   B. If a ∈ Z^+, then a ∈ Q
   C. If a ≥ 0, then a ∈ N
   D. If a ∈ Z, then a ∉ Q

**1.2 Relationships Between Sets**

**Definition 1:** Two sets A and B are **equal** (A = B) if they have the same elements.
**Example:** {-1,3,6} = {3,-1,6} = {6,-1,3}.

**Definition 2:** Set A is a **subset** of B (A ⊆ B) if every element of A is also in B.
**Example:** {1,3,4} ⊆ {1,3,4,5}, {2,3} ⊆ {2,3}.
**Note:** Every set is a subset of itself: A ⊆ A.
If A ⊆ B and B ⊆ A, then A = B.

**Definition 3:** Set A is a **proper subset** of B (A ⊂ B) if A ⊆ B but A ≠ B.
**Example:** {1,3,4} ⊂ {1,3,4,5}.

**Definition 4:** Set A is **not a subset** of B (A ⊄ B) if there is at least one element in A that is not in B.
**Example:** {1,3,4} ⊄ {1,3,5,6}.

**Example 1:** Let A = {-1,2,4,5,7,8}, B = {-1,4}, C = {-1,4}, D = {2,7,8}, E = {1,9}. Then:
- B ⊆ A, C ⊆ A, D ⊆ A (B, C, D are subsets of A)
- B ⊂ A, C ⊂ A, D ⊂ A (B, C, D are proper subsets of A)
- B ⊆ C or C ⊆ B (B is subset of C or C is subset of B)
- B = C (B and C are equal)
- E ⊄ A (E is not a subset of A)

**Note 2:** The **empty set** (∅) has no elements. The empty set is a subset of every set: ∅ ⊆ A. If A ≠ ∅, then ∅ is a proper subset: ∅ ⊂ A.
**Example:** {x ∈ R | x² + 1 = 0} = ∅.

**Example 2:** Find all subsets of {a,b}. Which are proper subsets?
**Solution:** All subsets: ∅, {a}, {b}, {a,b}. Proper subsets: ∅, {a}, {b}.

**Example 3:** Find all subsets of {a,b,c}. Which are proper subsets?
**Solution:** All subsets: ∅, {a}, {b}, {c}, {a,b}, {a,c}, {b,c}, {a,b,c}. Proper subsets: ∅, {a}, {b}, {c}, {a,b}, {a,c}, {b,c}.

**Note 3:** A set with n elements has 2^n subsets and 2^n - 1 proper subsets.
**Example:** {1,3,5,7,9} has 5 elements, so it has 2^5 = 32 subsets and 2^5 - 1 = 31 proper subsets.

**Summary**
1. A = B ⇔ same elements.
2. A ⊆ B ⇔ every element of A is in B. (A ⊆ A always true)
3. A ⊂ B ⇔ A ⊆ B and A ≠ B.
4. A ⊄ B ⇔ at least one element of A is not in B.
5. If A ⊆ B and B ⊆ A, then A = B.
6. For any set A: ∅ ⊆ A. If A ≠ ∅, then ∅ ⊂ A.

**Exercises 1.2**

1. Fill with "∈", "∉", "⊆", "⊂", "⊄", or "=".
   (1) 3 _ {-3,-2,1,2,3}
   (2) {1,2} _ {-3,-2,1,2,3}
   (3) {x | -2<x<2} _ {x | -4<x≤4}
   (4) ∅ _ {x | x³=8}
   (5) ∅ _ {x | x²+1=0, x∈R}
   (6) 3 _ {x | x<3}
   (7) {1,3} _ {1,2,4}
   (8) {x | 2≤x≤4} _ {x | 0<x<3}
   (9) Z^+ _ N _ Z _ Q _ R

2. Which is correct? ( )
   A. 2 ⊆ {x | x>0}
   B. {2} ∈ {x | -2<x<4}
   C. 3 ∉ {x | -3≤x<3}
   D. ∅ = {0}

3. M = {2}, N = {0,2,4}. Which is correct? ( )
   A. M < N
   B. M ∈ N
   C. M ⊆ N
   D. N ⊆ M

4. Which pair M and N are equal? ( )
   A. M = {π}, N = {3.14}
   B. M = {2,3}, N = {(2,3)}
   C. M = {x∈N | -1<x≤1}, N = {1}
   D. M = {1,2,3}, N = {3,1,√4}

5. List all subsets of {1,3,5,7}. Which are proper subsets?

**1.3 Operations on Sets**

**I. Union of Sets**

**Definition 1:** The **union** of sets A and B (A ∪ B) is the set of all elements that are in A or in B.
A ∪ B = {x | x ∈ A or x ∈ B}

**Example 1:** A = {1,2,4,7}, B = {3,2,8,7}. Then A ∪ B = {1,2,3,4,7,8}.

**Example 2:** A = {x | -1 < x < 3}, B = {x | 0 ≤ x < 4}. Then A ∪ B = {x | -1 < x < 4}.

**Note:** A ∪ A = A; A ∪ ∅ = A.

**II. Intersection of Sets**

**Definition 2:** The **intersection** of sets A and B (A ∩ B) is the set of all elements that are in both A and B.
A ∩ B = {x | x ∈ A and x ∈ B}

**Example 3:** A = {1,2,4,7}, B = {3,2,8,7}. Then A ∩ B = {2,7}.

**Example 4:** A = {x | -1 < x < 3}, B = {x | 0 ≤ x < 4}. Then A ∩ B = {x | 0 ≤ x < 3}.

**Note:** A ∩ A = A; A ∩ ∅ = ∅.

**III. Complement of a Set**

**Definition 3:** The **universal set** (U) contains all elements under consideration.

**Definition 4:** The **complement** of set A (with respect to U), written ∁_U A, is the set of all elements in U that are not in A.
∁_U A = {x | x ∈ U and x ∉ A}

**Example 5:** U = {1,2,4,6,7}, A = {1,2}. Then ∁_U A = {4,6,7}.

**Example 6:** U = {x | -1 < x < 3}, A = {x | 1 ≤ x < 2}. Then ∁_U A = {x | -1 < x < 1 or 2 ≤ x < 3}.

**IV. Intervals**

Some sets can be written as intervals:
- Closed interval: [a,b] = {x | a ≤ x ≤ b}
- Left-open, right-closed: (a,b] = {x | a < x ≤ b}
- Left-closed, right-open: [a,b) = {x | a ≤ x < b}
- Open interval: (a,b) = {x | a < x < b}

**Example:** {x | 1 ≤ x < 2} = [1,2).

**Summary**
1. A ∪ B = {x | x ∈ A or x ∈ B}
   A ∩ B = {x | x ∈ A and x ∈ B}
   ∁_U A = {x | x ∈ U and x ∉ A}
2. Intervals: [a,b], (a,b], [a,b), (a,b).

**Exercises 1.3**

1. A = {-1,1,3,5}, B = {-1,2,4,6}. Then A ∪ B = ____, A ∩ B = ____.
2. A = {x | -3 < x ≤ 4}, B = {x | -5 ≤ x < 2}. Then A ∪ B = ____, A ∩ B = ____.
3. A = {x | x ≥ 0}, B = {x | x > 4}. Then A ∪ B = ____, A ∩ B = ____.
4. A = {x | x < -1}, B = {x | x ≤ 3}. Then A ∪ B = ____, A ∩ B = ____.
5. A = {x∈R | x²+4=0}, B = {x | 2x-4=0}. Then A ∪ B = ____, A ∩ B = ____.
6. U = {0,2,4,6,8,10}, A = {2,6,8}, B = {0,4,6,10}. Then ∁_U A = ____, ∁_U B = ____, (∁_U A) ∩ (∁_U B) = ____.
7. U = {x | -5 ≤ x < 5}, A = {x | -2 ≤ x < 2}. Then ∁_U A = ____.
8. U = {x∈Z | -5 ≤ x ≤ 5}, A = {x∈Z | -2 ≤ x < 2}. Then ∁_U A = ____.
9. U = R, A = (-4,2), B = [-1,5]. Then A ∪ B = ____, A ∩ B = ____, ∁_U A = ____, ∁_U B = ____.
10. U = {0,1,2,3,4,5,6,7,8}, M = {1,2,4,5}, N = {0,3,5,7}. Then ∁_U (M ∪ N) = ( )
    A. {6,8} B. {5,7} C. {4,6,7} D. {1,3,5,6,8}
11. U = {1,2,3,4,5,6,7}, A = {2,3,4,5}, B = {2,3,6,7}. Then B ∩ (∁_U A) = ( )
    A. {1,6} B. {1,7} C. {6,7} D. {1,6,7}

**Self-Test 1**

1. Fill with "∈", "∉", "⊆", "⊂", "⊄", or "=".
   (1) 5 _ {-4,0,4}
   (2) {0,3} _ {-1,0,3,5}
   (3) {x | 1<x≤3} _ {x | -2<x≤6}
   (4) ∅ _ {x | 3x+6=0}


   Chapter 2: Inequalities
2.1 Properties of Inequalities
I. Inequality Relations
In general, expressions connected by the symbols "<", "≤", ">", "≥", or "≠" are called inequalities.
Examples: 3 > 2, 2x + 1 ≤ 0, a ≠ 1.
For comparing the sizes of real numbers a and b:
a - b > 0 ⇔ a > b
a - b = 0 ⇔ a = b
a - b < 0 ⇔ a < b
Example: From a - 1 > 0, we get a > 1 (a - 1 > 0 ⇒ a > 1). From a ≤ -3, we get a + 3 ≤ 0 (a ≤ -3 ⇒ a + 3 ≤ 0).
II. Properties of Inequalities
Inequalities have the following properties:
Property 1: a > b ⇔ b < a.
Example: 3 > 2 ⇔ 2 < 3.
Property 2: a > b, b > c ⇒ a > c.
Example: 5 > 3, 3 > 2 ⇒ 5 > 2.
Property 3: a > b ⇒ a + c > b + c and a - c > b - c.
(Adding or subtracting the same number from both sides does not change the inequality direction.)
Example: 2 > 1 ⇒ 2 + 3 > 1 + 3 and 2 - 4 > 1 - 4.
Property 4:
(1) a > b, c > 0 ⇒ ac > bc.
(Multiplying or dividing both sides by the same positive number does not change the inequality direction.)
Example: a > 2 ⇒ 2a > 4.
(2) a > b, c < 0 ⇒ ac < bc.
(Multiplying or dividing both sides by the same negative number reverses the inequality direction.)
Example: a > 2 ⇒ -2a < -4.
Property 5: a > b, c > d ⇒ a + c > b + d.
Example: 3 > 2, -4 > -5 ⇒ 3 + (-4) > 2 + (-5).
Property 6: a > b > 0, c > d > 0 ⇒ ac > bd.
Example: 6 > 4 > 0, 3 > 2 > 0 ⇒ 6 × 3 > 4 × 2.
Property 7: a > b > 0 ⇒ a^n > b^n (n ∈ N, n ≥ 1).
Example: 1/2 > 1/3 > 0 ⇒ (1/2)^3 > (1/3)^3.
Property 8: a > b > 0 ⇒ ⁿ√a > ⁿ√b (n ∈ N, n ≥ 2).
Example: 1/4 > 1/9 > 0 ⇒ √(1/4) > √(1/9).
Example 10: Let a < b < 0. Fill in the blank with > or <: 1/a _ 1/b.
Solution: Since a < b < 0, then ab > 0, so 1/(ab) > 0.
By Property 4: a × 1/(ab) < b × 1/(ab) ⇒ 1/b < 1/a.
Therefore, 1/a > 1/b.
Example 11: Let -1 < a < 2, 1 < b < 3. Find () < 2a - 3b < ().
Solution:
From -1 < a < 2, by Property 4: -2 < 2a < 4.
From 1 < b < 3, by Property 4: -9 < -3b < -3.
By Property 5: -2 + (-9) < 2a + (-3b) < 4 + (-3) ⇒ -11 < 2a - 3b < 1.
Summary
Inequality Relations:
a - b > 0 ⇔ a > b
a - b = 0 ⇔ a = b
a - b < 0 ⇔ a < b
Properties of Inequalities:
(1) a > b ⇔ b < a.
(2) a > b, b > c ⇒ a > c.
(3) a > b ⇒ a + c > b + c, a - c > b - c.
(4) a > b, c > 0 ⇒ ac > bc; a > b, c < 0 ⇒ ac < bc.
(5) a > b, c > d ⇒ a + c > b + d.
(6) a > b > 0, c > d > 0 ⇒ ac > bd.
(7) a > b > 0 ⇒ a^n > b^n (n ∈ N, n ≥ 1).
(8) a > b > 0 ⇒ ⁿ√a > ⁿ√b (n ∈ N, n ≥ 2).
Exercises 2.1
Multiple Choice (Choose the single correct answer).
(1) Given a < b < 0, c > 0, which is correct? ( )
A. a - c < b - c
B. ac > bc
C. c/a < c/b
D. a/c < b/c
Fill in the blanks.
(1) Given a < b, c > 0, then a + c _ b + c, ac _ bc.
(2) Given -2 < a < 3, then _ < 3a < _, _ < -3a < _.
(3) Given -1 < a < 2, then _ < a + 3 < _, _ < a - 5 < _.
(4) Given -3 < a < 0, 2 < b < 4, then _ < a + 2b < _, _ < a - b < _.
(5) Given 2 < a < 4, 1 < b < 5, then _ < ab < _.
(6) Given 1/3 < a < 1/4, 1/5 < b < 1/2, then _ < 1/a + 2/b < _.
(7) Given -3 < a < 3, then _ < a^3 < _.
(8) Given 4 < a < 81, then _ < √a < _.
(9) Given 30 < x < 42, 16 < y < 24, then _ < x - 2y < _, _ < x/y < _.
2.2 Solving Quadratic Inequalities in One Variable
I. Solving Quadratic Equations
Definition 1: An equation of the form ax² + bx + c = 0 (a ≠ 0) is called a quadratic equation in one variable. Let Δ = b² - 4ac.
If Δ = b² - 4ac > 0, the equation has two distinct real roots:
x₁,₂ = (-b ± √(b² - 4ac))/(2a) (Quadratic Formula).
If Δ = b² - 4ac = 0, the equation has two equal real roots: x₁ = x₂ = -b/(2a).
If Δ = b² - 4ac < 0, the equation has no real roots.
Example 1: Solve.
(1) 2x² - 3x + 1 = 0
Solution: a=2, b=-3, c=1. Δ = (-3)² - 4×2×1 = 1 > 0.
x₁ = (3 - √1)/(4) = 1/2, x₂ = (3 + √1)/(4) = 1.
(2) x² - 4x + 4 = 0
Solution: a=1, b=-4, c=4. Δ = (-4)² - 4×1×4 = 0.
x₁ = x₂ = -(-4)/(2×1) = 2.
(3) 3x² - 2x + 4 = 0
Solution: a=3, b=-2, c=4. Δ = (-2)² - 4×3×4 = -44 < 0. No real roots.
II. Solving Quadratic Inequalities
Definition 2: An inequality containing one variable with the highest degree 2 is a quadratic inequality in one variable. General forms:
ax² + bx + c > 0 (≥, <, ≤, ≠ 0) (a ≠ 0).
The set of all solutions is the solution set.
Example 2: Solve 2x + 3 > 0.
Solution: 2x > -3 ⇒ x > -3/2. Solution set: {x | x > -3/2}.
The solution sets for ax² + bx + c > 0 (or <, ≥, ≤ 0) when a > 0 are summarized in Table 2.2-1, based on the graph of y = ax² + bx + c and the discriminant Δ.
Table 2.2-1 Solution Sets for Quadratic Inequalities (a > 0)
Example 3: Solve 2x² - 3x + 1 > 0.
Solution: From 2x² - 3x + 1 = 0, we get x₁ = 1/2, x₂ = 1. Since a = 2 > 0, the parabola opens upward. The inequality >0 holds where the graph is above the x-axis: x < 1/2 or x > 1. Solution set: {x | x < 1/2 or x > 1}.
Example 4: Solve -3x² - 2x + 1 ≥ 0.
Solution: Since a = -3 < 0, multiply both sides by -1 (reverse inequality): 3x² + 2x - 1 ≤ 0.
Solve 3x² + 2x - 1 = 0 ⇒ x₁ = -1, x₂ = 1/3. a = 3 > 0, parabola opens upward. The inequality ≤0 holds between the roots: -1 ≤ x ≤ 1/3. Solution set: {x | -1 ≤ x ≤ 1/3}.
Example 5: Solve x² - 2x + 1 ≥ 0.
Solution: x² - 2x + 1 = 0 ⇒ x₁ = x₂ = 1. a = 1 > 0. The parabola touches the x-axis at x=1 and is above it elsewhere. For ≥0, the solution is all real numbers. Solution set: R.
Example 6: Solve x² - 2x + 3 < 0.
Solution: Δ = 4 - 12 = -8 < 0. a = 1 > 0. The parabola is entirely above the x-axis. So x² - 2x + 3 is never < 0. Solution set: ∅.
Summary
Solving ax² + bx + c = 0 (a ≠ 0):
Δ > 0: Two distinct roots: x = (-b ± √Δ)/(2a)
Δ = 0: One double root: x = -b/(2a)
Δ < 0: No real roots.
Solving ax² + bx + c > 0 (or <, ≥, ≤ 0):
If a < 0, multiply by -1 first to make a > 0 (reverse inequality sign).
Steps:
(1) Find roots of ax² + bx + c = 0.
(2) Sketch graph of y = ax² + bx + c (a > 0, parabola opening up).
(3) Write solution set based on graph and inequality sign.
Exercises 2.2
Solve the quadratic equations.
(1) x² - 8x + 16 = 0
(2) 2x² - 5x - 3 = 0
(3) 3x² + 2x + 5 = 0
(4) x² - 5x = 0
(5) 4x² - 1 = 0
Solve the quadratic inequalities.
(1) 2x² - 7x + 3 > 0
(2) 5x² + 3x < 0
(3) x² - 4 < 0
(4) x² - 4x + 4 > 0
(5) 2x² - 5x + 3 < 0
(6) 2x² + 4x + 3 ≥ 0
(7) x² - 4x + 4 ≤ 0
(8) (2x - 3)(x - 3) > 0
(9) (x + 6)(5 - 2x) ≥ 0
(10) -2x² + 3x + 5 ≥ 0
(11) -3x² - 6x + 1 > 0
2.3 Solving Fractional Inequalities
Definition: An expression with a variable in the denominator is a fraction.
Examples: (x²-2x)/(x+1), 2x/(x²-4x+3), 1/(x-1) are fractions. (x+5)/2 is not.
For solving fractional inequalities:
f(x)/g(x) > 0 ⇔ f(x)g(x) > 0
f(x)/g(x) < 0 ⇔ f(x)g(x) < 0
f(x)/g(x) ≥ 0 ⇔ { f(x)g(x) ≥ 0 and g(x) ≠ 0 }
f(x)/g(x) ≤ 0 ⇔ { f(x)g(x) ≤ 0 and g(x) ≠ 0 }
Example 1: Solve (x+1)/(2x-3) > 0.
Solution: (x+1)/(2x-3) > 0 ⇔ (x+1)(2x-3) > 0.
Equation: (x+1)(2x-3)=0 ⇒ x₁=-1, x₂=3/2.
Since the product > 0, solution: x < -1 or x > 3/2.
Solution set: {x | x < -1 or x > 3/2}.
Example 2: Solve (3x-1)/(x+2) < 0.
Solution: (3x-1)/(x+2) < 0 ⇔ (3x-1)(x+2) < 0.
Equation: (3x-1)(x+2)=0 ⇒ x₁=-2, x₂=1/3.
Product < 0 ⇒ solution between roots: -2 < x < 1/3.
Solution set: {x | -2 < x < 1/3}.
Example 3: Solve (x-2)/(2x+5) ≥ 0.
Solution: (x-2)/(2x+5) ≥ 0 ⇔ { (x-2)(2x+5) ≥ 0 and 2x+5 ≠ 0 }.
Solve (x-2)(2x+5) ≥ 0: Roots x=-5/2, x=2. Product ≥ 0 ⇒ x ≤ -5/2 or x ≥ 2.
Exclude x = -5/2 (makes denominator 0). So x < -5/2 or x ≥ 2.
Solution set: {x | x < -5/2 or x ≥ 2}.
Example 4: Solve (2x+3)/(x-5) ≤ 0.
Solution: (2x+3)/(x-5) ≤ 0 ⇔ { (2x+3)(x-5) ≤ 0 and x-5 ≠ 0 }.
Solve (2x+3)(x-5) ≤ 0: Roots x=-3/2, x=5. Product ≤ 0 ⇒ -3/2 ≤ x ≤ 5.
Exclude x = 5. So -3/2 ≤ x < 5.
Solution set: {x | -3/2 ≤ x < 5}.
Summary
To solve f(x)/g(x) > 0 (or <, ≥, ≤ 0), convert to a product inequality f(x)g(x) > 0 (<0, etc.), remembering to exclude values that make g(x) = 0 for ≥ or ≤ cases.
Exercises 2.3
Solve the fractional inequalities.
(1) (2x-5)/(3x-7) > 0
(2) (3x+1)/(x-2) > 0
(3) (x-3)/(2x+5) > 0
(4) (2-x)/(3x+2) > 0
(1) (x+4)/(5x+3) < 0
(2) (x-4)/(4x+1) < 0
(3) (2x-1)/(x-4) < 0
(4) (1-2x)/(x+2) < 0
(1) (x-5)/(x+3) ≥ 0
(2) (2x-3)/(x-4) ≥ 0
(3) (5x+4)/(3x-1) ≥ 0
(4) (-3x+5)/(2x-1) ≥ 0
(1) (x+10)/(x+2) ≤ 0
(2) (3x+1)/(x+2) ≤ 0
(3) (4x-1)/(2x+3) ≤ 0
(4) (x-2)/(4-3x) ≤ 0
Self-Test 2
Multiple Choice (Choose the single correct answer).
(1) Which is correct? ( )
A. If a > b, then a + 2c > b + 2c
B. If a > b, then 1/a > 1/b
C. If -3a > -3b, then a > b
D. If a > b, c > d, then a - c > b - d
(2) Which is correct? ( )
A. If a > b, c > d, then ac > bd
B. If a < b, then a⁴ < b⁴
C. If a > b, then a³ < b³
D. If a > b > 0, then √a > √b
(3) The solution set of x² - 5x - 14 < 0 is ( ).
A. (-2, 7)
B. (-∞, -2) ∪ (7, ∞)
C. (-7, 2)
D. (-∞, -7) ∪ (2, ∞)
(4) The solution set of -2x² + 7x - 3 ≤ 0 is ( ).
A. [1/2, 3]
B. (-∞, 1/2] ∪ [3, ∞)
C. [-3, 1/2]
D. (-∞, -3] ∪ [-1/2, ∞)
(5) The solution set of x² - 12x + 36 ≥ 0 is ( ).
A. R
B. {x | x = 6}
C. {x | x ≠ 6}
D. ∅
Fill in the blanks.
(1) Given -4 < x < 7, then _ < 1 - 2x < _.
(2) Given 2 < x < 4, -3 < y < -2, then _ < 3x + 2y < _, _ < 3x - 2y < _, _ < 1/x + 1/y < _.
(3) The solution set of (x+2)(x-3) > 0 is _.
(4) The solution set of (x-2)(x-7) ≤ 0 is _.
(5) The solution set of x² + 2x + 4 > 0 is _.
(6) The solution set of x² + x + 3 < 0 is _.
(7) The solution set of x² - x - 6 ≥ 0 is _.
(8) The solution set of -6x² - x + 2 ≤ 0 is _.
Solve (show work).
(1) (2x-3)/(x+3) < 0
(2) (3x-5)/(x+5) ≥ 0
(3) (x+4)/(2x-3) > 0
(4) (5x+2)/(1-2x) ≥ 0

Chapter 3: Functions 
3.1 Rectangular Coordinate System (Прямоугольная система координат)

I. Basic Concepts

Definition 1: In a plane, draw two number lines that are perpendicular to each other and intersect at their origins. The horizontal number line is called the x-axis (abscissa axis), and the vertical number line is called the y-axis (ordinate axis). Together they form a rectangular coordinate system (or Cartesian coordinate system). The point of intersection is called the origin.

Definition 2: The plane with a rectangular coordinate system is called the coordinate plane. The x-axis and y-axis divide the plane into four parts, called quadrants:
- First quadrant: x > 0, y > 0
- Second quadrant: x < 0, y > 0
- Third quadrant: x < 0, y < 0
- Fourth quadrant: x > 0, y < 0

II. Coordinates of a Point

Any point P in the coordinate plane corresponds to an ordered pair of real numbers (x, y), where x is the abscissa and y is the ordinate.

Example 1: Determine the quadrant of point P(-3, -7).
Solution: Since x = -3 < 0 and y = -7 < 0, point P is in the third quadrant.

III. Distance in the Coordinate Plane

1. Distance from a point to the axes:
- Distance from point P(x, y) to the x-axis is |y|
- Distance from point P(x, y) to the y-axis is |x|

Example 2: For point P(6, -8):
Distance to x-axis = |y| = | -8 | = 8
Distance to y-axis = |x| = |6| = 6

2. Distance from a point to the origin:
d = √(x² + y²)

Example 3: For point P(6, -8):
Distance to origin = √(6² + (-8)²) = √(36 + 64) = √100 = 10

3. Distance between two points P(x₁, y₁) and Q(x₂, y₂):
d = √[(x₂ - x₁)² + (y₂ - y₁)²]

Example 4: Distance between P(6, -8) and Q(1, 4):
d = √[(1 - 6)² + (4 - (-8))²] = √[(-5)² + (12)²] = √(25 + 144) = √169 = 13

IV. Symmetry of Points

1. Symmetry about the x-axis:
Point P(x, y) reflected about the x-axis becomes P'(x, -y)

2. Symmetry about the y-axis:
Point P(x, y) reflected about the y-axis becomes P'(-x, y)

3. Symmetry about the origin:
Point P(x, y) reflected about the origin becomes P'(-x, -y)

Example 5: Find the point symmetric to P(-2, 3) about the y-axis.
Solution: Reflection about y-axis changes the sign of x-coordinate: P'(-(-2), 3) = P'(2, 3)
3.2 Concept of a Function
I. Definition of a Function
Definition: Let A and B be two sets of real numbers, A ≠ Ø, B ≠ Ø. If there exists a corresponding relationship f such that for every number x in A, there is a uniquely determined number y in B corresponding to it, then f: A → B is called a function from set A to set B. It is denoted as:
y = f(x), x ∈ A
where:

x is called the independent variable.

y is called the dependent variable.

A is called the domain, denoted as D_f, i.e., D_f = A.

{f(x) | x ∈ A} is called the range, denoted as R_f, i.e., R_f = {f(x) | x ∈ A}.

Note 1: {f(x) | x ∈ A} ⊆ B.

Example 1: Given the function f(x) = sqrt(x+1) + 1/(x-2):
(1) Find the domain of y = f(x).
(2) Find the values of f(3) and f(8).

Solution:
(1) The set where sqrt(x+1) is defined is {x | x ≥ -1}. The set where 1/(x-2) is defined is {x | x ≠ 2}. Therefore, the domain of the function is:
D_f = {x | x ≥ -1} ∩ {x | x ≠ 2} = {x | x ≥ -1 and x ≠ 2}.
(2) f(3) = sqrt(3+1) + 1/(3-2) = 3.
f(8) = sqrt(8+1) + 1/(8-2) = 3 + 1/6 = 19/6.

Example 2: Find the domain of the function f(x) = sqrt(x^2 - 2x - 3).

Solution: From x^2 - 2x - 3 ≥ 0, we get x ≤ -1 or x ≥ 3. Therefore, the domain is:
D_f = {x | x ≤ -1 or x ≥ 3}.

Note 2: Two functions are considered identical if they have the same domain, the same range, and the same corresponding relationship.

Example 3: Which of the following functions is identical to y = x? ( )
A. y = x^2 / x B. y = sqrt(x^2) C. y = cbrt(x^3) D. y = 2x

Solution:
y = x; D_f = R, R_f = R.
A: D_f = {x | x ≠ 0} ≠ R (different domain) -> not identical.
B: R_f = {y | y ≥ 0} ≠ R (different range) -> not identical.
C: D_f = R, R_f = R, and y = cbrt(x^3) = x (same corresponding relationship) -> identical.
D: y = 2x (different corresponding relationship) -> not identical.
Answer: C.

II. Graphs of Functions
Example 4: Draw the graph of the function y = 2x.
Solution: D_f = R. In the coordinate plane, plot points (0, 0) and (1, 2) and draw a line through them. This line is the graph of y = 2x (see Figure 3.2-1).

Example 5: Draw the graph of the function y = |x|.
Solution: D_f = R. y = x for x ≥ 0, and y = -x for x < 0. So the graph of y = |x| is as shown in Figure 3.2-2.

Summary
For a function f: A → B, the domain D_f = A, and the range R_f = {f(x) | x ∈ A} ⊆ B.

Two functions are identical if they have the same domain, same range, and same corresponding relationship.

3.3 Monotonicity of Functions
Definition: Let function y = f(x) be defined on interval I. Let x_1 and x_2 be any two points in I.

If for x_1 < x_2, we have f(x_1) ≤ f(x_2), then f(x) is called a monotone increasing function (or simply increasing function) on I. I is called a monotone increasing interval (or simply increasing interval).

If for x_1 < x_2, we have f(x_1) < f(x_2), then f(x) is called a strictly monotone increasing function (or strictly increasing function) on I. I is called a strictly monotone increasing interval.

If for x_1 < x_2, we have f(x_1) ≥ f(x_2), then f(x) is called a monotone decreasing function (or simply decreasing function) on I. I is called a monotone decreasing interval.

If for x_1 < x_2, we have f(x_1) > f(x_2), then f(x) is called a strictly monotone decreasing function (or strictly decreasing function) on I. I is called a strictly monotone decreasing interval.

Note 1: (1) A strictly increasing function is always an increasing function; a strictly increasing interval is always an increasing interval.
(2) A strictly decreasing function is always a decreasing function; a strictly decreasing interval is always a decreasing interval.

Note 2: If a function y = f(x) is either increasing or decreasing on interval I, then it is called a monotone function on I, and I is called a monotone interval.

Example 1: Given the graph of y = f(x) on [-3, 6] as shown in Figure 3.3-3, state the monotone intervals and indicate which are increasing and which are decreasing.
Solution: The monotone intervals are [-3, -1], [-1, 2], [2, 4], [4, 6]. Among these, the increasing intervals are [-1, 2] and [4, 6]; the decreasing intervals are [-3, -1] and [2, 4].

Example 2: Determine if the function y = 2x is increasing or decreasing on (-∞, +∞).
Solution: For any x_1, x_2 ∈ (-∞, +∞), let x_1 < x_2.
Since x_1 < x_2 implies 2x_1 < 2x_2, we have f(x_1) < f(x_2).
Therefore, y = 2x is an increasing function on (-∞, +∞).

Example 3: Determine if the function y = 1/x is increasing or decreasing on (-∞, 0).
Solution: For any x_1, x_2 ∈ (-∞, 0), let x_1 < x_2.
f(x_1) - f(x_2) = 1/x_1 - 1/x_2 = (x_2 - x_1) / (x_1 * x_2).
Since x_1, x_2 ∈ (-∞, 0), we have x_1 * x_2 > 0.
Since x_1 < x_2, we have x_2 - x_1 > 0.
Thus, f(x_1) - f(x_2) > 0, so f(x_1) > f(x_2).
Therefore, y = 1/x is a decreasing function on (-∞, 0).

Example 4: Determine if the function y = x^2 + 1 is increasing or decreasing on (0, +∞).
Solution: For any x_1, x_2 ∈ (0, +∞), let x_1 < x_2.
f(x_1) - f(x_2) = (x_1^2 - x_2^2) = (x_1 + x_2)(x_1 - x_2).
Since x_1, x_2 > 0, x_1 + x_2 > 0.
Since x_1 < x_2, x_1 - x_2 < 0.
Thus, f(x_1) - f(x_2) < 0, so f(x_1) < f(x_2).
Therefore, y = x^2 + 1 is an increasing function on (0, +∞).

Summary
For y = f(x) and interval I ⊆ D_f:
For any x_1, x_2 ∈ I with x_1 < x_2:

If f(x_1) ≤ f(x_2), then f(x) is increasing on I (I is an increasing interval).

If f(x_1) < f(x_2), then f(x) is strictly increasing on I (I is a strictly increasing interval).

If f(x_1) ≥ f(x_2), then f(x) is decreasing on I (I is a decreasing interval).

If f(x_1) > f(x_2), then f(x) is strictly decreasing on I (I is a strictly decreasing interval).
A strictly increasing function is an increasing function. A strictly decreasing function is a decreasing function.

Steps to determine if f(x) is increasing or decreasing on interval I:

Step 1: For any x_1, x_2 ∈ I, let x_1 < x_2.

Step 2: Compute f(x_1) - f(x_2).

Step 3:
If f(x_1) - f(x_2) ≤ 0, then f(x) is increasing.
If f(x_1) - f(x_2) < 0, then f(x) is strictly increasing.
If f(x_1) - f(x_2) ≥ 0, then f(x) is decreasing.
If f(x_1) - f(x_2) > 0, then f(x) is strictly decreasing.

3.4 Parity of Functions (Even and Odd Functions)
I. Symmetry of a Set with Respect to the Origin
Definition 1: A set A is said to be symmetric with respect to the origin if whenever x is an element of A, then -x is also an element of A.

Example 1: The set B = {-2, -1, 0, 1, 2} is symmetric with respect to the origin.
Example 2: The set C = (-1, 1] is not symmetric with respect to the origin because 1 ∈ C, but -1 ∉ C.

II. Parity of Functions
1. Odd Function
Definition 2: Let the domain of the function y = f(x) be D_f. If:
(1) D_f is symmetric with respect to the origin, and
(2) f(-x) = -f(x) for all x ∈ D_f,
then y = f(x) is called an odd function.

Example 3: Is the function f(x) = 3x an odd function?
Solution: The domain D_f = R, which is symmetric about the origin. Also, f(-x) = 3(-x) = -3x = -f(x) for all x ∈ D_f. Therefore, it is an odd function.

Example 4: Is the function f(x) = 3x, x ∈ (-1, 2) an odd function?
Solution: The domain D_f = (-1, 2) is not symmetric about the origin. Therefore, it is not an odd function.

Example 5: Is the function f(x) = 2x + 1, x ∈ (-1, 1) an odd function?
Solution: The domain D_f = (-1, 1) is symmetric about the origin. However, f(-x) = 2(-x) + 1 = -2x + 1 ≠ -f(x). Therefore, it is not an odd function.

2. Even Function
Definition 3: Let the domain of the function y = f(x) be D_f. If:
(1) D_f is symmetric with respect to the origin, and
(2) f(-x) = f(x) for all x ∈ D_f,
then y = f(x) is called an even function.

Example 6: Is the function f(x) = x^2 + 1 an even function?
Solution: The domain D_f = R is symmetric about the origin. Also, f(-x) = (-x)^2 + 1 = x^2 + 1 = f(x). Therefore, it is an even function.

Example 7: Is the function f(x) = sqrt(x+1) an even function?
Solution: The domain D_f = [-1, +∞) is not symmetric about the origin. Therefore, it is not an even function.

Notes:
(1) If a function is neither odd nor even, it is called a non-odd, non-even function. If a function is both odd and even, it is called a both odd and even function.
(2) The graph of an odd function is symmetric with respect to the origin (Figure 3.4-1). The graph of an even function is symmetric with respect to the y-axis (Figure 3.4-2).

Summary
Set A is symmetric about the origin: ∀ x ∈ A ⇒ -x ∈ A.
If D_f is not symmetric about the origin ⇒ f(x) is non-odd, non-even.

If D_f is symmetric about the origin, then:

If f(-x) = -f(x) for all x ∈ D_f ⇒ f(x) is an odd function.

If f(-x) = f(x) for all x ∈ D_f ⇒ f(x) is an even function.

If f(-x) ≠ -f(x) and f(-x) ≠ f(x) ⇒ f(x) is non-odd, non-even.

3.5 Inverse Functions
Definition: Given a function f: A → f(A), if there is a one-to-one correspondence between elements x in A and elements y in f(A), then from y = f(x) we can obtain x = f^{-1}(y). x = f^{-1}(y) is called the inverse function of y = f(x). Conventionally, we write x = f^{-1}(y) as y = f^{-1}(x).

Therefore, in the inverse function y = f^{-1}(x), the variable x corresponds to the y in the original function, and the variable y corresponds to the x in the original function.

Note: The domain of the inverse function y = f^{-1}(x) is the range of the original function y = f(x). The range of the inverse function y = f^{-1}(x) is the domain of the original function y = f(x).

Example 1: Find the inverse function of y = 2x + 1.
Solution:
y = 2x + 1 ⇒ x = (y - 1)/2.
Swap x and y: y = (x - 1)/2.
Therefore, the inverse function is y = (x - 1)/2 (x ∈ R).

Example 2: Find the inverse function of y = sqrt(x-1) (x ≥ 1).
Solution: Since x ≥ 1, y = sqrt(x-1) ≥ 0.
y = sqrt(x-1) ⇒ x = y^2 + 1.
Swap x and y: y = x^2 + 1.
Therefore, the inverse function is y = x^2 + 1 (x ≥ 0).

Example 3: Find the inverse function of y = (2x-1)/(x+3) (x ≠ -3).
Solution:
y = (2x-1)/(x+3) ⇒ y(x+3) = 2x-1 ⇒ (2-y)x = 3y+1 ⇒ x = (3y+1)/(2-y).
Swap x and y: y = (3x+1)/(2-x).
Therefore, the inverse function is y = (3x+1)/(2-x) (x ≠ 2).

Summary
Steps to find the inverse function:
(1) From y = f(x), solve for x = f^{-1}(y).
(2) Swap x and y in x = f^{-1}(y) to get the inverse function y = f^{-1}(x).

The domain of y = f^{-1}(x) is the range of y = f(x). The range of y = f^{-1}(x) is the domain of y = f(x).


3.6 Power Functions

A function of the form y = x^a (where a is a constant) is called a power function.

Examples: y = x^2, y = x^(1/2), y = x^(-2), y = x^(-1/2)

Looking at the graphs of y = x, y = x^2, y = x^(1/2), y = x^(-2), y = x^(-1), y = x^(-1/2) in the first quadrant, we see:

(1) When a > 0, y = x^a is an increasing function in the first quadrant.
(2) When a < 0, y = x^a is a decreasing function in the first quadrant.
(3) When a is an odd number, y = x^a is an odd function; when a is an even number, y = x^a is an even function.

Note (1) For f(x) = x^(n/m) = m-th root of x^n (m, n are positive integers), the domain D_f is:
[0, +∞) when n is odd, m is even.
(-∞, +∞) when n is even, m is odd.
(-∞, +∞) when n is odd, m is odd.

For example: function y = x^(1/2) has domain [0, +∞), function y = x^(5/3) has domain (-∞, +∞).

(2) For f(x) = x^(-n/m) = 1/(m-th root of x^n), the domain D_f is:
(0, +∞) when n is odd, m is even.
(-∞, 0) ∪ (0, +∞) when n is even, m is odd.
(-∞, 0) ∪ (0, +∞) when n is odd, m is odd.

For example: function y = x^(-1/2) has domain (0, +∞), function y = x^(-5/3) has domain (-∞, 0) ∪ (0, +∞).

Example: Compare the following pairs of numbers.

(1) (3.03)^(1/2), (3.02)^(1/2)
Solution: Let y = x^(1/2). Since a = 1/2 > 0, y = x^(1/2) is increasing in the first quadrant. Since 3.03 > 3.02, we have (3.03)^(1/2) > (3.02)^(1/2).

(2) (0.85)^3, (0.84)^3
Solution: Let y = x^3. Since a = 3 > 0, y = x^3 is increasing in the first quadrant. Since 0.85 > 0.84, we have (0.85)^3 > (0.84)^3.

(3) (4.1)^(-1/3), (4.2)^(-1/3)
Solution: Let y = x^(-1/3). Since a = -1/3 < 0, y = x^(-1/3) is decreasing in the first quadrant. Since 4.1 < 4.2, we have (4.1)^(-1/3) > (4.2)^(-1/3).

(4) (0.35)^(-3), (0.34)^(-3)
Solution: Let y = x^(-3). Since a = -3 < 0, y = x^(-3) is decreasing in the first quadrant. Since 0.35 > 0.34, we have (0.35)^(-3) < (0.34)^(-3).

Summary

Domain rules for power functions f(x) = x^(n/m):

[0, +∞) when n odd, m even

(-∞, +∞) when n even, m odd

(-∞, +∞) when n odd, m odd

Monotonicity of power function y = x^a:

When a > 0, y = x^a is increasing in the first quadrant.

When a < 0, y = x^a is decreasing in the first quadrant.

Exercises 3.6

Multiple choice (choose the single correct answer for each).
(1) Which of the following is a power function?
A. y = 2x - 1
B. y = 2x^7
C. y = x^(-4)
D. y = x^2 - 1

(2) Which function is both even and increasing on (0, +∞)?
A. y = x^(2/3)
B. y = x^(3/5)
C. y = -2/x
D. y = x^(-1/2)

3.7 Exponential Functions

A function of the form y = a^x (where a is constant, a > 0 and a ≠ 1) is called an exponential function.

Examples: y = 2^x, y = 3^x, y = (1/2)^x

Looking at the graphs of y = 2^x and y = (1/2)^x, we see that y = a^x satisfies:
(1) Domain: all real numbers (R)
(2) Range: (0, +∞)
(3) Graph passes through point (0, 1)
(4) When a > 1, y = a^x is increasing on R; when 0 < a < 1, y = a^x is decreasing on R.

Example 1: Compare the following pairs of numbers.
(1) (3.03)^(1/2), (3.03)^(1/3)
Solution: Let y = (3.03)^x. Since a = 3.03 > 1, y = (3.03)^x is increasing on R. Since 1/2 > 1/3, we have (3.03)^(1/2) > (3.03)^(1/3).

(2) (5/4)^(-2), (5/4)^(-3)
Solution: Let y = (5/4)^x. Since a = 5/4 > 1, y = (5/4)^x is increasing on R. Since -2 > -3, we have (5/4)^(-2) > (5/4)^(-3).

(3) (0.43)^(2/3), (0.43)^(1/3)
Solution: Let y = (0.43)^x. Since 0 < a = 0.43 < 1, y = (0.43)^x is decreasing on R. Since 2/3 > 1/3, we have (0.43)^(2/3) < (0.43)^(1/3).

(4) (3/7)^(-5), (3/7)^(-4)
Solution: Let y = (3/7)^x. Since 0 < a = 3/7 < 1, y = (3/7)^x is decreasing on R. Since -5 < -4, we have (3/7)^(-5) > (3/7)^(-4).

Example 2: Solve the inequality (1/4)^(2x+3) > (1/4)^(x-4).
Solution: Since 0 < a = 1/4 < 1, y = a^x is decreasing. Therefore, 2x+3 < x-4, so x < -7. The solution set is {x | x < -7}.

Example 3: Solve the inequality 3^(x-1) > 3^(2-3x).
Solution: Since a = 3 > 1, y = a^x is increasing. Therefore, x-1 > 2-3x, so 4x > 3, x > 3/4. The solution set is {x | x > 3/4}.

Summary

Exponential function y = a^x (a > 0, a ≠ 1) properties:
(1) Domain: R
(2) Range: (0, +∞)
(3) Graph passes through (0, 1)
(4) When a > 1, y = a^x is increasing on R; when 0 < a < 1, y = a^x is decreasing on R.

Exercises 3.7

Find the domain of the following functions.
(1) y = 3^(2/(x-1))
(2) y = sqrt(2^(x-1) - 4)
(3) y = sqrt(1 - 3^(x-1))
(4) y = 2/(4^(x^2 - 2))

Compare the following pairs of numbers (fill in >, <, or =).
(1) 3^0.8 ___ 3^0.7
(2) 0.76^(-0.3) ___ 0.76^0.3
(3) 1.01^2.7 ___ 1.01^3.5
(4) a^m ___ a^n (a > 1, m < n)
(5) a^m ___ a^n (0 < a < 1, m < n)

Multiple choice.
(1) Let a = 0.8^1.7, b = 0.8^0.9, c = 1.2^0.8. Then:
A. a > b > c
B. c > b > a
C. b > a > c
D. b > c > a

(2) The inequality 3^(2a+1) < 3^(3-2a) holds when a is in:
A. (1, +∞)
B. (1/2, +∞)
C. (-∞, 1)
D. (-∞, 1/2)

(3) If x > 0 and a^x < b^x < 1, then:
A. 0 < b < a < 1
B. 0 < a < b < 1
C. 1 < b < a
D. 1 < a < b

3.8 Logarithms and Logarithmic Functions

Part 1: Logarithms

Definition 1: If a^b = N (a > 0, a ≠ 1), then b is called the logarithm of N to base a, written as b = log_a N, where a is the base and N is the antilogarithm.

Example 1: Find the value of log_2 8.
Solution: Let log_2 8 = b. By definition, 2^b = 8, so b = 3. Therefore, log_2 8 = 3.

Example 2: Find the value of log_(1/3) 9.
Solution: Let log_(1/3) 9 = b. By definition, (1/3)^b = 9, so 3^(-b) = 9, giving -b = 2, b = -2. Therefore, log_(1/3) 9 = -2.

Notes from the definition:
(1) log_a a = 1 (a > 0, a ≠ 1)
(2) log_a 1 = 0 (a > 0, a ≠ 1)
(3) Common logarithm: lg N = log_10 N (N > 0)
(4) Natural logarithm: ln N = log_e N (N > 0)
Examples: lg 100 = 2, ln e = 1.

Logarithmic properties:
If a > 0, a ≠ 1, b > 0, b ≠ 1, M > 0, N > 0, then:
(1) log_a (MN) = log_a M + log_a N
(2) log_a (M/N) = log_a M - log_a N
(3) log_a (M^n) = n log_a M (n ∈ R)
(4) log_a M = (log_b M)/(log_b a) (change of base)
(5) log_a b · log_b a = 1

Examples:
Example 3: log_2 3 + log_2 4 = log_2 (3×4) = log_2 12
Example 4: log_2 12 - log_2 4 = log_2 (12/4) = log_2 3
Example 5: log_4 27 = log_2 (3^3) = 3/2 log_2 3
Example 6: log_2 3 = (log_5 3)/(log_5 2)
Example 7: log_2 3 · log_3 2 = 1

Example 8: Calculate the value of: (log_4 (1/27) + log_(1/3) 9) × (log_81 (1/8) - log_3 4) + log_2 16
[Detailed calculation steps provided in the text]

Part 2: Logarithmic Functions

Definition 2: The function y = log_a x (a > 0, a ≠ 1) is called a logarithmic function.

Examples: y = log_2 x, y = log_3 x, y = log_(1/2) x

Properties from the graph:
(1) Domain: (0, +∞)
(2) Range: R
(3) Graph passes through (1, 0)
(4) When a > 1, y = log_a x is increasing on (0, +∞); when 0 < a < 1, y = log_a x is decreasing on (0, +∞).

Example 9: Compare the following pairs of numbers.
(1) log_5 7, log_5 8
Solution: Let y = log_5 x. Since a = 5 > 1, y = log_5 x is increasing. Since 7 < 8, we have log_5 7 < log_5 8.

(2) log_0.3 5, log_0.3 4
Solution: Let y = log_0.3 x. Since 0 < a = 0.3 < 1, y = log_0.3 x is decreasing. Since 5 > 4, we have log_0.3 5 < log_0.3 4.

(3) log_(4/5) 0.3, log_(4/5) 0.2
Solution: Let y = log_(4/5) x. Since 0 < a = 4/5 < 1, y = log_(4/5) x is decreasing. Since 0.3 > 0.2, we have log_(4/5) 0.3 < log_(4/5) 0.2.

(4) log_1.3 5.1, log_1.3 4.9
Solution: Let y = log_1.3 x. Since a = 1.3 > 1, y = log_1.3 x is increasing. Since 5.1 > 4.9, we have log_1.3 5.1 > log_1.3 4.9.

Example 10: Solve the inequality log_4 (2x+3) > log_4 (x-4).
Solution: Since a = 4 > 1, y = log_4 x is increasing. Therefore:
2x+3 > x-4 → x > -7
2x+3 > 0 → x > -3/2
x-4 > 0 → x > 4
The intersection gives x > 4. Solution set: {x | x > 4}.

Example 11: Find the domain of y = sqrt(log_0.5 (2x-3)).
Solution: log_0.5 (2x-3) ≥ 0 = log_0.5 1 → 2x-3 ≤ 1 → x ≤ 2
Also 2x-3 > 0 → x > 3/2
Therefore, domain: {x | 3/2 < x ≤ 2}.

Part 3: Inverse Functions of Logarithmic Functions

If y = log_a x (a > 0, a ≠ 1), then x = a^y. Swapping x and y gives y = a^x.
Thus, the inverse of y = log_a x is the exponential function y = a^x.
Similarly, the inverse of y = a^x is y = log_a x.

Example 12: Find the inverse of y = log_2 x.
Solution: y = log_2 x → x = 2^y. Swapping gives y = 2^x. So the inverse is y = 2^x (x ∈ R).

Example 13: Find the inverse of y = (1/2)^x.
Solution: Taking log base 1/2 of both sides: log_(1/2) y = x. Swapping gives y = log_(1/2) x. So the inverse is y = log_(1/2) x (x > 0).

Summary

Logarithm properties (listed above).

Logarithmic function y = log_a x (a > 0, a ≠ 1) properties:
(1) Domain: (0, +∞)
(2) Range: R
(3) Graph passes through (1, 0)
(4) When a > 1, increasing on (0, +∞); when 0 < a < 1, decreasing on (0, +∞)

Logarithmic and exponential functions are inverses of each other.

Exercises 3.8

Calculate the values of the following expressions.
(1) (log_4 3 + log_8 3)(log_3 2 + log_9 2)
(2) log_9 5 × log_8 9 × log_81 2 × log_25 27
(3) (log_8 5 + log_4 0.2)(log_5 16 + log_(4√5) 0.5)
(4) log_5 8 × log_(6√2) 4 × log_2 0.2

Find the domain of the following functions.
(1) y = log_3 (x-2)
(2) y = log_2 (1-2x)
(3) y = log_5 x^2
(4) y = sqrt(log_(1/4) (x+1))
(5) y = sqrt(log_(1/2) (3x-2))

Compare the following pairs of numbers.
(1) ln 5 ___ ln 3
(2) log_0.6 2.3 ___ log_0.6 3.4
(3) log_0.3 0.2 ___ log_3 0.2
(4) log_7 0.2 ___ log_7 0.8

Find the inverse functions of the following.
(1) y = log_2 (2x-1)
(2) y = 3 log_0.5 (x-1)
(3) y = 5^(2x-1)
(4) y = (2/3)^(1/(x-3))

SELF-TEST 3

Multiple choice.
(1) In the Cartesian coordinate system, point P(-3, -7) is in:
A. First quadrant
B. Second quadrant
C. Third quadrant
D. Fourth quadrant

(2) In the Cartesian coordinate system, the point symmetric to P(-2, 3) about the y-axis is:
A. (-2, -3)
B. (2, -3)
C. (3, 2)
D. (2, 3)

(3) Which function is identical to y = x + 1?
A. y = (x^2)/x + 1
B. y = sqrt((x+1)^2)
C. y = cube root of ((x+1)^3)
D. y = (sqrt(x+1))^2

(4) The graph of function y = x^(1/2) is: [Four graph options A, B, C, D shown]

(5) Function y = 2x - 3 on (-∞, +∞) is:
A. Increasing
B. Decreasing
C. Not monotonic
D. Cannot determine monotonicity

(6) Function f(x) = -2x^3 is:
A. Odd
B. Even
C. Neither odd nor even
D. Parity cannot be determined

(7) Function f(x) = x^3 - 2 is:
A. Odd
B. Even
C. Neither odd nor even
D. Parity cannot be determined

(8) Function f(x) = x^(6/5) is:
A. Odd
B. Even
C. Neither odd nor even
D. Parity cannot be determined

(9) Which expression is correct?
A. log_2.4 4.2 > log_2.4 4.6
B. 2.7^(-2/3) < 2.7^(-3/5)
C. 3^(-2.5) < 2^(-2.5)
D. 0.2^(-3) > 0.2^(-4)

Fill in the blanks.
(1) Point P(6, -8): distance to x-axis = ___, to y-axis = ___, to origin = ___, to point Q(1,4) = ___.
(2) Function f(x) = sqrt(x-1) / cube root of (x-4), domain = ___.
(3) Function y = 2x - 5, inverse function = ___.
(4) Function y = 2x^2 - 1 (x ≥ 0), inverse function = ___.
(5) Function y = (x-3)/(x+2) (x ≠ -2), inverse function = ___.
(6) Function y = sqrt(8 - 2^(2-x)), domain = ___.
(7) Function y = log_3 (3x-6), domain = ___.
(8) Function y = log_2 (3x+5), inverse function = ___.
(9) Function y = (1/3)^(2x-1), inverse function = ___.

Calculation problems (show work and give results).
(1) Calculate: (log_2 125 + log_4 25 + log_8 5)(log_125 8 + log_25 4 + log_5 2)
(2) Given sets A = {x | log_3 (4-x^2) > log_3 (3x)} and B = {x | 0.2^x > 0.2}, find A ∩ B.

CHAPTER 4: TRIGONOMETRY

4.1 Angle Definition and Radians

Part 1: Angle Definition

Angle Definition: An angle is formed by rotating a ray around its endpoint from one position to another in a plane. It is usually denoted by letters like α, β. The endpoint is called the vertex, the starting ray is the initial side, and the final ray is the terminal side.

Positive and Negative Angles: An angle formed by rotating counterclockwise is called a positive angle. An angle formed by rotating clockwise is called a negative angle.

Zero Angle: If the initial and terminal sides coincide, the angle is a zero angle, written as α = 0°.

Acute, Right, and Obtuse Angles:

If 0° < α < 90°, α is an acute angle.

If α = 90°, α is a right angle.

If 90° < α < 180°, α is an obtuse angle.

Quadrantal Angles: If the vertex is at the origin and the initial side coincides with the positive x-axis, then:

If the terminal side lies in the first quadrant, it's a first quadrant angle.

If it lies in the second quadrant, it's a second quadrant angle.

If it lies in the third quadrant, it's a third quadrant angle.

If it lies in the fourth quadrant, it's a fourth quadrant angle.

If the terminal side lies on an axis, the angle doesn't belong to any quadrant.

Specifically:

k·360° < α < k·360° + 90° (k ∈ Z) ⇔ α is a first quadrant angle.

k·360° + 90° < α < k·360° + 180° (k ∈ Z) ⇔ α is a second quadrant angle.

k·360° + 180° < α < k·360° + 270° (k ∈ Z) ⇔ α is a third quadrant angle.

k·360° + 270° < α < k·360° + 360° (k ∈ Z) ⇔ α is a fourth quadrant angle.

Note 1: (1) Angles α = k·360° + β (k ∈ Z) and β have the same terminal side and are in the same quadrant.
(2) To determine the quadrant of α, write it as α = k·360° + β, where -360° < β < 360° (k ∈ Z).

Example 1: Determine the quadrant of each angle.
(1) 800° (2) 1000° (3) -1600°

Solution:
(1) 800° = 2×360° + 80°, and 80° is a first quadrant angle → 800° is a first quadrant angle.
(2) 1000° = 3×360° - 80°, and -80° is a fourth quadrant angle → 1000° is a fourth quadrant angle.
(3) -1600° = (-4)×360° - 160°, and -160° is a third quadrant angle → -1600° is a third quadrant angle.

Part 2: Radians

360° = 2π radians; 180° = π radians.

Note 2: (1) Angles α = k·2π + β (k ∈ Z) and β have the same terminal side.
(2) To determine the quadrant of α in radians, write it as α = k·2π + β, where -2π < β < 2π (k ∈ Z).

Example 2: Determine the quadrant of each angle in radians.
(1) 37π/6 (2) 341π/8 (3) 100π/3

Solution:
(1) 37π/6 = 3·2π + π/6, and π/6 is first quadrant → 37π/6 is first quadrant.
(2) 341π/8 = 21·2π + 5π/8, and 5π/8 is second quadrant → 341π/8 is second quadrant.
(3) 100π/3 = -16·2π - 4π/3, and -4π/3 is second quadrant → 100π/3 is second quadrant.

Summary

Angle Types: Positive, negative, zero, acute (0°<α<90°), right (α=90°), obtuse (90°<α<180°).

Quadrantal Angles:

First quadrant: k·360° < α < k·360°+90° (k ∈ Z)

Second quadrant: k·360°+90° < α < k·360°+180° (k ∈ Z)

Third quadrant: k·360°+180° < α < k·360°+270° (k ∈ Z)

Fourth quadrant: k·360°+270° < α < k·360°+360° (k ∈ Z)

Degree-Radian Relation: π = 180°

Method to Determine Quadrant: Write α = k·360° + β (-360°<β<360°) or α = k·2π + β (-2π<β<2π). The quadrant of β is the quadrant of α.

Exercises 4.1

Multiple choice.
(1) Let A = {angles less than 90°}, B = {first quadrant angles}. Then A ∩ B =
A. {acute angles} B. {angles less than 90°} C. {first quadrant angles} D. None of the above

(2) If α is acute, then α+90° is:
A. First quadrant angle B. Third quadrant angle C. Positive angle less than 180° D. Positive angle less than a right angle

(3) If α is a third quadrant angle, then α/2 is:
A. First or second quadrant B. Second or third quadrant C. First or third quadrant D. Second or fourth quadrant

(4) Which angle has the same terminal side as 330°?
A. 30° B. 150° C. -150° D. -30°

(5) If α = 6π/7, its terminal side lies in:
A. First quadrant B. Second quadrant C. Third quadrant D. Fourth quadrant

(6) -300° =
A. 4π/3 B. 5π/3 C. 7π/4 D. 7π/6

(7) -335° is a:
A. First quadrant angle B. Second quadrant angle C. Third quadrant angle D. Fourth quadrant angle

(8) The set {α | kπ ≤ α ≤ kπ + π/4, k ∈ Z} represents which shaded region? [Diagram options]

Complete the table:

Degrees	0°	45°	90°	120°	135°	150°	270°
Radians	0	π/6	π/3				π	2π
Determine the quadrant of each angle.

Group A:
(1) 80° (2) -215° (3) 305° (4) -150° (5) 5π/3 (6) π/9 (7) 11π/6 (8) 5π/4

Group B:
(1) 860° (2) 1356° (3) -2000° (4) -1200° (5) 31π/5 (6) 89π/7 (7) 57π/9 (8) 97π/4

4.2 Trigonometric Definitions and Relationships

Part 1: Trigonometric Function Definitions

Let α be any angle, and P(x, y) be any point on its terminal side (not the origin). Let r = √(x² + y²). Then:

sin α = y/r

cos α = x/r

tan α = y/x (x ≠ 0)

cot α = x/y (y ≠ 0)

sec α = r/x (x ≠ 0)

csc α = r/y (y ≠ 0)

Example 1: Given P(-3, 4) on the terminal side of α, find sin α, cos α, tan α, cot α, sec α, csc α.

Solution: x=-3, y=4, r=√((-3)²+4²)=5.
sin α = 4/5, cos α = -3/5, tan α = -4/3, cot α = -3/4, sec α = -5/3, csc α = 5/4.

Note 1: Since angles k·2π + α (k ∈ Z) and α have the same terminal side, by definition:
sin(2kπ + α) = sin α, or sin(k·360° + α) = sin α (k ∈ Z)
cos(2kπ + α) = cos α, or cos(k·360° + α) = cos α (k ∈ Z)
tan(2kπ + α) = tan α, or tan(k·360° + α) = tan α (k ∈ Z)
cot(2kπ + α) = cot α, or cot(k·360° + α) = cot α (k ∈ Z) [Formula 4-1]

Example: sin(13π/6) = sin(2π + π/6) = sin(π/6) = 1/2.

Part 2: Signs of Trigonometric Functions

The signs of sin α, cos α, and tan α in each quadrant can be summarized as:

Quadrant I: all positive (sin>0, cos>0, tan>0)

Quadrant II: only sin positive

Quadrant III: only tan positive

Quadrant IV: only cos positive

A mnemonic: "All Students Take Calculus" (All, Sin, Tan, Cos) for quadrants I, II, III, IV respectively.

Example 2: Fill with > or <.
(1) cos 250° ___ 0
(2) sin(-π/4) ___ 0
(3) tan(-672°) ___ 0
(4) tan(11π/3) ___ 0

Solution:
(1) 250° is QIII → cos negative → <
(2) -π/4 is QIV → sin negative → <
(3) -672° = -2×360° + 48°, 48° is QI → tan positive → >
(4) 11π/3 = 4π - π/3, -π/3 is QIV → tan negative → <

Example 3: If cos α > 0, then α is in which quadrant(s)?
A. I or II B. II or III C. III or IV D. I or IV
Solution: cos α = x/r > 0 → x > 0 → α in QI or QIV. Answer D.

Part 3: Trigonometric Relationships

From the definitions, we get:

sin²α + cos²α = 1

tan α = sin α / cos α

cot α = cos α / sin α = 1/tan α

sec α = 1/cos α

csc α = 1/sin α

1 + tan²α = sec²α

1 + cot²α = csc²α

cos²α = 1/(1+tan²α)

Example 4: Given cos α = 1/4, find sin α.
Solution: sin²α = 1 - cos²α = 1 - (1/16) = 15/16 → sin α = ±√15/4.

Example 5: Given sin α = -1/3, and α in QIII, find cos α, tan α, cot α, sec α, csc α.
Solution: cos²α = 1 - sin²α = 1 - 1/9 = 8/9. Since α in QIII, cos α = -√(8/9) = -2√2/3.
tan α = sin α/cos α = (-1/3)/(-2√2/3) = 1/(2√2) = √2/4.
cot α = 1/tan α = 2√2.
sec α = 1/cos α = -3/(2√2) = -3√2/4.
csc α = 1/sin α = -3.

Example 6: Given tan α = 3, find (3 sin α - cos α)/(sin α + 2 cos α).
Solution: Since tan α = sin α/cos α = 3 → sin α = 3 cos α.
Substitute: (3(3 cos α) - cos α)/(3 cos α + 2 cos α) = (8 cos α)/(5 cos α) = 8/5.

Example 7: Given tan α = -2, find cos α and sin α.
Solution: cos²α = 1/(1+tan²α) = 1/(1+4) = 1/5 → cos α = ±√5/5.
sin α = tan α·cos α = (-2)(±√5/5) = ∓2√5/5.

Summary

Definitions: sin α = y/r, cos α = x/r, tan α = y/x, cot α = x/y, sec α = r/x, csc α = r/y.

Periodicity (Formula 4-1): sin(2kπ+α)=sin α, cos(2kπ+α)=cos α, tan(2kπ+α)=tan α, cot(2kπ+α)=cot α (k ∈ Z).

Signs by Quadrant:

Quadrant	sin α	cos α	tan α
I	+	+	+
II	+	-	-
III	-	-	+
IV	-	+	-
Fundamental Relationships:

sin²α + cos²α = 1

tan α = sin α/cos α

cot α = 1/tan α

sec α = 1/cos α

csc α = 1/sin α

1 + tan²α = sec²α

cos²α = 1/(1+tan²α)

Exercises 4.2

Given P(2, -2√3) on terminal side of α, find all six trig functions.

Given P(-6, 8) on terminal side of α, find all six trig functions.

Given P(-1, -2) on terminal side of α, find all six trig functions.

Compute values:
(1) sin(-300°), cos(-300°), tan(-300°), cot(-300°)
(2) sin(-690°), cos(-690°), tan(-690°), cot(-690°)
(3) sin(17π/4), cos(17π/4), tan(17π/4), cot(17π/4)
(4) sin 450°, cos 540°, cos 6π, tan 5π, cot 450°

Fill with > or <:
(1) sin 770° ___ 0
(2) cos(-950°) ___ 0
(3) sin(99π/5) ___ 0
(4) tan(117π/6) ___ 0

If terminal side of α passes through P(1,-3), then sin α cos α =
A. 3/10 B. 3√10/10 C. -3/10 D. -√10/10

cos 1110° =
A. 1/2 B. -1/2 C. √3/2 D. -√3/2

Given P(√3, -1) on terminal side of α, then cos α + tan α = ____.

Terminal side of α passes through (3a-9, a+2). If sin α > 0 and cos α < 0, then α is in ____.

Given cos α = 12/13, α in QIV, find sin α, tan α, cot α, sec α, csc α.

Given cos α = -2/3, find sin α and tan α.

Given tan α = -1/2, find:
(1) (sin α + cos α)/(sin α - 3 cos α)
(2) (1 + 2 sin α cos α)/(sin²α - cos²α)
(3) 2 sin²α - 3 sin α cos α - 5 cos²α

4.3 Trigonometric Reduction Formulas

Part 1: Formulas for π+α and -α

Formula 4-2 (π+α):
sin(π+α) = -sin α
cos(π+α) = -cos α
tan(π+α) = tan α
cot(π+α) = cot α

Formula 4-3 (-α):
sin(-α) = -sin α
cos(-α) = cos α
tan(-α) = -tan α
cot(-α) = -cot α

Formula 4-4 (π-α):
sin(π-α) = sin α
cos(π-α) = -cos α
tan(π-α) = -tan α
cot(π-α) = -cot α

Note: The trigonometric values of π±α and -α are equal to the corresponding function values of α, with the sign determined by the quadrant of π±α or -α when α is considered acute.

Example 1: Find values.
(1) sin 210° = sin(180°+30°) = -sin 30° = -1/2
(2) cos(-60°) = cos 60° = 1/2
(3) tan(3π/4) = tan(π-π/4) = -tan(π/4) = -1
(4) sin(11π/6) = sin(2π-π/6) = sin(-π/6) = -sin(π/6) = -1/2
(5) cos(21π/4) = cos(4π+5π/4) = cos(5π/4) = cos(π+π/4) = -cos(π/4) = -√2/2
(6) tan(8π/3) = -tan(8π/3) ... [simplification steps] = √3

Part 2: Formulas for π/2 ± α

Formula 4-5 (π/2 - α):
sin(π/2 - α) = cos α
cos(π/2 - α) = sin α
tan(π/2 - α) = cot α
cot(π/2 - α) = tan α

Formula 4-6 (π/2 + α):
sin(π/2 + α) = cos α
cos(π/2 + α) = -sin α
tan(π/2 + α) = -cot α
cot(π/2 + α) = -tan α

Note: For π/2 ± α, the sine becomes cosine and vice versa, with the sign determined by the quadrant.

Example 2: Given sin α = -3/5, find cos(π/2 - α) and cos(π/2 + α).
Solution: cos(π/2 - α) = sin α = -3/5; cos(π/2 + α) = -sin α = 3/5.

Example 3: Given tan α = 1/2, find cot(π/2 - α), cot(π/2 + α), cot(3π/2 + α).
Solution:
cot(π/2 - α) = tan α = 1/2
cot(π/2 + α) = -tan α = -1/2
cot(3π/2 + α) = cot(π + (π/2+α)) = cot(π/2+α) = -tan α = -1/2

Summary

Memorize and use Formulas 4-2 through 4-6.

Exercises 4.3

Find values:
(1) sin 210°, cos 210°, tan 210°, cot 210°
(2) sin(5π/4), cos(5π/4), tan(5π/4), cot(5π/4)
(3) sin(4π/3), cos(4π/3), tan(4π/3), cot(4π/3)
(4) sin 180°, cos 180°, tan 180°, cot 270°

Find values:
(1) sin(-45°), cos(-45°), tan(-45°), cot(-45°)
(2) sin(-7π/6), cos(-5π/4), tan(-4π/3), cot(-4π/3)

Find values:
(1) sin 150°, cos 150°, tan 150°, cot 150°
(2) sin(2π/3), cos(3π/4), tan(5π/6)
(3) sin(-2π/3), cos(-3π/4), tan(-5π/6)

Simplify:
(1) [-sin(180°+α) + sin(-α) - tan(360°+α)] / [tan(180°+α) + cos(-α) + cos(180°-α)]
(2) [sin(α-5π) cos(π/2-α) cos(8π-α)] / [cos(3π-α) sin(α-3π) sin(-α-4π)]

Calculate:
(1) sin 750° tan(-405°) + cos(-660°)
(2) sin 585° cos 1290° + cos(-30°) sin 210°
(3) sin(-420°) + cos 510° - tan 675° + cot 390°
(4) sin(-π/3) + 2 sin(4π/3) sin(16π/3) + sin(2π/3)
(5) sin(-10π/3) cos(5π/6) tan(-7π/4)
(6) sin(-13π/4) cos(25π/6) tan(-8π/3) cot(11π/4)

Given sin α = 1/3, find cos(π/2-α) and cos(π/2+α).

Given cos α = -2/3, find sin(π/2-α) and sin(π/2+α).

Given tan α = -3/5, find cot(π/2-α), cot(π/2+α), cot(3π/2+α).

Given cot α = -2, find tan(π/2-α), tan(π/2+α), tan(3π/2+α).

4.4 Graphs and Properties of Sine and Cosine Functions

Definition: A function f(x) is periodic if there exists a non-zero constant T such that f(x+T) = f(x) for all x in its domain. T is called a period.

Example: f(x) = sin x is periodic because f(x+2π) = sin(x+2π) = sin x = f(x), so 2π is a period.

Part 1: Sine Function y = sin x

Properties:

Domain: R

Range: [-1, 1]

Parity: sin(-x) = -sin x → odd function

Periodicity: sin(2kπ + x) = sin x (k ∈ Z). Period = 2kπ (k≠0), minimal positive period = 2π.

Monotonicity:

Increasing on [2kπ - π/2, 2kπ + π/2] (k ∈ Z)

Decreasing on [2kπ + π/2, 2kπ + 3π/2] (k ∈ Z)

Note 1: For y = A sin(ωx + φ) (A, ω, φ constants, ω≠0):

Domain: R

Range: [-|A|, |A|]

Minimal positive period: 2π/|ω|

Example 1: Find domain, range, and minimal period.
(1) y = -2 sin x + 1
Domain: R
Range: -1 ≤ sin x ≤ 1 → -2 ≤ -2 sin x ≤ 2 → -1 ≤ -2 sin x + 1 ≤ 3 → Range = [-1, 3]
Period: T = 2π/1 = 2π

(2) y = 4 sin(-2x + π/3)
Domain: R
Range: [-4, 4]
Period: T = 2π/|-2| = π

Example 2: Which function is odd?
A. y = sin x + 1
B. y = sin(x+π/4)
C. y = 3 sin(5x-π/6)
D. y = -sin 2x
Answer: D

Example 3: The increasing interval of y = sin x is:
A. [-π, 0] B. [0, π] C. [-π/2, π/2] D. [π/2, 3π/2]
Answer: C

Part 2: Cosine Function y = cos x

Properties:

Domain: R

Range: [-1, 1]

Parity: cos(-x) = cos x → even function

Periodicity: cos(2kπ + x) = cos x (k ∈ Z). Minimal positive period = 2π.

Monotonicity:

Increasing on [2kπ + π, 2kπ + 2π] (k ∈ Z)

Decreasing on [2kπ, 2kπ + π] (k ∈ Z)

Note 2: For y = A cos(ωx + φ):

Domain: R

Range: [-|A|, |A|]

Minimal positive period: 2π/|ω|

Example 4:
(1) y = (1/2) cos x - 2
Domain: R
Range: -1 ≤ cos x ≤ 1 → -1/2 ≤ (1/2)cos x ≤ 1/2 → -5/2 ≤ (1/2)cos x - 2 ≤ -3/2 → Range = [-5/2, -3/2]
Period: T = 2π

(2) y = -3 cos(4x - π/3)
Domain: R
Range: [-3, 3]
Period: T = 2π/4 = π/2

Example 5: Which is NOT even?
A. y = 2 cos x + 1
B. y = (1/2) cos x
C. y = sin(πx/2) [Note: sin(πx/2) = cos x? Actually sin(π/2 x) ≠ cos x generally, but if x=... This seems like a trick. The text says it becomes cos x after simplification? Let's check original: In text: "y = sin(π/2 x) = cos x" – This is true only for certain x? Actually sin(π/2 - θ)=cos θ, but here it's sin(π/2 * x). Possibly a misprint?]
D. y = x + cos(x+π/2) = x - sin x
Checking parity: f(-x) = -x - sin(-x) = -x + sin x = -(x - sin x) = -f(x) → odd, not even.
So D is not even. Answer D.

Example 6: Decreasing interval of y = cos x:
A. [-π, 0] B. [0, π] C. [-π/2, π/2] D. [π/2, 3π/2]
Answer: B

Summary

Sine function y = sin x:

Domain: R, Range: [-1,1], Odd, Period: 2π

Increasing: [2kπ-π/2, 2kπ+π/2]

Decreasing: [2kπ+π/2, 2kπ+3π/2]

Cosine function y = cos x:

Domain: R, Range: [-1,1], Even, Period: 2π

Increasing: [2kπ+π, 2kπ+2π]

Decreasing: [2kπ, 2kπ+π]

General forms y = A sin(ωx+φ) or y = A cos(ωx+φ):

Domain: R, Range: [-|A|, |A|], Period: 2π/|ω|

Exercises 4.4

Sketch y = sin x on [-2π, 2π], indicate increasing/decreasing intervals.

Sketch y = -3 sin x on [-2π, 2π], indicate increasing/decreasing intervals.

Sketch y = cos x on [-2π, 2π], indicate increasing/decreasing intervals.

Sketch y = -2 cos x on [-2π, 2π], indicate increasing/decreasing intervals.

Find ranges:
(1) y = 5 sin x + 2
(2) y = -2 sin x + 5
(3) y = (2/3) sin(3x-π/3) - 1
(4) y = 4 cos(2x+π/4)
(5) y = -7 cos x + 10
(6) y = -3 cos(7x-π/5) + 9

Find minimal periods:
(1) y = 2 sin(-2x+π/2) - 1
(2) y = -8 sin(4x+π/5) - 5
(3) y = (5/7) cos(x/3+π/3) + 2
(4) y = 4 cos(5x-π/7) - 1

Determine parity (odd/even/neither):
(1) y = 2 sin x + 4
(2) y = (5/3) sin(2x-2π)
(3) y = 2 sin(x-π/3)
(4) y = 5 cos(2x+π/4)
(5) y = -3 cos(-2x) + 1
(6) y = 3 cos(x+π/5)

4.5 Graphs and Properties of Tangent and Cotangent Functions

Part 1: Tangent Function y = tan x

Properties:

Domain: {x | x ≠ kπ + π/2, k ∈ Z}

Range: (-∞, +∞)

Parity: tan(-x) = -tan x → odd function

Periodicity: tan(kπ + x) = tan x (k ∈ Z). Minimal positive period = π.

Monotonicity: Increasing on (kπ - π/2, kπ + π/2) for each k ∈ Z.

Note 1: For y = A tan(ωx+φ) (ω≠0):

Domain: Solve ωx+φ ≠ kπ+π/2 → x ≠ (kπ + π/2 - φ)/ω

Range: (-∞, +∞)

Period: π/|ω|

Example 1:
(1) y = -2 tan(2x+π/3) + 1
Domain: 2x+π/3 ≠ kπ+π/2 → x ≠ kπ/2 + π/12
Range: R
Period: π/2

(2) y = 4 tan(-3x+π/4)
Domain: -3x+π/4 ≠ kπ+π/2 → x ≠ -kπ/3 - π/12
Range: R
Period: π/3

Example 2: Which is odd?
A. y = 2 tan x - 1
B. y = tan(x+π/4)
C. y = (1/2) tan(x-π/6)
D. y = 5 tan 2x
Answer: D

Example 3: Increasing interval of y = tan x:
A. (-π,0) B. (0,π) C. (-π/2,π/2) D. (π,2π)
Answer: C

Part 2: Cotangent Function y = cot x

Properties:

Domain: {x | x ≠ kπ, k ∈ Z}

Range: (-∞, +∞)

Parity: cot(-x) = -cot x → odd function

Periodicity: cot(kπ + x) = cot x (k ∈ Z). Minimal positive period = π.

Monotonicity: Decreasing on (kπ, kπ+π) for each k ∈ Z.

Note 2: For y = A cot(ωx+φ) (ω≠0):

Domain: Solve ωx+φ ≠ kπ → x ≠ (kπ - φ)/ω

Range: (-∞, +∞)

Period: π/|ω|

Example 4:
(1) y = -2 cot(2x+π/3) + 1
Domain: 2x+π/3 ≠ kπ → x ≠ kπ/2 - π/6
Range: R
Period: π/2

(2) y = (1/2) cot(-4x+π/4)
Domain: -4x+π/4 ≠ kπ → x ≠ -kπ/4 + π/16
Range: R
Period: π/4

Example 5: Which is odd?
A. y = 3 cot x + 1
B. y = cot(x-π/4)
C. y = 2 cot(3x-π/6)
D. y = 3 cot 2x
Answer: D

Example 6: Decreasing interval of y = cot x:
A. (-2π,0) B. (0,π) C. (-π/2,π/2) D. (π/2,3π/2)
Answer: B

Summary

Tangent y = tan x:

Domain: x ≠ kπ+π/2, Range: R, Odd, Period: π

Increasing on (kπ-π/2, kπ+π/2)

Cotangent y = cot x:

Domain: x ≠ kπ, Range: R, Odd, Period: π

Decreasing on (kπ, kπ+π)

General forms:

y = A tan(ωx+φ): Domain from ωx+φ ≠ kπ+π/2, Period π/|ω|

y = A cot(ωx+φ): Domain from ωx+φ ≠ kπ, Period π/|ω|

Exercises 4.5

Sketch y = tan x on (3π/2, 5π/2).

Sketch y = cot x on (-2π, 3π).

Find domains:
(1) y = 3 tan(x+3π/4) + 1
(2) y = (1/3) tan(-2x+π/3) - 1
(3) y = 3 cot(-2x+π/4)
(4) y = -4 cot(3x-π/5) + 3

Find minimal periods:
(1) y = -4 tan(x+π/3) - 1
(2) y = 7 tan(-2x+π/5) + 3
(3) y = (1/3) cot(x/3+π/3)
(4) y = 5 cot(-4x-π/4) + 1

Determine parity:
(1) y = tan x - 3
(2) y = 2 tan(x+π/6)
(3) y = cot(x+π/5)
(4) y = 3 cot(2x-2π/5)
(5) y = 5 cot(x/2+π)

SELF-TEST 4

Multiple choice.
(1) -840° is in which quadrant?
A. I B. II C. III D. IV

(2) 35π/4 is in which quadrant?
A. I B. II C. III D. IV

(3) If tan α < 0, α is in which quadrant(s)?
A. I or II B. I or III C. III or IV D. II or IV

(4) If sin α cos α > 0, α is in which quadrant(s)?
A. I or II B. II or III C. I or III D. II or IV

(5) cos(π/2 - α) =
A. cos α B. -cos α C. sin α D. -sin α

(6) Maximum of y = (5/2) sin x - 1 is:
A. 7/2 B. -7/2 C. -3/2 D. 3/2

(7) Which interval is increasing for y = -cos x?
A. [-π/2, π/2] B. [π/2, 3π/2] C. [0, π] D. [π, 2π]

(8) Function y = 2 cot(x+π/2):
A. Odd B. Even C. Both odd and even D. Neither

(9) sin α = ?
A. sin(π-α) B. sin(π+α) C. sin(2π-α) D. sin(3π+α)

(10) Domain of y = tan(π/4 - x) is:
A. {x | x ≠ π/4} B. {x | x ≠ -π/4} C. {x | x ≠ kπ+π/4, k∈Z} D. {x | x ≠ kπ+3π/4, k∈Z}

(11) If sin(α+π/6) = 4/5, then sin(α+7π/6) =
A. 2√3/5 B. -2√3/5 C. 4/5 D. -4/5

Fill in blanks.
(1) If tan α = 1/2, then (sin α + 3 cos α)/(sin α - cos α) = ____.
(2) Minimal period of y = 2 cos((2/3)x + π/5) is T = ____.
(3) Minimal period of y = 2 tan(-2x + π/3) is T = ____.
(4) sin(11π/3) cos(5π/6) tan(4π/3) = ____.
(5) If cos α = -1/2, π/2 ≤ α ≤ π, then α = ____.
(6) Decreasing interval of y = 2 sin(2x+π/6) for x ∈ [-π,0] is ____.

Calculation problems.
(1) Given P(4,-2) on terminal side of α, find all six trig functions.
(2) Given cos α = -√5/5, α in QIII, find sin α and tan α.
(3) Given tan α = -3, find 2 cos²α + 3 sin α cos α.
(4) Calculate tan(-780°) cos 390° - sin(-1020°).
(5) For y = A sin(ωx+φ) (A>0, ω>0, 0<φ<2π): minimum = -2, minimal period = 2π/3, graph passes through (0,-√2). Find the function.

CHAPTER 5: INVERSE TRIGONOMETRIC FUNCTIONS

5.1 Inverse Sine and Inverse Cosine Functions

Part 1: Inverse Sine Function

The inverse of the sine function y = sin x for x in [-π/2, π/2] is called the inverse sine function, written as y = arcsin x.

Examples:
arcsin(-1) = -π/2
arcsin(-√2/2) = -π/4
arcsin(0) = 0
arcsin(√2/2) = π/4
arcsin(1) = π/2

Properties of y = arcsin x:

Domain: [-1, 1]

Range: [-π/2, π/2]

Parity: arcsin(-x) = -arcsin x, so it is an odd function.

Monotonicity: Increasing on [-1, 1]

Relationships:
sin(arcsin x) = x for x in [-1, 1]
arcsin(sin x) = x for x in [-π/2, π/2]

Example 1: Write x using inverse sine.
(1) sin x = 1/4, x in [-π/2, π/2]
Answer: x = arcsin(1/4)
(2) sin x = -2/3, x in [-π/2, π/2]
Answer: x = arcsin(-2/3) = -arcsin(2/3)

Example 2: Which is correct?
A. sin(arcsin 2) = 2
B. arcsin(sin(2π/3)) = 2π/3
C. sin(arcsin 1) = π/2
D. arcsin(sin(2π/3)) = π/3

Answer: D is correct.
A is wrong because 2 is not in [-1,1].
B is wrong because 2π/3 is not in [-π/2, π/2].
C is wrong because sin(arcsin 1) = 1, not π/2.
D is correct because sin(2π/3)=√3/2, and arcsin(√3/2)=π/3.

Part 2: Inverse Cosine Function

The inverse of the cosine function y = cos x for x in [0, π] is called the inverse cosine function, written as y = arccos x.

Examples:
arccos(-1) = π
arccos(-√2/2) = 3π/4
arccos(0) = π/2
arccos(√2/2) = π/4
arccos(1) = 0

Properties of y = arccos x:

Domain: [-1, 1]

Range: [0, π]

Parity: arccos(-x) = π - arccos x, so it is neither odd nor even.

Monotonicity: Decreasing on [-1, 1]

Relationships:
cos(arccos x) = x for x in [-1, 1]
arccos(cos x) = x for x in [0, π]

Example 3: Write x using inverse cosine.
(1) cos x = 4/5, x in [0, π]
Answer: x = arccos(4/5)
(2) cos x = -3/4, x in [0, π]
Answer: x = arccos(-3/4) = π - arccos(3/4)

Example 4: Calculate.
(1) sin(arccos(1/3))
Let θ = arccos(1/3). Then cos θ = 1/3 and θ in (0, π/2).
sin θ = √(1 - cos²θ) = √(1 - 1/9) = √(8/9) = 2√2/3.

(2) sin(arccos(-1/3))
arccos(-1/3) = π - arccos(1/3)
sin(π - arccos(1/3)) = sin(arccos(1/3)) = 2√2/3.

(3) cos(arcsin(3/5))
Let θ = arcsin(3/5). Then sin θ = 3/5 and θ in (0, π/2).
cos θ = √(1 - sin²θ) = √(1 - 9/25) = √(16/25) = 4/5.

(4) cos(arcsin(-3/5))
arcsin(-3/5) = -arcsin(3/5)
cos(-arcsin(3/5)) = cos(arcsin(3/5)) = 4/5.

Summary

Inverse sine y = arcsin x:
Domain: [-1,1], Range: [-π/2, π/2], Odd, Increasing.
sin(arcsin x) = x (for x in [-1,1]), arcsin(sin x) = x (for x in [-π/2, π/2]).

Inverse cosine y = arccos x:
Domain: [-1,1], Range: [0, π], Neither odd nor even, Decreasing.
cos(arccos x) = x (for x in [-1,1]), arccos(cos x) = x (for x in [0, π]).

Exercises 5.1

Fill in the blanks:
(1) arcsin(√2/2) = ______
(2) arcsin 1 = ______
(3) arcsin(√3/2) = ______
(4) arcsin(-1) = ______
(5) arccos(√2/2) = ______
(6) arccos(√3/2) = ______
(7) arccos(-√2/2) = ______
(8) arccos(-1/2) = ______
(9) sin(arcsin(√3/2)) = ______
(10) arcsin(sin(-π/6)) = ______
(11) cos(arccos(-1/2)) = ______
(12) arccos(cos(π/3)) = ______

Find domain and range:
(1) y = 4 arcsin((1/2)x - 1)
(2) y = 2 arccos(√(x + 1/2))
(3) y = -5 arcsin(2x + 1)
(4) y = (3/2) arccos(2x - 4)

Which statement is correct?
A. sin(arcsin x) = x, for x in [-π/2, π/2]
B. arcsin(-x) = -arcsin x, for x in [-1,1]
C. arcsin(sin x) = x, for x in R
D. arccos(-x) = arccos x, for x in [-1,1]
Answer: B

Write x using inverse trig functions:
(1) sin x = 2/5, x in [-π/2, π/2]
(2) sin x = √3/4, x in [-π/2, π/2]
(3) cos x = 1/5, x in [0, π]
(4) cos x = 3/4, x in [0, π]

5.2 Inverse Tangent and Inverse Cotangent Functions

Part 1: Inverse Tangent Function

The inverse of the tangent function y = tan x for x in (-π/2, π/2) is called the inverse tangent function, written as y = arctan x.

Examples:
arctan(-1) = -π/4
arctan(0) = 0
arctan(1) = π/4

Properties of y = arctan x:

Domain: (-∞, +∞)

Range: (-π/2, π/2)

Parity: arctan(-x) = -arctan x, so it is an odd function.

Monotonicity: Increasing on (-∞, +∞)

Relationships:
tan(arctan x) = x for x in (-∞, +∞)
arctan(tan x) = x for x in (-π/2, π/2)

Example 1: Calculate.
(1) arctan(√3/3) = π/6
(2) arctan(-√3) = -π/3

Example 2: Write x using inverse tangent.
(1) tan x = 2, x in (-π/2, π/2)
Answer: x = arctan 2
(2) tan x = -4, x in (-π/2, π/2)
Answer: x = arctan(-4) = -arctan 4

Part 2: Inverse Cotangent Function

The inverse of the cotangent function y = cot x for x in (0, π) is called the inverse cotangent function, written as y = arccot x.

Examples:
arccot(-1) = 3π/4
arccot(0) = π/2
arccot(1) = π/4

Properties of y = arccot x:

Domain: (-∞, +∞)

Range: (0, π)

Parity: arccot(-x) = π - arccot x, so it is neither odd nor even.

Monotonicity: Decreasing on (-∞, +∞)

Relationships:
cot(arccot x) = x for x in (-∞, +∞)
arccot(cot x) = x for x in (0, π)

Example 3: Calculate.
(1) arccot(√3/3) = π/3
(2) arccot(-√3) = 5π/6

Example 4: Write x using inverse cotangent.
(1) cot x = 3, x in (0, π)
Answer: x = arccot 3
(2) cot x = -5, x in (0, π)
Answer: x = arccot(-5) = π - arccot 5

Summary

Inverse tangent y = arctan x:
Domain: R, Range: (-π/2, π/2), Odd, Increasing.
tan(arctan x) = x (for x in R), arctan(tan x) = x (for x in (-π/2, π/2)).

Inverse cotangent y = arccot x:
Domain: R, Range: (0, π), Neither odd nor even, Decreasing.
cot(arccot x) = x (for x in R), arccot(cot x) = x (for x in (0, π)).

Exercises 5.2

Fill in the blanks:
(1) arctan 1 = ______
(2) arctan(-√3) = ______
(3) arccot(-√3) = ______
(4) arccot(-1) = ______
(5) tan(arctan 4) = ______
(6) tan(arctan(-5)) = ______
(7) cot(arccot 2) = ______
(8) cot(arccot(-4)) = ______

Find domain and range:
(1) y = 2 arctan(3x - 4) + π/3
(2) y = (5/3) arctan(x + 3)
(3) y = (3/5) arccot(2x + 3)
(4) y = -3 arccot(2x - 3) + 4

Which statement is incorrect?
A. tan(arctan(-1)) = -1
B. cot(arccot 2) = 2
C. arctan(tan(π/4)) = π/4
D. arccot(cot(5π/4)) = 5π/4
Answer: D (because 5π/4 is not in (0, π), so the formula does not directly apply).

Write x using inverse trig functions:
(1) tan x = 5/2, x in (-π/2, π/2)
(2) tan x = -1/3, x in (-π/2, π/2)
(3) cot x = 10, x in (0, π)
(4) cot x = -5, x in (0, π)

SELF-TEST 5

Multiple choice:
(1) Inverse sine y = arcsin x is:
A. Even B. Neither odd nor even C. Odd D. Decreasing
Answer: C

(2) Inverse cosine y = arccos x is:
A. Even B. Neither odd nor even C. Odd D. Increasing
Answer: B

(3) Inverse tangent y = arctan x is:
A. Even B. Neither odd nor even C. Odd D. Decreasing
Answer: C

(4) Inverse cotangent y = arccot x is:
A. Even B. Neither odd nor even C. Odd D. Increasing
Answer: B

(5) Domain of arctan x is:
A. (-π/2, π/2) B. [-π/2, π/2] C. (0, π) D. R
Answer: D

(6) Range of arccot x is:
A. (-π/2, π/2) B. [0, π] C. (0, π) D. R
Answer: C

(7) Which is correct?
A. arcsin(sin(7π/2)) = 7π/2
B. arccos(cos(3π)) = 3π
C. arctan(tan(3π/4)) = -π/4
D. arccot(cot(2π/3)) = -2π/3
Answer: C

Write x using inverse trig functions:
(1) sin x = 1/3, x in [-π/2, π/2]
(2) sin x = -√3/2, x in [-π/2, π/2]
(3) cos x = 3/4, x in [0, π]
(4) cos x = -√2/5, x in [0, π]
(5) tan x = 5, x in (-π/2, π/2)
(6) tan x = -7/3, x in (-π/2, π/2)
(7) cot x = 1/5, x in (0, π)
(8) cot x = -5/3, x in (0, π)

Graphing exercises:
(1) Complete table and graph y = arcsin x.

Chapter 6: Trigonometric Functions of Sums and Differences of Angles

6.1 Formulas for Sine, Cosine, and Tangent of Sums and Differences of Angles

I. Formulas for Sine of Sum and Difference of Two Angles

sin(α + β) = sinα cosβ + cosα sinβ (Formula 6-1)
sin(α - β) = sinα cosβ - cosα sinβ (Formula 6-2)

Example 1: Calculate the value of sin75°.

Solution: Using Formula 6-1:
sin75° = sin(45° + 30°)
= sin45° cos30° + cos45° sin30°
= (√2/2) * (√3/2) + (√2/2) * (1/2) = (√6 + √2)/4

Example 2: Given sinα = -1/3, cosβ = 1/4, and α, β are both angles in the fourth quadrant, find the value of sin(α - β).

Solution: Since sin²α + cos²α = 1,
cos²α = 1 - sin²α, cosα = ±√(1 - sin²α),
and α is in the fourth quadrant with sinα = -1/3,
cosα = √(1 - sin²α) = √(1 - (-1/3)²) = √(8/9) = (2√2)/3.
Since sin²β + cos²β = 1,
sin²β = 1 - cos²β, sinβ = ±√(1 - cos²β),
and β is in the fourth quadrant with cosβ = 1/4,
sinβ = -√(1 - cos²β) = -√(1 - (1/4)²) = -√(15/16) = -√15/4.
Therefore, using Formula 6-2:
sin(α - β) = sinα cosβ - cosα sinβ
= (-1/3) * (1/4) - (2√2/3) * (-√15/4)
= -1/12 + (2√30)/12 = (2√30 - 1)/12.

Example 3: Find the maximum and minimum values of the function y = a sin x + b cos x (x ∈ R).

Solution:
y = a sin x + b cos x = √(a² + b²) * [ (a/√(a² + b²)) sin x + (b/√(a² + b²)) cos x ].
Since (a/√(a² + b²))² + (b/√(a² + b²))² = 1,
there exists an angle α such that cosα = a/√(a² + b²), sinα = b/√(a² + b²).
Thus, y = √(a² + b²) (cosα sin x + sinα cos x)
= √(a² + b²) sin(x + α).
Since -1 ≤ sin(x + α) ≤ 1,
-√(a² + b²) ≤ y ≤ √(a² + b²).
Therefore, the maximum value of y = a sin x + b cos x is √(a² + b²), and the minimum value is -√(a² + b²).

II. Formulas for Cosine of Sum and Difference of Two Angles

cos(α + β) = cosα cosβ - sinα sinβ (Formula 6-3)
cos(α - β) = cosα cosβ + sinα sinβ (Formula 6-4)

Example 4: Calculate the values of cos75° and cos15°.

Solution: Using Formula 6-3:
cos75° = cos(45° + 30°)
= cos45° cos30° - sin45° sin30° = (√2/2)*(√3/2) - (√2/2)*(1/2) = (√6 - √2)/4.

Using Formula 6-4:
cos15° = cos(45° - 30°)
= cos45° cos30° + sin45° sin30° = (√2/2)*(√3/2) + (√2/2)*(1/2) = (√6 + √2)/4.

III. Formulas for Tangent of Sum and Difference of Two Angles

tan(α + β) = (tanα + tanβ) / (1 - tanα tanβ) (Formula 6-5)
tan(α - β) = (tanα - tanβ) / (1 + tanα tanβ) (Formula 6-6)

Example 5: Calculate the value of tan75°.

Solution: Using Formula 6-5:
tan75° = tan(45° + 30°) = (tan45° + tan30°) / (1 - tan45° tan30°) = (1 + √3/3) / (1 - √3/3) = (3 + √3)/(3 - √3) = 2 + √3.

Example 6: Given tanα = 1, find the value of tan(α - π/3).

Solution: Using Formula 6-6:
tan(α - π/3) = (tanα - tan(π/3)) / (1 + tanα tan(π/3)) = (1 - √3) / (1 + √3) = √3 - 2.

Summary

Common trigonometric formulas for sums and differences of angles:
(1) Sine formulas:
sin(α + β) = sinα cosβ + cosα sinβ (Formula 6-1)
sin(α - β) = sinα cosβ - cosα sinβ (Formula 6-2)
(2) Cosine formulas:
cos(α + β) = cosα cosβ - sinα sinβ (Formula 6-3)
cos(α - β) = cosα cosβ + sinα sinβ (Formula 6-4)
(3) Tangent formulas:
tan(α + β) = (tanα + tanβ) / (1 - tanα tanβ) (Formula 6-5)
tan(α - β) = (tanα - tanβ) / (1 + tanα tanβ) (Formula 6-6)

The maximum value of y = a sin x + b cos x (x ∈ R) is √(a² + b²), and the minimum value is -√(a² + b²).

Exercises 6.1

cos 285° = ( ).
A. (√6 - √2)/4
B. (√6 + √2)/4
C. (√2 - √6)/4
D. (√2 + √6)/4

(tan 75° - tan 15°) / (1 + tan 75° tan 15°) = ( ).
A. √3
B. √3/3
C. 1
D. -√3

cos(5π/12) cos(π/6) + sin(5π/12) sin(π/6) = ( ).
A. 0
B. 1/2
C. √2/2
D. √3/2

Calculate the values of cos 105° and tan 15°.

Calculate the value of each expression.
(1) sin42° cos18° + cos42° sin18°
(2) sin170° cos50° - cos170° sin50°
(3) cos12° cos18° - sin12° sin18°
(4) cos160° cos25° + sin160° sin25°
(5) cos(45° - α) cos(α + 15°) - sin(45° - α) sin(α + 15°)

Fill in the blanks.
(1) The maximum value of the function y = 6 sin x - 8 (x ∈ R) is ______, and the minimum value is ______.
(2) The maximum value of the function y = 2√2 sin x - cos x (x ∈ R) is ______, and the minimum value is ______.
(3) The maximum value of the function y = 2 sin x - 5 cos x (x ∈ R) is ______, and the minimum value is ______.

Given sinα = 1/3, and α is in the second quadrant, find the value of each expression.
(1) sin(α - π/6)
(2) cos(α - π/3)
(3) tan(α + π/4)

Given cosα = 3/5, α is in the fourth quadrant, cosβ = 5/13, β is in the first quadrant, find the value of each expression.
(1) sin(α - β)
(2) cos(α + β)
(3) tan(α + β)

Given sinα = -√5/5, α is in the third quadrant, cosβ = -√10/10, β is in the third quadrant, find the value of each expression.
(1) sin(α + β)
(2) sin(α - β)
(3) cos(α + β)
(4) cos(α - β)
(5) tan(α + β)
(6) tan(α - β)

6.2 Double-Angle Formulas for Sine, Cosine, and Tangent

I. Double-Angle Sine Formula

Setting β = α in sin(α + β) = sinα cosβ + cosα sinβ gives:
sin 2α = 2 sinα cosα (Formula 6-7)

Example 1: Given sinα = 3/5, and α is in the second quadrant, find sin 2α.

Solution: Since sin²α + cos²α = 1, cosα = ±√(1 - sin²α).
Because α is in the second quadrant with sinα = 3/5, cosα = -√(1 - (3/5)²) = -4/5.
Therefore, sin 2α = 2 sinα cosα = 2 * (3/5) * (-4/5) = -24/25.

Example 2: Given cosα = -12/13, and α is in the third quadrant, find sin 2α.

Solution: Since sin²α + cos²α = 1, sinα = ±√(1 - cos²α).
Because α is in the third quadrant with cosα = -12/13, sinα = -√(1 - (12/13)²) = -5/13.
Therefore, sin 2α = 2 sinα cosα = 2 * (-5/13) * (-12/13) = 120/169.

II. Double-Angle Cosine Formula

Setting β = α in cos(α + β) = cosα cosβ - sinα sinβ gives:
cos 2α = cos²α - sin²α = 2 cos²α - 1 = 1 - 2 sin²α (Formula 6-8)

Example 3: Given cosα = -2/5, find cos 2α.

Solution: cos 2α = 2 cos²α - 1 = 2 * (-2/5)² - 1 = 8/25 - 1 = -17/25.

Example 4: Given sinα = 2/3, find cos 2α.

Solution: cos 2α = 1 - 2 sin²α = 1 - 2 * (2/3)² = 1 - 8/9 = 1/9.

III. Double-Angle Tangent Formula

Setting β = α in tan(α + β) = (tanα + tanβ)/(1 - tanα tanβ) gives:
tan 2α = (2 tanα) / (1 - tan²α) (Formula 6-9)

Example 5: Given tanα = -2, find tan 2α.

Solution: tan 2α = (2 * (-2)) / (1 - (-2)²) = -4 / -3 = 4/3.

Example 6: Given tanα = 1/3, find tan 4α.

Solution: First, tan 2α = (2*(1/3)) / (1 - (1/3)²) = (2/3) / (8/9) = 3/4.
Then, tan 4α = (2 * tan 2α) / (1 - tan² 2α) = (2*(3/4)) / (1 - (3/4)²) = (3/2) / (7/16) = 24/7.

Summary

Double-angle sine formula: sin 2α = 2 sinα cosα (Formula 6-7)

Double-angle cosine formula: cos 2α = cos²α - sin²α = 2 cos²α - 1 = 1 - 2 sin²α (Formula 6-8)

Double-angle tangent formula: tan 2α = (2 tanα) / (1 - tan²α) (Formula 6-9)

Exercises 6.2

Given sinα = √3/4, find sin 2α.

Given sinα = -2/3, α is in the fourth quadrant, find sin 2α and cos 2α.

Given cosα = √2/3, α is in the first quadrant, find sin 2α and cos 2α.

Given cosα = √5/3, find cos 2α.

Given sinα = 1/5, find cos 2α.

sin105° cos105° = ( ).
A. 1/4
B. -1/4
C. √3/4
D. -√3/4

(sin15° + cos15°)² = ______.

Given tanα = 4, find tan 2α.

Given tanα = 1/3, find tan 2α.

Given cosα = -√3/3, find cos 4α.

Given tanα = -3, find tan 2α, tan 4α, tan 8α.

Given sinα = 1/3, α is in the second quadrant, find sin 2α, cos 2α, sin 4α.

6.3 Half-Angle Formulas for Sine, Cosine, and Tangent

I. Half-Angle Sine Formula

From cos 2α = 1 - 2 sin²α, we get 2 sin²α = 1 - cos 2α, so sin²α = (1 - cos 2α)/2.
Thus,
sin(α/2) = ±√[(1 - cosα)/2] (Formula 6-10)
The sign is '+' when α/2 is in the first or second quadrant, and '-' when α/2 is in the third or fourth quadrant.

Example 1: Find sin75°.

Solution: Using Formula 6-10:
sin75° = √[(1 - cos150°)/2] = √[(1 - (-√3/2))/2] = √[(2 + √3)/4] = √(2 + √3)/2.

Example 2: Given sinα = -3/5, 3π/2 < α < 2π, find sin(α/2).

Solution: Since 3π/2 < α < 2π, then 3π/4 < α/2 < π (second quadrant).
Also, sinα = -3/5, so cosα = √(1 - sin²α) = √(1 - 9/25) = 4/5.
Therefore, sin(α/2) = √[(1 - cosα)/2] = √[(1 - 4/5)/2] = √(1/10) = √10/10.

II. Half-Angle Cosine Formula

From cos 2α = 2 cos²α - 1, we get 2 cos²α = 1 + cos 2α, so cos²α = (1 + cos 2α)/2.
Thus,
cos(α/2) = ±√[(1 + cosα)/2] (Formula 6-11)
The sign is '+' when α/2 is in the first or fourth quadrant, and '-' when α/2 is in the second or third quadrant.

Example 3: Find cos(π/8).

Solution: Using Formula 6-11:
cos(π/8) = √[(1 + cos(π/4))/2] = √[(1 + √2/2)/2] = √[(2 + √2)/4] = √(2 + √2)/2.

Example 4: Given cosα = -1/3, π < α < 3π/2, find cos(α/2).

Solution: Since π < α < 3π/2, then π/2 < α/2 < 3π/4 (second quadrant).
Therefore, cos(α/2) = -√[(1 + cosα)/2] = -√[(1 - 1/3)/2] = -√(1/3) = -√3/3.

III. Half-Angle Tangent Formula

From Formulas 6-10 and 6-11, we get:
tan(α/2) = ±√[(1 - cosα)/(1 + cosα)] (Formula 6-12)
The sign is '+' when α/2 is in the first or third quadrant, and '-' when α/2 is in the second or fourth quadrant.

We also frequently use these formulas:
tan(α/2) = sinα/(1 + cosα) = (1 - cosα)/sinα (Formula 6-13)

Example 5: Given cosα = -2/3, π/2 < α < π, find tan(α/2).

Solution: Since π/2 < α < π, then π/4 < α/2 < π/2 (first quadrant).
Thus, tan(α/2) = √[(1 - cosα)/(1 + cosα)] = √[(1 - (-2/3))/(1 + (-2/3))] = √[(5/3)/(1/3)] = √5.

Example 6: Given sinα = 3/4, 0 < α < π/2, find tan(α/2).

Solution: Since sinα = 3/4 and 0 < α < π/2, cosα = √(1 - sin²α) = √(1 - 9/16) = √7/4.
Using Formula 6-13: tan(α/2) = (1 - cosα)/sinα = (1 - √7/4) / (3/4) = (4 - √7)/3.

Summary

Half-angle sine formula: sin(α/2) = ±√[(1 - cosα)/2] (Formula 6-10)

Half-angle cosine formula: cos(α/2) = ±√[(1 + cosα)/2] (Formula 6-11)

Half-angle tangent formulas:
tan(α/2) = ±√[(1 - cosα)/(1 + cosα)] (Formula 6-12)
tan(α/2) = sinα/(1 + cosα) = (1 - cosα)/sinα (Formula 6-13)

Exercises 6.3

If cosα = 1/3, α ∈ (0, π/2), then cos(α/2) = ( ).
A. √6/3
B. -√6/3
C. ±√6/3
D. ±√3/3

If cosα = 1/5, α ∈ (3π/2, 2π), then sin(α/2) = ( ).
A. √10/5
B. -√10/5
C. 2√6/5
D. 2√5/5

Given sinα = √3/2, π/2 < α < π, find sin(α/2).

Given sinα = -8/17, 3π/2 < α < 2π, find sin(α/2), cos(α/2), sin 2α.

Given cosα = 5/6, 0 < α < π/2, find sin(α/2), cos(α/2), cos 2α.

Given cosα = -√2/2, π/2 < α < π, find tan(α/2).

Given sinα = -1/5, 3π/2 < α < 2π, find tan(α/2).

Given cosα = -2/3, π < α < 3π/2, find tan(α/2).

Given sinα = 3/5, 0 < α < π/2, find tan(α/2).

6.4 Product-to-Sum and Sum-to-Product Formulas

I. Product-to-Sum Formulas

From:
cos(α + β) = cosα cosβ - sinα sinβ ... (1)
cos(α - β) = cosα cosβ + sinα sinβ ... (2)
(1)+(2) gives: cos(α+β) + cos(α-β) = 2 cosα cosβ
So,
cosα cosβ = (1/2)[cos(α+β) + cos(α-β)] (Formula 6-14)

(1)-(2) gives: cos(α+β) - cos(α-β) = -2 sinα sinβ
So,
sinα sinβ = -(1/2)[cos(α+β) - cos(α-β)] (Formula 6-15)

Similarly, from:
sin(α+β) = sinα cosβ + cosα sinβ
sin(α-β) = sinα cosβ - cosα sinβ
We get:
sinα cosβ = (1/2)[sin(α+β) + sin(α-β)] (Formula 6-16)
cosα sinβ = (1/2)[sin(α+β) - sin(α-β)] (Formula 6-17)

Example 1: Given cos(α+β) = 1/4, cos(α-β) = -2/3, find cosα cosβ, sinα sinβ.

Solution: Using Formula 6-14:
cosα cosβ = (1/2)[cos(α+β) + cos(α-β)] = (1/2)(1/4 - 2/3) = -5/24.
Using Formula 6-15:
sinα sinβ = -(1/2)[cos(α+β) - cos(α-β)] = -(1/2)(1/4 - (-2/3)) = -(1/2)(1/4 + 2/3) = -11/24.

II. Sum-to-Product Formulas

Let α + β = u, α - β = v, then α = (u+v)/2, β = (u-v)/2. Substituting into Formulas 6-14 to 6-17 gives:
cos u + cos v = 2 cos[(u+v)/2] cos[(u-v)/2] (Formula 6-18)
cos u - cos v = -2 sin[(u+v)/2] sin[(u-v)/2] (Formula 6-19)
sin u + sin v = 2 sin[(u+v)/2] cos[(u-v)/2] (Formula 6-20)
sin u - sin v = 2 cos[(u+v)/2] sin[(u-v)/2] (Formula 6-21)

Example 2: Simplify the following expressions.
(1) cos(60° + φ) + cos(60° - φ)
(2) sin(π/4 - φ) - sin(π/4 + φ)

Solution (1): Using Formula 6-18 with u = 60°+φ, v = 60°-φ:
cos(60°+φ) + cos(60°-φ) = 2 cos( ( (60°+φ)+(60°-φ) )/2 ) cos( ( (60°+φ)-(60°-φ) )/2 )
= 2 cos(60°) cos(φ) = cos φ.

Solution (2): Using Formula 6-21 with u = π/4-φ, v = π/4+φ:
sin(π/4-φ) - sin(π/4+φ) = 2 cos( ( (π/4-φ)+(π/4+φ) )/2 ) sin( ( (π/4-φ)-(π/4+φ) )/2 )
= 2 cos(π/4) sin(-φ) = -√2 sin φ.

Summary

Memorize Formulas 6-14 to 6-21.

Exercises 6.4

Given cos(α+β) = -1/2, cos(α-β) = 1/3, find cosα cosβ, sinα sinβ.

Given cos(α+β) = 3/4, cos(α-β) = -2/5, find cosα cosβ, sinα sinβ.

Given sin(α+β) = -1/4, sin(α-β) = 1/5, find sinα cosβ, cosα sinβ.

Given sin(α+β) = √2/3, sin(α-β) = -√2/6, find sinα cosβ, cosα sinβ.

Calculate the value of each expression.
(1) sin105° cos165°
(2) cos165° cos15°

Simplify the following expressions.
(1) sin(60°+φ) + sin(60°-φ)
(2) sin(30°+φ) - sin(30°-φ)
(3) cos(5π/6 + φ) - cos(5π/6 - φ)
(4) cos(3π/4 - φ) + cos(3π/4 + φ)

Self-Test for Chapter 6

Multiple-choice questions.
(1) cos^4(π/8) - sin^4(π/8) = ( ).
A. 0
B. √3/2
C. 1
D. √2/2
(2) If sin(α/2) = 4/5, cos(α/2) = -3/5, then the terminal side of angle α/2 is in the ( ) quadrant.
A. first
B. second
C. third
D. fourth
(3) If α is in the third quadrant, cosα = -5/13, then sin 2α = ( ).
A. 12/13
B. -12/13
C. 120/169
D. -120/169
(4) If 180° < α < 360°, then cos(α/2) = ( ).
A. -√[(1 - cosα)/2]
B. √[(1 - cosα)/2]
C. -√[(1 + cosα)/2]
D. √[(1 + cosα)/2]
(5) cos(210°+φ) - cos(210°-φ) = ( ).
A. sin φ
B. -sin φ
C. √3 cos φ
D. -√3 cos φ

Fill in the blanks.
(1) If cosα = 2/3, π < α < 3π/2, then cos(α/2) = ______.
(2) cos75° = ______.
(3) If sinα = 3/7, then cos 2α = ______.
(4) cos85° cos25° + sin85° sin25° = ______.
(5) If sinα = 3√7/8, π/4 < α < π/2, then sin(α/2) = ______.
(6) If cosα = 4/9, 3π/2 < α < 2π, then sin(α/2) = ______.
(7) If cosα = 1/5, π < α < 3π/2, then tan(α/2) = ______.
(8) The maximum value of the function y = 4 sin x - 6 cos x (x ∈ R) is ______, and the minimum value is ______.
(9) If cosα = 3/4, π < α < 3π/2, then tan 2α = ______.
(10) The minimum positive period of the function y = sin(x+π/6) cos(x+π/6) is ______.
(11) The maximum value of the function y = 2 cos²x + sin 2x, x ∈ R is ______.
(12) If sin(α+β) = 3/4, sin(α-β) = -1/2, then sinα cosβ = ______.

Calculation problems (show your work and give results).
(1) Given sinα = 1/3, α is in the second quadrant, cosβ = -2/3, β is in the third quadrant, find the value of:
① sin(α - β)
② cos(α + β)
③ tan(α - β)
(2) Given α, β ∈ (3π/4, π), sin(α+β) = -3/5, sin(β - π/4) = 12/13, find cos(α + π/4).
(3) Given cos(2α - β) = √2/2, sin(α - 2β) = √2/2, and π/4 < α < π/2, 0 < β < π/4, find cos(α + β).


Chapter 7: Sequences

7.1 The Concept of Sequences and Related Formulas

I. The Concept of a Sequence

Definition 1: A set of numbers arranged in a specific order is called a sequence.

For example, 1, 1/3, 1/5, ..., 1/(2n-1), ... is a sequence; 2, 4, 6, 8 is also a sequence.

Each number in a sequence is called a term. The first number is called the first term (or the initial term), the second number is called the second term, ..., the 
n
nth number is called the 
n
nth term.

The general form of a sequence is: 
a
1
,
a
2
,
a
3
,
.
.
.
,
a
n
,
.
.
.
a 
1
​
 ,a 
2
​
 ,a 
3
​
 ,...,a 
n
​
 ,..., denoted by 
{
a
n
}
{a 
n
​
 }.

For example, in the sequence 1, 1/3, 1/5, ..., 1/(2n-1), ..., the first term is 1, denoted 
a
1
=
1
a 
1
​
 =1; the second term is 1/3, denoted 
a
2
=
1
/
3
a 
2
​
 =1/3; the 
n
nth term is 
1
/
(
2
n
−
1
)
1/(2n−1), denoted 
a
n
=
1
/
(
2
n
−
1
)
a 
n
​
 =1/(2n−1).

A sequence with a finite number of terms is called a finite sequence. A sequence with an infinite number of terms is called an infinite sequence.

For example, 1, 2, 3, 4 is a finite sequence; 1, 2, 3, ..., n is also a finite sequence; 1, 2, 3, ..., n, ... is an infinite sequence.

II. The General Term Formula of a Sequence

Definition 2: If the relationship between the 
n
nth term of a sequence 
{
a
n
}
{a 
n
​
 } and its position 
n
n can be expressed by a formula, then this formula is called the general term formula of the sequence.

For example, the general term formula of the sequence 1, 1/3, 1/5, ..., 1/(2n-1), ... is 
a
n
=
1
/
(
2
n
−
1
)
a 
n
​
 =1/(2n−1); the general term formula of the sequence 1, 2, 3, ..., n, ... is 
a
n
=
n
a 
n
​
 =n; the general term formula of the sequence 2, 4, 6, ..., 2n, ... is 
a
n
=
2
n
a 
n
​
 =2n.

Example 1: Write the general term formula for the following sequences.
(1) 1, -1, 1, -1, 1, ...
(2) 1/(1*2), 1/(3*4), 1/(5*6), ...

Solution:
(1) 
a
n
=
(
−
1
)
n
−
1
a 
n
​
 =(−1) 
n−1
 
(2) 
a
n
=
1
/
[
2
n
(
2
n
−
1
)
]
a 
n
​
 =1/[2n(2n−1)] (This matches the pattern: first term has denominator 1*2=2, second 3*4=12, third 5*6=30, ... The general term uses (2n-1)*(2n) in the denominator and 1 in the numerator)

Example 2: A sequence 
{
a
n
}
{a 
n
​
 } satisfies 
a
1
=
2
a 
1
​
 =2, and subsequent terms are given by the formula 
a
n
=
2
a
n
−
1
+
1
(
n
≥
2
)
a 
n
​
 =2a 
n−1
​
 +1(n≥2). Find the first 4 terms of this sequence.

Solution:
a
1
=
2
a 
1
​
 =2
a
2
=
2
a
1
+
1
=
2
∗
2
+
1
=
5
a 
2
​
 =2a 
1
​
 +1=2∗2+1=5
a
3
=
2
a
2
+
1
=
2
∗
5
+
1
=
11
a 
3
​
 =2a 
2
​
 +1=2∗5+1=11
a
4
=
2
a
3
+
1
=
2
∗
11
+
1
=
23
a 
4
​
 =2a 
3
​
 +1=2∗11+1=23

Example 3: A sequence 
{
a
n
}
{a 
n
​
 } satisfies 
a
1
=
1
,
a
2
=
3
a 
1
​
 =1,a 
2
​
 =3, and subsequent terms are given by the formula 
a
n
=
2
a
n
−
1
+
3
a
n
−
2
(
n
≥
3
)
a 
n
​
 =2a 
n−1
​
 +3a 
n−2
​
 (n≥3). Find the first 6 terms of this sequence.

Solution:
a
1
=
1
,
a
2
=
3
a 
1
​
 =1,a 
2
​
 =3
a
3
=
2
a
2
+
3
a
1
=
2
∗
3
+
3
∗
1
=
9
a 
3
​
 =2a 
2
​
 +3a 
1
​
 =2∗3+3∗1=9
a
4
=
2
a
3
+
3
a
2
=
2
∗
9
+
3
∗
3
=
27
a 
4
​
 =2a 
3
​
 +3a 
2
​
 =2∗9+3∗3=27
a
5
=
2
a
4
+
3
a
3
=
2
∗
27
+
3
∗
9
=
81
a 
5
​
 =2a 
4
​
 +3a 
3
​
 =2∗27+3∗9=81
a
6
=
2
a
5
+
3
a
4
=
2
∗
81
+
3
∗
27
=
243
a 
6
​
 =2a 
5
​
 +3a 
4
​
 =2∗81+3∗27=243

III. The Sum of the First 
n
n Terms Formula

Definition 3: We call 
a
1
+
a
2
+
.
.
.
+
a
n
a 
1
​
 +a 
2
​
 +...+a 
n
​
  the sum of the first 
n
n terms of the sequence 
{
a
n
}
{a 
n
​
 }, denoted 
S
n
S 
n
​
 . That is, 
S
n
=
a
1
+
a
2
+
.
.
.
+
a
n
S 
n
​
 =a 
1
​
 +a 
2
​
 +...+a 
n
​
 .

Example 4: The general term formula of a sequence 
{
a
n
}
{a 
n
​
 } is 
a
n
=
n
2
+
1
a 
n
​
 =n 
2
 +1. Find 
S
4
S 
4
​
 .

Solution: Since 
S
4
=
a
1
+
a
2
+
a
3
+
a
4
S 
4
​
 =a 
1
​
 +a 
2
​
 +a 
3
​
 +a 
4
​
 ,
a
1
=
1
2
+
1
=
2
a 
1
​
 =1 
2
 +1=2
a
2
=
2
2
+
1
=
5
a 
2
​
 =2 
2
 +1=5
a
3
=
3
2
+
1
=
10
a 
3
​
 =3 
2
 +1=10
a
4
=
4
2
+
1
=
17
a 
4
​
 =4 
2
 +1=17
Therefore, 
S
4
=
2
+
5
+
10
+
17
=
34
S 
4
​
 =2+5+10+17=34.

Summary

Concepts.
(1) A set of numbers arranged in a specific order is called a sequence.
(2) A sequence with a finite number of terms is called a finite sequence.
(3) A sequence with an infinite number of terms is called an infinite

It does not paste, do smth with formulas and numbers, make it simple but structured, redo
Chapter 7: Sequences

7.1 The Concept of Sequences and Related Formulas

I. The Concept of a Sequence

Definition 1: A set of numbers arranged in a specific order is called a sequence.

Example: 1, 1/3, 1/5, ..., 1/(2n-1), ... is a sequence; 2, 4, 6, 8 is also a sequence.

Each number in a sequence is called a term. The first number is called the first term (or initial term), the second number is called the second term, ..., the n-th number is called the n-th term.

The general form of a sequence is: a₁, a₂, a₃, ..., a_n, ..., denoted by {a_n}.

Example: In the sequence 1, 1/3, 1/5, ..., 1/(2n-1), ...
First term a₁ = 1
Second term a₂ = 1/3
n-th term a_n = 1/(2n-1)

A sequence with a finite number of terms is called a finite sequence. A sequence with an infinite number of terms is called an infinite sequence.

Example: 1, 2, 3, 4 is a finite sequence.
1, 2, 3, ..., n is also a finite sequence.
1, 2, 3, ..., n, ... is an infinite sequence.

II. The General Term Formula of a Sequence

Definition 2: If the relationship between the n-th term a_n of a sequence and its position n can be expressed by a formula, then this formula is called the general term formula.

Example:
Sequence: 1, 1/3, 1/5, ... → a_n = 1/(2n-1)
Sequence: 1, 2, 3, ... → a_n = n
Sequence: 2, 4, 6, ... → a_n = 2n

Example 1: Write the general term formula for:
(1) 1, -1, 1, -1, 1, ...
(2) 1/(1×2), 1/(3×4), 1/(5×6), ...

Solution:
(1) a_n = (-1)^(n-1)
(2) a_n = 1/[2n(2n-1)]

Example 2: Given sequence {a_n} with a₁ = 2 and a_n = 2a_{n-1} + 1 (for n ≥ 2). Find first 4 terms.

Solution:
a₁ = 2
a₂ = 2a₁ + 1 = 2×2 + 1 = 5
a₃ = 2a₂ + 1 = 2×5 + 1 = 11
a₄ = 2a₃ + 1 = 2×11 + 1 = 23

Example 3: Given sequence {a_n} with a₁ = 1, a₂ = 3 and a_n = 2a_{n-1} + 3a_{n-2} (n ≥ 3). Find first 6 terms.

Solution:
a₁ = 1
a₂ = 3
a₃ = 2a₂ + 3a₁ = 2×3 + 3×1 = 9
a₄ = 2a₃ + 3a₂ = 2×9 + 3×3 = 27
a₅ = 2a₄ + 3a₃ = 2×27 + 3×9 = 81
a₆ = 2a₅ + 3a₄ = 2×81 + 3×27 = 243

III. The Sum of the First n Terms Formula

Definition 3: S_n = a₁ + a₂ + ... + a_n is called the sum of the first n terms of sequence {a_n}.

Example 4: Sequence {a_n} has general term a_n = n² + 1. Find S₄.

Solution:
a₁ = 1² + 1 = 2
a₂ = 2² + 1 = 5
a₃ = 3² + 1 = 10
a₄ = 4² + 1 = 17
S₄ = 2 + 5 + 10 + 17 = 34

Summary

Concepts:

A sequence is numbers in a specific order.

Finite sequence has limited terms.

Infinite sequence has unlimited terms.

General term formula relates a_n to n.

S_n = a₁ + a₂ + ... + a_n is the sum of first n terms.

Exercises 7.1

Which sequences are finite? Which are infinite?
(1) 1, 2, 5, 7, 10
(2) 2, 2, 2, 2, 2, ...
(3) 2, 4, 6, 8, ..., 2n
(4) 5, 10, 20, 30, ...

Write general term formula:
(1) 1/(2×3), 1/(3×4), 1/(4×5), 1/(5×6), ...
(2) 1, 4, 7, 10, ...
(3) 3, 9, 27, 81, ...
(4) 1, -4, 9, -16, ...
(5) 1/(3×5), 2/(5×7), 3/(7×9), 4/(9×11), ...

Fill in blanks:
(1) If a_n = -n² + 7n + 9, then a₄ = ______.
(2) If a_n = n - (n-1)²/2, then a₂ = _____.
(3) If a₁ = 1/2, a_n = 4a{n-1} + 19 (n ≥ 2), then a₂ = ______, a₃ = ______.

Sequence {a_n}: a_n = (-1)^n × n²/[(2n-1)(2n+1)], find a₁, a₂, a₃.

Sequence {a_n}: a_{n+1} = 2a_n + 1, a₁ = 1. Write first 4 terms.

Sequence {a_n}: a₁ = 1, a₂ = 2, a_n = a_{n-1} + a_{n-2} (n ≥ 3). Write first 5 terms.

Sequence {a_n}: a₁ = 1, a_{n+1} = 2a_n/(a_n + 2) (n ≥ 1). Write first 5 terms.

Sequence {a_n}: a₁ = 1, a₂ = 6, a_{n+2} = a_{n+1} - a_n (n ≥ 1). Find a₆.

Sequence {a_n}: S_n = n² + n. Find a₄.

Sequence {a_n}: S_n = -n³ + 1. Find a₅ + a₆ + a₇ + a₈.

7.2 Arithmetic Sequences

I. Concept of Arithmetic Sequence

Look at sequence: 2, 4, 6, 8, 10, 12, 14, 16.
We see: each term minus previous term gives same number. Such sequence is called arithmetic sequence.

Definition 1: If sequence {a_n} satisfies a_n - a_{n-1} = d (n ≥ 2), where d is constant, then {a_n} is an arithmetic sequence, and d is the common difference.

Note: {a_n} is arithmetic ⇔ a₂ - a₁ = a₃ - a₂ = a₄ - a₃ = ... = a_n - a_{n-1} = ... = d.

Examples:
(1) 1, 3, 5, 7, ..., (2n-1), ... is arithmetic, d = 2.
(2) 2, 2, 2, 2, ..., 2, ... is arithmetic, d = 0.
(3) 1, 1/2, 1/3, 1/4 is NOT arithmetic.

Question: Is 0, 0, 0, 0, ... arithmetic? Answer: Yes.

Example 1: Sequence {a_n} with a_n = 3n + 2. Is it arithmetic?

Solution: a_n - a_{n-1} = (3n+2) - [3(n-1)+2] = 3n - 3(n-1) = 3 (constant). Yes.

Example 2: Sequence {a_n} with a_n = n² - 1. Is it arithmetic?

Solution: a_n - a_{n-1} = (n²-1) - [(n-1)²-1] = 2n - 1 (NOT constant). No.

Note: To check if sequence is arithmetic, see if a_n - a_{n-1} is constant.

II. General Term Formula of Arithmetic Sequence

Let {a_n} be arithmetic with common difference d. Then:
a₂ = a₁ + d
a₃ = a₂ + d = a₁ + 2d
a₄ = a₃ + d = a₁ + 3d
...
a_n = a₁ + (n-1)d (Formula 7-1)

Example 3: Arithmetic sequence {a_n} with a₁ = 2, d = -3. Find a₅ and a₁₀.

Solution:
a₅ = a₁ + 4d = 2 + 4×(-3) = -10
a₁₀ = a₁ + 9d = 2 + 9×(-3) = -25

Example 4: Arithmetic sequence {a_n} with a₃ = -5, a₁₁ = 19. Find a₁, d, and a₈.

Solution:
From a_n = a₁ + (n-1)d:
a₃ = a₁ + 2d = -5 ... (1)
a₁₁ = a₁ + 10d = 19 ... (2)

(2) - (1): 8d = 24 → d = 3
Substitute d=3 into (1): a₁ + 2×3 = -5 → a₁ = -11
a₈ = a₁ + 7d = -11 + 7×3 = 10

III. Arithmetic Mean

Definition 2: If three numbers a, b, c form arithmetic sequence (c - b = b - a), then b is the arithmetic mean of a and c.

Formula: b = (a + c)/2 (Formula 7-2)

Example 5: Arithmetic mean of 4 and 20 is (4+20)/2 = 12.

Example 6: In arithmetic sequence {a_n}, arithmetic mean of a₃ and a₉ is ______.

Solution: (a₃ + a₉)/2 = [a₁+2d + a₁+8d]/2 = a₁ + 5d = a₆
Answer: a₆

Note: In arithmetic sequence, arithmetic mean of a_m and a_n is a_{(m+n)/2} (if (m+n)/2 is integer).

Example 7: Arithmetic sequence {a_n} with a₅ + a₆ + a₇ + a₈ + a₉ = 45. Find a₇.

Solution:
a₅ + a₉ = 2a₇
a₆ + a₈ = 2a₇
So: 5a₇ = 45 → a₇ = 9

IV. Sum of First n Terms of Arithmetic Sequence

Let {a_n} be arithmetic with common difference d.
Sum S_n = n(a₁ + a_n)/2 (Formula 7-3)
Also: S_n = n a₁ + n(n-1)d/2 (Formula 7-4)

Example 8: Sequence {a_n} with a_n = n. Find S_n.

Solution 1: It's arithmetic with a₁=1, a_n=n. So S_n = n(1+n)/2.

Solution 2: d=1, a₁=1. S_n = n×1 + n(n-1)×1/2 = n + (n²-n)/2 = (n²+n)/2 = n(n+1)/2.

Example 9: Arithmetic sequence {a_n} with a₁=2, a₄=-10. Find a₈ and S₁₀.

Solution:
a₄ = a₁ + 3d = -10 → 2 + 3d = -10 → d = -4
a₈ = a₁ + 7d = 2 + 7×(-4) = -26
S₁₀ = 10×2 + 10×9×(-4)/2 = 20 - 180 = -160

Summary

{a_n} is arithmetic ⇔ a_n - a_{n-1} = d (constant).

Formulas:
a_n = a₁ + (n-1)d (7-1)
Arithmetic mean: b = (a+c)/2 (7-2)
S_n = n(a₁ + a_n)/2 (7-3)
S_n = n a₁ + n(n-1)d/2 (7-4)

Exercises 7.2

Multiple choice:
(1) Which is NOT arithmetic?
A. 3, 3, 3, ...
B. 2, 8, 14, 20
C. 1/2, 1/4, 1/6, ...
D. 0, 0, 0, ...
(2) Which IS arithmetic?
A. 2, 4, 8, 16, ...
B. ln2, ln4, ln8, ln16
C. lg1, lg2, lg3, lg4, ...
D. 0, 10, 40, 50
(3) Arithmetic {a_n}: a₃=7, a₇=-5, then d=
A. 3 B. -3 C. 2 D. -2
(4) Arithmetic {a_n}: a₂=-5, a₆=a₄+6, then a₁₀=
A. 19 B. 18 C. -19 D. -18
(5) Arithmetic {a_n}: a₂+a₆=8, a₃+a₄=3, then d=
A. 4 B. 5 C. 6 D. 7
(6) Arithmetic mean of √5-2 and √5+2 is
A. 2 B. 1 C. ±√5 D. √5
(7) Arithmetic {a_n}: a₁=21, a₇=18, then d=
A. 1/2 B. 1/3 C. -1/2 D. -1/3
(8) Arithmetic {a_n}: a₂=5, a₆=17, then a₁₄=
A. 45 B. 41 C. 39 D. 37
(9) If m and 2n have arithmetic mean 4, and 2m and n have arithmetic mean 5, then arithmetic mean of m and n is
A. 2 B. 3 C. 6 D. 9
(10) {a_n}: a₁=2, d=3; {b_n}: b₁=-2, d=4. If a_n = b_n, then n=
A. 4 B. 5 C. 6 D. 7
(11) Arithmetic {a_n}: a₂=1, a₃=3, then S₄=
A. 12 B. 10 C. 8 D. 6
(12) Arithmetic {a_n}: a₂+a₅=19, S₅=40, then a₁₀=
A. 24 B. 27 C. 29 D. 48
(13) Arithmetic {a_n}: S₁₀=120, then a₂+a₉=
A. 12 B. 24 C. 36 D. 48

Fill blanks:
(1) Arithmetic mean of -5 and 15 = __.
(2) Arithmetic mean of 4 and -18 = _.
(3) In arithmetic {a_n}, arithmetic mean of a₄ and a₁₈ = , of a₁₀ and a₂₀ = .
(4) Arithmetic {a_n}: a₁₅+a₁₆+a₁₇=39, then a₁₆=.
(5) Arithmetic {a_n}: a₈+a₉+a₁₀+a₁₁+a₁₂=55, then a₁₀=.
(6) {a_n}: a{n+1}=a_n+4 (n≥1), a₁=1, then a_n=.
(7) Arithmetic {a_n}: a₄+a₆=6, S₅=10, then d=__.
(8) If a_n=4n-3, then a₁=__, d=.
(9) Arithmetic {a_n}: a₃=7, a₅=a₂+6, then a₆=.
(10) {a_n}: a₁=-7, a{n+1}=a_n+2 (n≥1), then a₁+a₂+...+a₁₇=_.

Answer questions:
(1) Sequence a_n = n/2 - 3. Is it arithmetic? If yes, find d.
(2) Sequence a_n = n³. Is it arithmetic? If yes, find d.

Calculate:
(1) Arithmetic {a_n}: a₁=2, d=-2. Find a₁₀.
(2) Arithmetic {a_n}: a₄=6, a₁₀=-6. Find a₁, d, a₇.
(3) Arithmetic {a_n}: a₃=-1, a₇=7. Find a₁, d, a₉.
(4) Arithmetic {a_n}: a₁=2, a₁₇=20. Find S₁₇.
(5) Arithmetic {a_n}: a₁=2, d=-4. Find S₁₅.
(6) Arithmetic {a_n}: a₂=5, a₆=21. Find S₁₀.
(7) Arithmetic {a_n}: a₃=-8, a₉=4. Find a₁₂ and S₁₃.

7.3 Geometric Sequences

I. Concept of Geometric Sequence

Look at sequence: 2, 4, 8, 16, 32, 64, 128, 256.
We see: each term divided by previous term gives same number. Such sequence is called geometric sequence.

Definition 1: If sequence {a_n} satisfies a_n/a_{n-1} = q (n ≥ 2), where q ≠ 0 is constant, then {a_n} is a geometric sequence, and q is the common ratio.

Note: {a_n} is geometric ⇔ a₂/a₁ = a₃/a₂ = ... = a_n/a_{n-1} = ... = q.

Examples:
(1) 3, 9, 27, 81, ..., 3^n, ... is geometric, q = 3.
(2) 2, 2, 2, 2, ..., 2, ... is geometric, q = 1.
(3) 1, 3, 3, 9 is NOT geometric.

Question: Is 0, 0, 0, 0, ... geometric? Answer: No.

Example 1: Sequence {a_n} with a_n = 5^n. Is it geometric?

Solution: a_n/a_{n-1} = 5^n/5^{n-1} = 5 (constant). Yes.

Example 2: Sequence {a_n} with a_n = n+1. Is it geometric?

Solution: a_n/a_{n-1} = (n+1)/n (NOT constant). No.

Note: To check if sequence is geometric, see if a_n/a_{n-1} is constant (non-zero).

II. General Term Formula of Geometric Sequence

Let {a_n} be geometric with common ratio q. Then:
a₂ = a₁ q
a₃ = a₂ q = a₁ q²
a₄ = a₃ q = a₁ q³
...
a_n = a₁ q^{n-1} (Formula 7-5)

Example 3: Geometric sequence {a_n} with a₁ = 2, q = 1/2. Find a₃ and a₆.

Solution:
a₃ = a₁ q² = 2 × (1/2)² = 2 × 1/4 = 1/2
a₆ = a₁ q⁵ = 2 × (1/2)⁵ = 2 × 1/32 = 1/16

Example 4: Geometric sequence {a_n} with a₂ = 2, a₄ = 8. Find a₁, q, and a₇.

Solution:
a₂ = a₁ q = 2 ... (1)
a₄ = a₁ q³ = 8 ... (2)

(2) ÷ (1): q² = 4 → q = ±2
If q=2: from (1): 2a₁ = 2 → a₁=1, then a₇ = a₁ q⁶ = 1×64=64
If q=-2: from (1): -2a₁ = 2 → a₁=-1, then a₇ = a₁ q⁶ = -1×64=-64

III. Geometric Mean

Definition 2: If three numbers a, b, c form geometric sequence (c/b = b/a), then b is the geometric mean of a and c.

Formula: b = ±√(a×c) (Formula 7-6)

Example 5: Geometric mean of 4 and 16 is ±√(4×16) = ±8.

Example 6: In geometric sequence {a_n}, geometric mean of a₃ and a₉ is ______.

Solution: ±√(a₃ a₉) = ±√(a₁ q² × a₁ q⁸) = ±√(a₁² q¹⁰) = ± a₁ q⁵ = ± a₆
Answer: ±a₆

Note: In geometric sequence, geometric mean of a_m and a_n is ± a_{(m+n)/2}.

Example 7: Geometric sequence {a_n} with a₅ a₆ a₇ = 8. Find a₆.

Solution: a₅ a₇ = a₆² (since a₆ is geometric mean)
So a₅ a₆ a₇ = a₆³ = 8 → a₆ = 2

IV. Sum of First n Terms of Geometric Sequence

Let {a_n} be geometric with common ratio q.
For q ≠ 1: S_n = a₁ (1 - qⁿ)/(1 - q) (Formula 7-7)
For q = 1: S_n = n a₁ (Formula 7-8)

Example 8: Sequence {a_n} with a_n = 2ⁿ. Find S_n.

Solution: It's geometric with a₁=2, q=2.
S_n = 2(1 - 2ⁿ)/(1-2) = 2(2ⁿ - 1)

Example 9: Geometric sequence {a_n} with a₁=2, a₄=1/4. Find a₆ and S₅.

Solution:
a₄ = a₁ q³ = 1/4 → 2q³ = 1/4 → q³ = 1/8 → q = 1/2
a₆ = a₁ q⁵ = 2 × (1/2)⁵ = 2 × 1/32 = 1/16
S₅ = 2[1 - (1/2)⁵]/(1 - 1/2) = 2[1 - 1/32]/(1/2) = 4 × 31/32 = 31/8

Example 10: Sequence {a_n} with a_n = 3. Find S₁₀.

Solution: It's geometric with q=1, a₁=3.
S₁₀ = 10 × 3 = 30

Summary

{a_n} is geometric ⇔ a_n/a_{n-1} = q (constant, q ≠ 0).

Formulas:
a_n = a₁ q^{n-1} (7-5)
Geometric mean: b = ±√(a×c) (7-6)
For q ≠ 1: S_n = a₁(1 - qⁿ)/(1 - q) (7-7)
For q = 1: S_n = n a₁ (7-8)

Exercises 7.3

Multiple choice:
(1) Which is NOT geometric?
A. 3, 3, 3, ...
B. 4, 8, 16, 32
C. 1/2, 1/4, 1/8, ...
D. 0, 0, 0, ...
(2) Which IS geometric?
A. 2, 4, 6, 8, ...
B. 1, 1/3, 1/9, 1/27, ...
C. lg2, lg4, lg8, lg16, ...
D. 0, 10, 100, 1000
(3) Geometric {a_n}: q=3, a₁=2, then a₄=
A. 80 B. 81 C. 54 D. 53
(4) Geometric {a_n}: a₂=8, a₅=64, then q=
A. 2 B. 3 C. 4 D. 8
(5) Geometric {a_n}: a_n>0, a₁+a₂=1, a₃+a₄=9, then a₄+a₅=
A. 16 B. 27 C. 36 D. 81
(6) Geometric {a_n}: a₅=5, a₈=25, then a₂=
A. ∛5 B. √5 C. 5 D. 1

Fill blanks:
(1) Geometric {a_n}: a₅=2, a₁₀=6, then a₂₅=.
(2) Geometric {a_n}: a₃=4, a₇=16, then a₅=.
(3) Arithmetic {a_n} with d=2, and a₁, a₂, a₄ form geometric sequence, then a₁=.
(4) Geometric {a_n} with q>0, a₃ a₉ = 2a₅², a₂=1, then a₁=.
(5) Geometric {a_n}: a₂=1, a₈ = a₆ + 2a₄, then a₆=______.
(6) Geometric mean of -4 and -16 = ______.
(7) Geometric mean of √6-2 and √6+2 = ______.
(8) In geometric {a_n}, geometric mean of a₄ and a₁₈ = , of a₁₀ and a₂₀ = .
(9) Geometric {a_n}: a₁₅ a₁₆ a₁₇ = 27, then a₁₆=.
(10) Geometric {a_n}: a₈ a₉ a₁₀ a₁₁ a₁₂ = 32, then a₁₀=.

Answer questions:
(1) Sequence a_n = (-3)^n. Is it geometric? If yes, find q.
(2) Sequence a_n = 2n. Is it geometric? If yes, find q.

Calculate:
(1) Geometric {a_n}: a₁=2, a₄=-54. Find S₅.
(2) Sequence a_n = 5. Find S₁₅.
(3) Geometric {a_n}: a₂=5, a₄=45. Find S₄.
(4) Geometric {a_n}: a₃=-8, a₅=-2. Find a₄ and S₄.

Self-Test for Chapter 7

Multiple choice:
(1) If a_n = 2n² - 3, then a₄=
A. 28 B. 29 C. 30 D. 31
(2) Arithmetic mean of 2 and 14 is
A. 10 B. 8 C. ±8 D. 6
(3) Sequence 0, 2/3, 4/5, 6/7, ... has general term
A. a_n = (n-1)/(n+1)
B. a_n = (n-1)/(2n+1)
C. a_n = 2(n-1)/(2n-1)
D. a_n = 2n/(2n+1)
(4) If S_n = n/(n+1) for sequence {a_n}, then a₅=
A. 5/6 B. 6/5 C. 1/30 D. 30
(5) Arithmetic {a_n}: a₂ + a₁₂ = 10, then a₇=
A. 5 B. 6 C. 7 D. 8
(6) Which is geometric?
A. 2, 4, 6
B. 2², 2⁴, 2⁸
C. ln2, ln4, ln8
D. √2, -2, 2√2

Fill blanks:
(1) Arithmetic {a_n}: a₅ + a₆ = 25, then S₁₀ = _____.
(2) {a_n}: a₁=1, a{n+1} - a_n = 2 (n≥1), then a_n = _____.
(3) {a_n}: a₁=1, a{n+1} = a_n + 1/(n²+n) (n≥1), then a₃ = ______.
(4) Geometric mean of √2 and 2√2 = ______.
(5) In arithmetic {a_n}, arithmetic mean of a₁₀ and a₅₀ = ______.
(6) In geometric {a_n}, geometric mean of a₃₀ and a₄₀ = ______.
(7) Arithmetic {a_n}: a₂=0, a₆=-12, then S₇ = ______.
(8) Geometric {a_n}: a₃=1/3, a₅=1/27, then S₄ = ______.
(9) 1+2+3+...+100 = ______.
(10) 3+9+27+...+3²⁰ = ______.

Calculate:
(1) {a_n}: a₁=1, a₂=3, a_n = a_{n-1} + 2a_{n-2} (n≥3). Find a₅.
(2) Geometric {a_n}: a₂=3, a₅=24.
① Find a₁ and q.
② Find S₅.
(3) Arithmetic {a_n}: a₃=10, a₆=19.
① Find a₁ and d.
② Find S₁₀.


Chapter 8: Complex Numbers

8.1 Definition of Complex Numbers

I. Concept of Complex Numbers

Definition 1: When a and b are real numbers, the expression a + bi is called a complex number. i is called the imaginary unit, with the property i² = -1.

Complex numbers are usually denoted by lowercase letter z: z = a + bi (a, b ∈ R).

Here:

a is called the real part of z, denoted Re(z)

b is called the imaginary part of z, denoted Im(z)

√(a² + b²) is called the modulus (or absolute value) of z, denoted |z| or |a+bi|

So: Re(z) = a, Im(z) = b, |z| = √(a² + b²)

The set of all complex numbers is called the set of complex numbers, denoted by C: C = {z | z = a + bi, a, b ∈ R}.

Examples:
z₁ = 3 + 4i: Re(z₁) = 3, Im(z₁) = 4, |z₁| = √(3² + 4²) = 5
z₂ = -1 + 2i: Re(z₂) = -1, Im(z₂) = 2, |z₂| = √((-1)² + 2²) = √5
z₃ = 4 - 6i: Re(z₃) = 4, Im(z₃) = -6, |z₃| = √(4² + (-6)²) = 2√13

Example 1: Complex number z = -6 - 8i has real part = ______, imaginary part = ______, modulus = ______.

Solution: Real part = -6, imaginary part = -8, modulus = √((-6)² + (-8)²) = 10.

For complex number z = a + bi (a, b ∈ R), there are three cases:

When b = 0: z = a is a real number. Example: z = 2 is real.

When b ≠ 0: z = a + bi is called an imaginary number. Example: z = 1 + 5i is imaginary.

When a = 0 and b ≠ 0: z = bi is called a purely imaginary number. Example: z = -3i is purely imaginary.

Note: A purely imaginary number is always an imaginary number.

Example 2: Find the real number x such that complex number z = (x+3) + (x-1)i satisfies:
(1) z is a real number
(2) z is an imaginary number
(3) z is a purely imaginary number

Solution:
(1) z is real when imaginary part = 0: x - 1 = 0 → x = 1
(2) z is imaginary when imaginary part ≠ 0: x - 1 ≠ 0 → x ≠ 1
(3) z is purely imaginary when real part = 0 and imaginary part ≠ 0:
x + 3 = 0 and x - 1 ≠ 0 → x = -3

The set of real numbers R is a proper subset of the set of complex numbers C: R ⊂ C.

Relationship between number sets:
Complex numbers (a+bi)
→ Real numbers (b=0)
→ Rational numbers
→ Integers
→ Fractions
→ Irrational numbers
→ Imaginary numbers (b≠0)
→ Purely imaginary (a=0)
→ Non-purely imaginary (a≠0)

II. Equality of Complex Numbers

Definition 2: Two complex numbers a + bi and c + di are equal if and only if their real parts and imaginary parts are respectively equal: a + bi = c + di ⇔ a = c and b = d (where a, b, c, d ∈ R).

In particular: a + bi = 0 ⇔ a = 0 and b = 0.

Note: Two complex numbers can only be equal or not equal; they cannot be compared using < or >.

Example 3: Find real numbers x and y satisfying:
(1) x + 2i = 3 - yi
(2) (x+y) + (x-y-1)i = (2x-3) + (x-2y+3)i
(3) (x+y-3) + (2x-y)i = 0

Solution:
(1) x = 3, y = -2
(2) From equality: x+y = 2x-3 and x-y-1 = x-2y+3
Solve: x = 7, y = 4
(3) From a+bi=0 condition: x+y-3 = 0 and 2x-y = 0
Solve: x = 1, y = 2

III. Conjugate Complex Numbers

Definition 3: Two complex numbers are called conjugate if their real parts are equal and their imaginary parts are opposites.

The conjugate of complex number z is denoted by z̄ (z with a bar over it).

If z = a + bi (a, b ∈ R), then z̄ = a - bi.

Clearly, |z| = |z̄|.

Example: z = 1 + 3i, conjugate z̄ = 1 - 3i, |z| = |z̄| = √(1² + 3²) = √10.

Example 4: If z = -2 - i, then z̄ = ______, |z̄| = ______.

Solution: z̄ = -2 + i, |z̄| = √((-2)² + 1²) = √5.

Summary

Complex number form: z = a + bi (a, b ∈ R)
Re(z) = a, Im(z) = b, |z| = √(a² + b²)

The set of all complex numbers is C. Number relationships:
Complex numbers
→ Real numbers (b=0)
→ Rational
→ Irrational
→ Imaginary numbers (b≠0)
→ Purely imaginary (a=0)
→ Non-purely imaginary (a≠0)

Equality: a+bi = c+di ⇔ a=c, b=d
In particular: a+bi=0 ⇔ a=0, b=0

Conjugate: If z = a+bi, then z̄ = a-bi, and |z| = |z̄|.

Exercises 8.1

Fill in blanks:
(1) Among these numbers: -1, √3 i, 2+√5, -2-i, √2+3i, 0, (1/2)i
Real numbers: ______
Imaginary numbers: ______
Purely imaginary numbers: ______
(2) z = 3-2i: Re(z)=, Im(z)=, |z|=______
(3) z = -5-12i: Re(z)=, Im(z)=, |z|=______
(4) z = -4i: Re(z)=, Im(z)=, |z|=______
(5) z = 6+2i: z̄=, |z̄|=
(6) z = -3+8i: z̄=, |z̄|=
(7) z = 7i: z̄=, |z̄|=
(8) For real x,y: (x+1)+(y-2)i = (2x+5)+(3y-4)i, then x=, y=
(9) For real x,y: (3x+y-2)+(x+2y)i = (x+y)+(2x-3y-2)i, then x=, y=
(10) For real x,y: (x-y-4)i = (x+y-1)+(2y+1)i, then x=, y=

Multiple choice:
(1) Which statement is incorrect?
A. N ⊆ Z B. Z ⊆ Q C. Q ⊆ R D. R ⊈ C
(2) If (x-2)+(x+1)i is real, then x=
A. 2 B. 1 C. -1 D. -2
(3) If (x+2)+(x²+x-6)i is purely imaginary, then x=
A. -2 B. 2 C. 3 D. 1
(4) If (x²-5x+4)+(x-4)i is imaginary, then correct is:
A. x ≠ 1 B. x ≠ 2 C. x ≠ 4 D. x ≠ 5

Answer questions:
(1) Complex number z has real and imaginary parts that are opposites, and |z|=5√2. Find z.
(2) z = a+bi (a,b∈R) satisfies |z|=√5 and b=-2a. Find z.

8.2 Arithmetic Operations with Complex Numbers

I. Addition of Complex Numbers

Let z₁ = a+bi, z₂ = c+di (a,b,c,d ∈ R) be any two complex numbers. Their sum z₁+z₂ is defined as:
z₁ + z₂ = (a+bi) + (c+di) = (a+c) + (b+d)i

In particular: z + z̄ = (a+bi) + (a-bi) = 2a

The sum of two complex numbers is always a complex number. Addition satisfies:

Commutative law: z₁ + z₂ = z₂ + z₁

Associative law: (z₁+z₂)+z₃ = z₁+(z₂+z₃)

Example 1: z₁ = 2+3i, z₂ = -4+5i, then z₁+z₂ = ______.

Solution: (2+3i) + (-4+5i) = (2-4) + (3+5)i = -2+8i

Example 2: z = 6-2i, then z+z̄ = ______.

Solution: (6-2i) + (6+2i) = 12

II. Subtraction of Complex Numbers

For z = a+bi, its opposite is -z = -(a+bi) = -a-bi.

The difference z₁ - z₂ = z₁ + (-z₂), so:
z₁ - z₂ = (a+bi) - (c+di) = (a-c) + (b-d)i

In particular: z - z̄ = (a+bi) - (a-bi) = 2bi

The difference of two complex numbers is always a complex number. Subtraction is not commutative: z₁-z₂ ≠ z₂-z₁.

Example 3: z₁ = -5+2i, z₂ = 1+4i, then z₁-z₂ = ______.

Solution: (-5+2i) - (1+4i) = (-5-1) + (2-4)i = -6-2i

III. Multiplication of Complex Numbers

The product z₁z₂ is defined as:
z₁z₂ = (a+bi)(c+di) = ac + adi + bci + bdi²
Since i² = -1: = (ac - bd) + (ad + bc)i

In particular: z z̄ = (a+bi)(a-bi) = a² + b²

Multiplication satisfies:

Commutative: z₁z₂ = z₂z₁

Associative: (z₁z₂)z₃ = z₁(z₂z₃)

Distributive: z₁(z₂+z₃) = z₁z₂ + z₁z₃

(z₁+z₂)² = z₁² + 2z₁z₂ + z₂²

(z₁+z₂)(z₁-z₂) = z₁² - z₂²

Example 4: z₁ = 1-2i, z₂ = -3+4i, then z₁z₂ = ______.

Solution: (1-2i)(-3+4i) = 1×(-3) + 1×4i + (-2i)×(-3) + (-2i)×4i
= -3 + 4i + 6i - 8i² = -3 + 10i + 8 = 5 + 10i

Example 5: z = 4-2i, then z² = ______.

Solution: (4-2i)² = 4² - 2×4×2i + (2i)² = 16 - 16i - 4 = 12 - 16i

IV. Division of Complex Numbers

For z₁ = a+bi, z₂ = c+di ≠ 0, the quotient z₁/z₂ is calculated by multiplying numerator and denominator by conjugate of denominator:

z₁/z₂ = (a+bi)/(c+di) = [(a+bi)(c-di)]/[(c+di)(c-di)] = [(ac+bd)+(bc-ad)i]/(c²+d²)

So: z₁/z₂ = (ac+bd)/(c²+d²) + [(bc-ad)/(c²+d²)]i

Example 6: (3-7i)/(2+5i) = ______.

Solution: = [(3-7i)(2-5i)]/[(2+5i)(2-5i)] = [6-15i-14i+35i²]/[4+25]
= [6-29i-35]/29 = [-29-29i]/29 = -1 - i

Example 7: z = 3-5i, then z/z̄ = ______.

Solution: z/z̄ = (3+5i)/(3-5i) = [(3+5i)²]/[(3-5i)(3+5i)] = [9+30i+25i²]/[9+25]
= [9+30i-25]/34 = [-16+30i]/34 = -8/17 + (15/17)i

For quadratic equation with real coefficients: ax²+bx+c=0 (a,b,c∈R)
When discriminant Δ = b²-4ac < 0, the equation has no real roots, but has two complex roots:

x₁,₂ = [-b ± √(b²-4ac)]/(2a) = [-b ± √(-(4ac-b²))]/(2a) = [-b ± √(4ac-b²) i]/(2a)

Example 8: Solve in complex numbers:
(1) x²+1=0
(2) x²+3x+5=0

Solution:
(1) x² = -1 → x = ±i
(2) x = [-3 ± √(9-20)]/2 = [-3 ± √(-11)]/2 = [-3 ± √11 i]/2 = -3/2 ± (√11/2)i

Summary

Operations for z₁=a+bi, z₂=c+di:

Addition: (a+bi)+(c+di) = (a+c)+(b+d)i

Subtraction: (a+bi)-(c+di) = (a-c)+(b-d)i

Multiplication: (a+bi)(c+di) = (ac-bd)+(ad+bc)i

Division: (a+bi)/(c+di) = [(ac+bd)+(bc-ad)i]/(c²+d²)

Quadratic equation ax²+bx+c=0 (a,b,c∈R) with Δ<0 has complex roots:
x = [-b ± √(4ac-b²) i]/(2a)

Exercises 8.2

Calculate:
(1) (4+2i)+(-3+5i)
(2) -4i+(3+9i)+(-6-5i)
(3) (-1+3i)+(2-5i)-(-8+7i)
(4) 2-(5-2i)+(-3-8i)

Calculate:
(1) i(-4+3i)
(2) (5-2i)(-4+9i)
(3) (4-3i)(4+3i)
(4) (6+5i)²
(5) (1+i)(2-3i)(-4+5i)
(6) (2+i)⁴
(7) (2-i)/(4+3i)
(8) (4-2i)/i
(9) (5+3i)/(2+i)
(10) [(1-i)/(1+i)]²

Given z₁=1-2i, z₂=3+4i, and 1/z = 1/z₁ + 1/z₂, find z.

Solve in complex numbers:
(1) x²-2x+6=0
(2) 2x²+x+3=0
(3) 3x²-4x+3=0

x₁,x₂ are roots of 4x²-2x+1=0. Find |x₁-x₂|².

Self-Test for Chapter 8

Fill in blanks:
(1) Among: π, 1+√2 i, -3, 2i, -√5+i, 6, (5/3)i
Real: ______
Imaginary: ______
Purely imaginary: ______
(2) z = -4+2i: Re(z)=, Im(z)=, |z|=______
(3) z = 1/2 + (3/2)i: Re(z)=, Im(z)=, |z|=______
(4) z = 3-4i: z̄=, |z̄|=
(5) For real x,y: (2x+y-3)+(x-y-2)i = (x-2y+5)+(2x-4)i, then x=, y=
(6) (-3+2i)-(4+i)+(-2+5i)=______
(7) (4-2i)(-5+3i)=______
(8) (1-3i)/(2+5i)=______
(9) (2-i)²/(1+2i)=______
(10) Solutions of 4x²-4x+3=0 in complex numbers: ______

Multiple choice:
(1) If (x²+x-2)+(x²-3x+2)i is purely imaginary, then x=
A. 1 B. 2 C. -2 D. 1 or -2
(2) If z satisfies z-2i = 4-3i, then z̄=
A. 4-i B. 4+i C. 4-5i D. 4+5i
(3) If z satisfies 3(z+z̄)+2(z-z̄)=2+3i, then z=
A. 1/3 + (3/4)i B. 1/2 + (3/4)i C. 1/2 + (4/3)i D. 1/3 + (4/3)i
(4) If z=3+2i, then z(z̄-3i)=
A. -1+9i B. 19+9i C. 19-9i D. -1-9i
(5) If z=1+2i+3i²+4i³, then |z|=
A. 2√5 B. 2√2 C. 2√10 D. 2√13
(6) If (1+i)x = 3+2i, then x=
A. 3+i B. 3-i C. -3+i D. -3-i
(7) Real part of (5-4i)/(2+i) is
A. 6/5 B. 13/5 C. 14/3 D. 13/3
(8) If z=-1+√2 i, then z/(z z̄-1)=
A. 1/3 + (√2/3)i B. -1+√2 i C. 1/2 + (√2/2)i D. 1/4 + (√2/4)i
(9) If z satisfies z+|z|=2+8i, then z=
A. -12+8i B. -13+6i C. -15+8i D. -14+6i

Chapter 9: Lines in the Plane

9.1 Inclination Angle and Slope of a Line

I. Inclination Angle of a Line

Definition 1: When a line intersects the x-axis, the angle between the positive x-axis and the upward direction of the line is called the inclination angle of the line.

Special case: When the line is parallel to or coincides with the x-axis, its inclination angle is defined as 0°.

Range of inclination angle α: 0° ≤ α < 180°.

II. Slope of a Line

Definition 2: When the inclination angle α ≠ 90°, we define slope k = tan α.

Special case: When α = 90°, the slope does not exist (is undefined).

Example 1: If inclination angle = 45°, then slope k = tan45° = 1.

Example 2: If inclination angle = 150°, then k = tan150° = -√3/3.

Formula: If line passes through points P₁(x₁,y₁) and P₂(x₂,y₂), then its slope is:
k = (y₂ - y₁)/(x₂ - x₁), provided x₁ ≠ x₂.

If x₁ = x₂, the slope does not exist, and the inclination angle is 90°.

Example 3: Line l passes through P₁(2,-3) and P₂(-4,3). Find slope and inclination angle.

Solution: k = [3 - (-3)]/[-4 - 2] = 6/(-6) = -1
tan α = -1 → α = 135° (or 3π/4 radians)

Example 4: Line l passes through P₁(-1,3) and P₂(-1,-2). Find inclination angle.

Solution: Since x₁ = x₂ = -1, the line is vertical, so α = 90°.

Summary

For line with inclination angle α and slope k:

0° ≤ α < 180°

If α ≠ 90°: k = tan α

If α = 90°: k does not exist

Line through P₁(x₁,y₁) and P₂(x₂,y₂):

If x₁ ≠ x₂: k = (y₂-y₁)/(x₂-x₁)

If x₁ = x₂: slope does not exist, α = 90°

Exercises 9.1

Multiple choice:
(1) If inclination α = 30°, then k=
A. 0 B. √3/3 C. 1 D. √3
(2) Points M(3,5) and N(-1,-3). Slope of line through M,N is:
A. 2 B. 1 C. 1/2 D. does not exist
(3) If |k| = √3, then α=
A. 60° B. 30° C. 60° or 120° D. 30° or 150°

Fill blanks:
(1) α=60° → k=______
(2) α=120° → k=______
(3) α=150° → k=______
(4) Through A(√3,-2) and B(0,1): k=______
(5) Through A(1,3) and B(4,7): k=______
(6) Through A(0,-3) and B(-2,0): k=______
(7) Through (-2,1) and (1,4): α=______
(8) Line x=-1: α=______
(9) Line y+7=0: α=______
(10) Through A(m,2) and B((3/2)m, 2m-1) with α=45°, then m=______

9.2 Equations of Lines

I. Point-Slope Form

If line passes through point P₀(x₀,y₀) and has slope k, its equation is:
y - y₀ = k(x - x₀) (Formula 9-1)

Example 1: Line through P₀(-1,2) with k=3: y-2 = 3(x+1)

Example 2: Line through P₀(2,-3) with α=60°: k=tan60°=√3
Equation: y+3 = √3(x-2)

Special cases:

If k=0: line is horizontal: y = y₀

If α=90° (k undefined): line is vertical: x = x₀

Example 3: Line through P₀(1,2) with α=0°: k=0 → y=2

Example 4: Line through P₀(1,2) with α=90°: x=1

II. Slope-Intercept Form

If line passes through (0,b) with slope k, equation is:
y = kx + b (Formula 9-2)
where b is y-intercept.

Example 5: Line through (0,2) with k=3: y=3x+2

Example 6: Line with y-intercept -3 and k=1/2: y=(1/2)x-3

III. Two-Point Form

Line through P₁(x₁,y₁) and P₂(x₂,y₂):
(y - y₁)/(y₂ - y₁) = (x - x₁)/(x₂ - x₁) (Formula 9-3)
(provided x₁≠x₂, y₁≠y₂)

Example 7: Line through P₁(-1,3) and P₂(2,-3):
(y-3)/(-3-3) = (x+1)/(2+1) → (y-3)/(-6) = (x+1)/3

Alternative using point-slope: k=(-3-3)/(2+1)=-2
y-3 = -2(x+1) → y=-2x+1

IV. Intercept Form

Line with x-intercept a and y-intercept b:
x/a + y/b = 1 (Formula 9-4)

Example 8: Line with x-intercept 5, y-intercept -3: x/5 + y/(-3) = 1

V. General Form

Any line can be written as:
Ax + By + C = 0 (Formula 9-5)
where A,B,C are constants, A and B not both zero.

Example: y=3x+2 → 3x - y + 2 = 0

Example 9: Write x/2 + y/3 = 1 in general form.

Solution: Multiply by 6: 3x + 2y = 6 → 3x + 2y - 6 = 0

Chapter 9: Lines in the Plane

9.1 Inclination Angle and Slope of a Line

I. Inclination Angle of a Line

Definition 1: When a line intersects the x-axis, the angle between the positive x-axis and the upward direction of the line is called the inclination angle of the line.

Special case: When the line is parallel to or coincides with the x-axis, its inclination angle is defined as 0°.

Range of inclination angle α: 0° ≤ α < 180°.

II. Slope of a Line

Definition 2: When the inclination angle α ≠ 90°, we define slope k = tan α.

Special case: When α = 90°, the slope does not exist (is undefined).

Example 1: If inclination angle = 45°, then slope k = tan45° = 1.

Example 2: If inclination angle = 150°, then k = tan150° = -√3/3.

Formula: If line passes through points P₁(x₁,y₁) and P₂(x₂,y₂), then its slope is:
k = (y₂ - y₁)/(x₂ - x₁), provided x₁ ≠ x₂.

If x₁ = x₂, the slope does not exist, and the inclination angle is 90°.

Example 3: Line l passes through P₁(2,-3) and P₂(-4,3). Find slope and inclination angle.

Solution: k = [3 - (-3)]/[-4 - 2] = 6/(-6) = -1
tan α = -1 → α = 135° (or 3π/4 radians)

Example 4: Line l passes through P₁(-1,3) and P₂(-1,-2). Find inclination angle.

Solution: Since x₁ = x₂ = -1, the line is vertical, so α = 90°.

Summary

For line with inclination angle α and slope k:

0° ≤ α < 180°

If α ≠ 90°: k = tan α

If α = 90°: k does not exist

Line through P₁(x₁,y₁) and P₂(x₂,y₂):

If x₁ ≠ x₂: k = (y₂-y₁)/(x₂-x₁)

If x₁ = x₂: slope does not exist, α = 90°

Exercises 9.1

Multiple choice:
(1) If inclination α = 30°, then k=
A. 0 B. √3/3 C. 1 D. √3
(2) Points M(3,5) and N(-1,-3). Slope of line through M,N is:
A. 2 B. 1 C. 1/2 D. does not exist
(3) If |k| = √3, then α=
A. 60° B. 30° C. 60° or 120° D. 30° or 150°

Fill blanks:
(1) α=60° → k=______
(2) α=120° → k=______
(3) α=150° → k=______
(4) Through A(√3,-2) and B(0,1): k=______
(5) Through A(1,3) and B(4,7): k=______
(6) Through A(0,-3) and B(-2,0): k=______
(7) Through (-2,1) and (1,4): α=______
(8) Line x=-1: α=______
(9) Line y+7=0: α=______
(10) Through A(m,2) and B((3/2)m, 2m-1) with α=45°, then m=______

9.2 Equations of Lines

I. Point-Slope Form

If line passes through point P₀(x₀,y₀) and has slope k, its equation is:
y - y₀ = k(x - x₀) (Formula 9-1)

Example 1: Line through P₀(-1,2) with k=3: y-2 = 3(x+1)

Example 2: Line through P₀(2,-3) with α=60°: k=tan60°=√3
Equation: y+3 = √3(x-2)

Special cases:

If k=0: line is horizontal: y = y₀

If α=90° (k undefined): line is vertical: x = x₀

Example 3: Line through P₀(1,2) with α=0°: k=0 → y=2

Example 4: Line through P₀(1,2) with α=90°: x=1

II. Slope-Intercept Form

If line passes through (0,b) with slope k, equation is:
y = kx + b (Formula 9-2)
where b is y-intercept.

Example 5: Line through (0,2) with k=3: y=3x+2

Example 6: Line with y-intercept -3 and k=1/2: y=(1/2)x-3

III. Two-Point Form

Line through P₁(x₁,y₁) and P₂(x₂,y₂):
(y - y₁)/(y₂ - y₁) = (x - x₁)/(x₂ - x₁) (Formula 9-3)
(provided x₁≠x₂, y₁≠y₂)

Example 7: Line through P₁(-1,3) and P₂(2,-3):
(y-3)/(-3-3) = (x+1)/(2+1) → (y-3)/(-6) = (x+1)/3

Alternative using point-slope: k=(-3-3)/(2+1)=-2
y-3 = -2(x+1) → y=-2x+1

IV. Intercept Form

Line with x-intercept a and y-intercept b:
x/a + y/b = 1 (Formula 9-4)

Example 8: Line with x-intercept 5, y-intercept -3: x/5 + y/(-3) = 1

V. General Form

Any line can be written as:
Ax + By + C = 0 (Formula 9-5)
where A,B,C are constants, A and B not both zero.

Example: y=3x+2 → 3x - y + 2 = 0

Example 9: Write x/2 + y/3 = 1 in general form.

Solution: Multiply by 6: 3x + 2y = 6 → 3x + 2y - 6 = 0
Solve: x/2 + y/3 = 1 ⇒ 3x+2y=6 ⇒ 3x+2y-6=0.

Note 6: If { Ax+By+C=0, B≠0 } ⇒ y = -(A/B)x - (C/B) ⇒ Slope k = -A/B.

Example 10: The slope *k* of the line 2x - 4y + 1 = 0 is ______.

Solution: Since 2x - 4y + 1 = 0 ⇒ 4y = 2x + 1 ⇒ y = (1/2)x + 1/4,
Therefore, k = 1/2.

Example 11: The x-intercept of the line 3x + 4y - 5 = 0 is ______, and the y-intercept is ______.

Solution:
{ 3x + 4y - 5 = 0, y = 0 } ⇒ 3x - 5 = 0 ⇒ x = 5/3.
So the x-intercept of the line 3x + 4y - 5 = 0 is 5/3.
{ 3x + 4y - 5 = 0, x = 0 } ⇒ 4y - 5 = 0 ⇒ y = 5/4.
So the y-intercept of the line 3x + 4y - 5 = 0 is 5/4.

Summary

Write the equation of a line based on known conditions:

y - y_0 = k(x - x_0) (Point-slope form) Formula 9-1

y = kx + b (Slope-intercept form) Formula 9-2

(y - y_1)/(y_2 - y_1) = (x - x_1)/(x_2 - x_1) (where x_1 ≠ x_2, y_1 ≠ y_2) (Two-point form) Formula 9-3

x/a + y/b = 1 (where a ≠ 0, b ≠ 0) (Intercept form) Formula 9-4

Ax + By + C = 0 (where A, B, C are constants and A, B are not both zero) (General form) Formula 9-5

Find the slope from the general form of a line:
{ Ax + By + C = 0, B ≠ 0 } ⇒ y = -(A/B)x - (C/B) ⇒ k = -A/B.

Find the x-intercept and y-intercept of the line Ax + By + C = 0 (A ≠ 0, B ≠ 0, C ≠ 0):
{ Ax + By + C = 0, y = 0 } ⇒ Ax + C = 0 ⇒ Ax = -C ⇒ x = -C/A (x-intercept);
{ Ax + By + C = 0, x = 0 } ⇒ By + C = 0 ⇒ By = -C ⇒ y = -C/B (y-intercept).

Exercise 9.2

Multiple Choice (For each question, only one of the four options is correct).
(1) The inclination angle of the line x - √3y - 5 = 0 is ( ).
A. 30° B. 60° C. 120° D. 150°
(2) The equation of the line with intercepts -3 on the x-axis and 4 on the y-axis is ( ).
A. x/(-3) + y/4 = 1 B. x/3 + y/(-4) = 1 C. x/(-3) - y/4 = 1 D. x/4 + y/(-3) = 1
(3) The equation of the line passing through points (-1, 2) and (2, 1) is ( ).
A. 3x + y - 5 = 0 B. x + 3y - 5 = 0 C. x - 3y + 5 = 0 D. 3x - y + 5 = 0
(4) Given the line 2x + y - 3 = 0 has an inclination angle α, then (sinα + cosα)/(sinα - cosα) = ( ).
A. -3 B. -2 C. 1/3 D. 3
(5) As shown in Figure 9.2-4, let the slopes of lines l1, l2, l3 be k1, k2, k3 respectively. Then the relationship between k1, k2, k3 is ( ).
A. k1 < k2 < k3 B. k1 < k3 < k2 C. k2 < k1 < k3 D. k3 < k2 < k1

Fill in the blanks.
(1) The equation of the line passing through point (4, -2) with an inclination angle of 135° is ______.
(2) The equation of the line passing through point (-5, 2) with an inclination angle of 0° is ______.
(3) The equation of the line passing through point (-1, 3) with an inclination angle of 90° is ______.
(4) The equation of the line passing through point A(2, -1) with slope 1/3 is ______.
(5) If line l passes through the origin and has a slope of -5, then the equation of l is ______.
(6) The equation of the line with the same slope as line 3x - 2y = 0 and passing through point (-4, 3) is ______.
(7) The equation of the line passing through point (-1, 1) with a slope that is twice the slope of the line y = (√2/2)x - 2 is ______.
(8) If line l1 passes through point A(-1, -2) and its inclination angle is twice that of line l2: y = (√3/3)x, then the equation of l1 is ______.
(9) If line l has a y-intercept of 2 and a slope of -1/3, then the equation of l is ______.
(10) If line l passes through points P1(-3, 1) and P2(2, 5), then the equation of l is ______.
(11) If line l passes through points P1(1, 5) and P2(-2, 8), then the equation of l is ______.
(12) The equation of the line with an x-intercept of 2 and a y-intercept of -2 is ______.
(13) If the equation of line l is y = 3x - 1, then the slope of l is ______, and the y-intercept is ______.
(14) If the equation of line l is 2x + 4y + 1 = 0, then the slope of l is ______, the x-intercept is ______, and the y-intercept is ______.
(15) If the equation of line l is x/3 + y/4 = 1, then the slope of l is ______, the x-intercept is ______, and the y-intercept is ______.
(16) If the equation of line l is y + 2 = √3 (x + 1), then the slope of l is ______, and the inclination angle is ______.

9.3 Parallel and Perpendicular Lines in a Plane

I. Parallel Lines

Let l1 and l2 be two lines (i.e., l1 and l2 do not coincide). If l1 and l2 have no intersection point, they are said to be parallel.

Note: Two lines l1 and l2 are parallel ⇔ Their inclination angles are equal.

If the slope of line l1 does not exist (i.e., inclination angle is 90°), then the slope of line l2 does not exist ⇔ l1 is parallel to l2 (denoted as l1 // l2) (See Figure 9.3-1).

Example 1: If line l2 passes through point P0(2, -1) and is parallel to line l1: x = -1, then the equation of line l2 is ______.
Solution: The answer is x = 2.

If line l1: y = k1x + b1, line l2: y = k2x + b2, then l1 // l2 ⇔ k1 = k2 and b1 ≠ b2. (See Figure 9.3-2)

Example 2: Given line l2 passes through point P0(-3, -1) and is parallel to line l1: 4x - 2y + 3 = 0, find the equation of line l2.
Solution: Let k_l1 and k_l2 represent the slopes of lines l1 and l2 respectively.
Since 4x - 2y + 3 = 0 ⇒ 2y = 4x + 3 ⇒ y = 2x + 3/2,
Therefore, k_l1 = 2.
Since l1 // l2,
Therefore, k_l2 = k_l1 = 2.
Since l2 passes through point P0(-3, -1),
Using the point-slope form, the equation of line l2 is:
y - (-1) = 2[x - (-3)] ⇒ y + 1 = 2(x + 3) ⇒ y = 2x + 5.

If line l1: A1x + B1y + C1 = 0, line l2: A2x + B2y + C2 = 0, then l1 // l2 ⇔ A1/A2 = B1/B2 ≠ C1/C2 (where A2 ≠ 0, B2 ≠ 0, C2 ≠ 0).

Example 3: Let line l1: 2x + 3y + 1 = 0, line l2: 4x + 6y - 3 = 0. Are l1 and l2 parallel?
Solution: Since 2/4 = 3/6 ≠ 1/(-3), Therefore, l1 // l2.

Example 4: Let line l1: y - 1 = 0, line l2: 2y + 5 = 0. Are l1 and l2 parallel?
Because A2 = 0, we cannot use A1/A2 = B1/B2 ≠ C1/C2 to determine if the lines are parallel (cannot divide by 0). In this case, we can determine by drawing the lines. (See Figure 9.3-3)
Solution:
l1: y - 1 = 0 ⇒ y = 1,
l2: 2y + 5 = 0 ⇒ y = -5/2.
From Figure 9.3-3, l1 // l2.

Example 5: Let line l1: x - 2 = 0, line l2: x + 1 = 0. Are l1 and l2 parallel?
Because B2 = 0, we cannot use A1/A2 = B1/B2 ≠ C1/C2 to determine if the lines are parallel. In this case, we can determine by drawing the lines. (See Figure 9.3-4)
Solution:
l1: x - 2 = 0 ⇒ x = 2,
l2: x + 1 = 0 ⇒ x = -1.
From Figure 9.3-4, l1 // l2.

Example 6: Let line l1: 2x - y = 0, line l2: 3x + 5y = 0. Are l1 and l2 parallel?
Solution: Since 2/3 ≠ -1/5, Therefore, l1 and l2 are not parallel.

II. Perpendicular Lines

If the slope of line l1 does not exist, then the slope of l2 is 0 ⇔ l1 is perpendicular to l2 (denoted as l1 ⟂ l2) (See Figure 9.3-5); If the slope of line l1 is 0, then the slope of l2 does not exist ⇔ l1 is perpendicular to l2.

Example 7: Given line l2 passes through point P0(1, -1) and is perpendicular to line l1: x = 2, find the equation of line l2.
Solution: The slope of line l1 does not exist, and l1 ⟂ l2,
Therefore, k_l2 = 0.
So, using the point-slope form, the equation of line l2 is:
y - (-1) = 0(x - 1) ⇒ y = -1.

If line l1: y = k1x + b1, line l2: y = k2x + b2, then l1 ⟂ l2 ⇔ k1 * k2 = -1, as shown in Figure 9.3-6.

Example 8: Given line l2 passes through point P0(-5, 3) and is perpendicular to line l1: 4x - 2y + 3 = 0, find the equation of line l2.
Solution: Since 4x - 2y + 3 = 0 ⇒ 2y = 4x + 3 ⇒ y = 2x + 3/2,
Therefore, k_l1 = 2.
Since l1 ⟂ l2,
Therefore, k_l1 * k_l2 = -1 ⇒ 2 * k_l2 = -1 ⇒ k_l2 = -1/2.
Since l2 passes through point P0(-5, 3),
Using the point-slope form, the equation of line l2 is:
y - 3 = (-1/2)[x - (-5)] ⇒ y - 3 = (-1/2)(x + 5) ⇒ y = (-1/2)x + 1/2.

If line l1: A1x + B1y + C1 = 0, line l2: A2x + B2y + C2 = 0, then l1 ⟂ l2 ⇔ A1A2 + B1B2 = 0.

Example 9: Given line l1: 2x - 4y + 1 = 0, line l2: 2x + y + 1 = 0, are l1 and l2 perpendicular?
Solution: Let A1 = 2, B1 = -4, A2 = 2, B2 = 1.
Since A1A2 + B1B2 = 2*2 + (-4)*1 = 0,
Therefore, l1 ⟂ l2.

Example 10: Given line l: x + 3y - 2 = 0, which of the following lines is perpendicular to l? ( )
A. 2x + 6y + 1 = 0
B. x + 3y = 0
C. 3y - 2 = 0
D. 3x - y - 1 = 0
Solution:
A: Because A1A2 + B1B2 = 1*2 + 3*6 = 20 ≠ 0, so A is incorrect.
B: Because A1A2 + B1B2 = 1*1 + 3*3 = 10 ≠ 0, so B is incorrect.
C: Because A1A2 + B1B2 = 1*0 + 3*3 = 9 ≠ 0, so C is incorrect.
D: Because A1A2 + B1B2 = 1*3 + 3*(-1) = 0, so D is correct.
The answer is D.

III. Distance from a Point to a Line

Let P0(x0, y0) be a point not on line l: Ax + By + C = 0. Then the distance from P0 to l is:
d(P0, l) = |Ax0 + By0 + C| / sqrt(A^2 + B^2).

Example 11: The distance from point P0(-2, 5) to line l: 4x + 3y - 1 = 0 is ______.
Solution: d(P0, l) = |4*(-2) + 3*5 - 1| / sqrt(4^2 + 3^2) = 6/5.
The answer is 6/5.

IV. Distance Between Two Parallel Lines

Let two lines be l1: Ax + By + C1 = 0, l2: Ax + By + C2 = 0. Then the distance between l1 and l2 is:
d(l1, l2) = |C1 - C2| / sqrt(A^2 + B^2).

Example 12: The distance between lines l1: 2x + y - 2 = 0 and l2: 2x + y + 3 = 0 is ______.
Solution: d(l1, l2) = |-2 - 3| / sqrt(2^2 + 1^2) = sqrt(5).

Example 13: The distance between lines l1: x - 3y - 1 = 0 and l2: 2x - 6y + 5 = 0 is ______.
Solution: l2: 2x - 6y + 5 = 0 ⇒ x - 3y + 5/2 = 0.
d(l1, l2) = |-1 - 5/2| / sqrt(1^2 + (-3)^2) = (7/2) / sqrt(10) = 7 / (2*sqrt(10)) = 7*sqrt(10)/20.

Summary

Memorize the conditions for two lines to be parallel or perpendicular.
(1) Parallel Lines:
* If the slope of line l1 does not exist, then the slope of line l2 does not exist ⇔ l1 // l2.
* If line l1: y = k1x + b1, line l2: y = k2x + b2, then l1 // l2 ⇔ k1 = k2 and b1 ≠ b2.
* If line l1: A1x + B1y + C1 = 0, line l2: A2x + B2y + C2 = 0, then l1 // l2 ⇔ A1/A2 = B1/B2 ≠ C1/C2 (where A2 ≠ 0, B2 ≠ 0, C2 ≠ 0).
(2) Perpendicular Lines:
* If the slope of line l1 does not exist, then the slope of l2 is 0 ⇔ l1 ⟂ l2; If the slope of line l1 is 0, then the slope of l2 does not exist ⇔ l1 ⟂ l2.
* If l1: y = k1x + b1, l2: y = k2x + b2, then l1 ⟂ l2 ⇔ k1 * k2 = -1.
* If line l1: A1x + B1y + C1 = 0, line l2: A2x + B2y + C2 = 0, then l1 ⟂ l2 ⇔ A1A2 + B1B2 = 0.

Be able to write the equation of a line parallel or perpendicular to a given line.

Be able to determine the positional relationship (parallel or perpendicular) between two lines.

Memorize the distance formula from point P0(x0, y0) to line l: Ax + By + C = 0: d(P0, l) = |Ax0 + By0 + C| / sqrt(A^2 + B^2).

Memorize the distance formula from line l1: Ax + By + C1 = 0 to l2: Ax + By + C2 = 0: d(l1, l2) = |C1 - C2| / sqrt(A^2 + B^2).

Exercise 9.3

Multiple Choice (For each question, only one of the four options is correct).
(1) Which line is parallel to 3x - 4y + 1 = 0? ( )
A. 3x + 4y + 3 = 0
B. x + 2 = 0
C. y + 3 = 0
D. 6x - 8y - 1 = 0
(2) Which line is parallel to x + 1 = 0? ( )
A. 2x + y + 3 = 0
B. x + 2 = 0
C. y - 2x + 3 = 0
D. 2y - x = 0
(3) Which line is perpendicular to x - 2y + 1 = 0? ( )
A. 2x + y = 0
B. x + 2y = 0
C. 2x - y = 0
D. x - 2y + 3 = 0
(4) Which line is perpendicular to 2x - 5y + 2 = 0? ( )
A. 2x + 3 = 0
B. y + 2 = 0
C. 4x - 10y + 3 = 0
D. 5x + 2y = 0
(5) The positional relationship between lines x + 4y - 2 = 0 and 2x + 8y + 5 = 0 is ( ).
A. Parallel
B. Intersecting but not perpendicular
C. Coincident
D. Perpendicular
(6) Given lines l1: ax + 3y + 1 = 0, l2: x + (a-2)y + a = 0. If l1 // l2, then a = ( ).
A. 2
B. 3
C. -1
D. -2

Fill in the blanks.
(1) If line l2 passes through point P0(1, -3) and is parallel to line l1: x = 4, then the equation of l2 is ______.
(2) If line l2 passes through point P0(1, -3) and is parallel to line l1: y = 4, then the equation of l2 is ______.
(3) If line l2 passes through point P0(2, 1) and is parallel to line l1: 2x - 3y + 1 = 0, then the equation of l2 is ______.
(4) If line l2 passes through point P0(2, 4) and is perpendicular to line l1: x = 1, then the equation of l2 is ______.
(5) If line l2 passes through point P0(2, 4) and is perpendicular to line l1: y = 1, then the equation of l2 is ______.
(6) If line l2 passes through point P0(-1, 2) and is perpendicular to line l1: 4x + 3y + 1 = 0, then the equation of l2 is ______.
(7) If line l1: y = (2a - 1)x + 3 is perpendicular to line l2: y = 4x - 3, then a = ______.
(8) Given the slope of line l1 is 3, and line l2 passes through points A(1, 2) and B(2, a). If l1 // l2, then a = ______; If l1 ⟂ l2, then a = ______.
(9) The distance from point P0(-1, 2) to line l: x + 2y - 1 = 0 is ______.
(10) The distance from point P0(3, -2) to line l: 2x - 2y + 1 = 0 is ______.
(11) The distance between lines l1: 4x - 2y - 1 = 0 and l2: 4x - 2y + 1 = 0 is ______.
(12) The distance between lines l1: 3x + y - 3 = 0 and l2: 3x + y + 1 = 0 is ______.
(13) The distance between lines l1: 4x - 8y + 1 = 0 and l2: x - 2y - 1 = 0 is ______.

Calculation Problems (Show your work and provide the result).
(1) Given line l1: 2x + 3y + 5 = 0, and line l2 passes through point P(1, -4) and is parallel to l1. Find the equation of line l2.
(2) Given line l1: 5x - 3y + 1 = 0, and line l2 passes through point Q(-2, 3) and is perpendicular to l1. Find the equation of line l2.

9.4 Intersection of Lines in a Plane

Given lines l1: A1x + B1y + C1 = 0, l2: A2x + B2y + C2 = 0, find the intersection of l1 and l2. Combine the equations of l1 and l2:
{ A1x + B1y + C1 = 0,
A2x + B2y + C2 = 0 } → Intersection point (x0, y0). (See Figure 9.4-1)

Example 1: Given lines l1: 2x + y - 1 = 0, l2: 4x + y + 3 = 0, find their intersection.
Solution:
{ 2x + y - 1 = 0 ...①
4x + y + 3 = 0 ...② }
② - ① → 2x + 4 = 0 → x = -2.
Substitute x = -2 into ①:
2*(-2) + y - 1 = 0 ⇒ -4 + y - 1 = 0 ⇒ y = 5.
Therefore, the intersection point of l1 and l2 is (-2, 5).

Example 2: Given lines l1: 3x + 2y - 1 = 0, l2: 4x + 3y - 3 = 0, find their intersection.
Solution:
{ 3x + 2y - 1 = 0 ...①
4x + 3y - 3 = 0 ...② }
①×4 ⇒ 12x + 8y - 4 = 0 ...③
②×3 ⇒ 12x + 9y - 9 = 0 ...④
④ - ③ ⇒ y - 5 = 0 ⇒ y = 5.
Substitute y = 5 into ①:
3x + 2*5 - 1 = 0 ⇒ 3x + 9 = 0 ⇒ x = -3.
Therefore, the intersection point of l1 and l2 is (-3, 5).

Example 3: Given line l is parallel to line l1: 3x + 6y - 1 = 0 and passes through the intersection of lines l2: 2x + y - 3 = 0 and l3: x + y - 1 = 0. Find the equation of line l.
Solution:
3x + 6y - 1 = 0 ⇒ 6y = -3x + 1 ⇒ y = (-1/2)x + 1/6.
Therefore, k_l1 = -1/2.
Since l // l1,
Therefore, k_l = k_l1 = -1/2.
Solve for the intersection of l2 and l3:
{ 2x + y - 3 = 0,
x + y - 1 = 0 } ⇒ x = 2, y = -1.
Therefore, the intersection point of l2 and l3 is (2, -1).
Using the point-slope form, the equation of line l is:
y - (-1) = (-1/2)(x - 2) ⇒ y = (-1/2)x.

Example 4: Given line l is perpendicular to line l1: x - 2y - 1 = 0 and passes through the intersection of lines l2: 2x + y - 3 = 0 and l3: x + y - 1 = 0. Find the equation of line l.
Solution:
x - 2y - 1 = 0 ⇒ 2y = x - 1 ⇒ y = (1/2)x - 1/2.
Therefore, k_l1 = 1/2.
Since l ⟂ l1,
Therefore, k_l * k_l1 = -1 ⇒ k_l = -2.
Solve for the intersection of l2 and l3:
{ 2x + y - 3 = 0,
x + y - 1 = 0 } ⇒ x = 2, y = -1.
Therefore, the intersection point of l2 and l3 is (2, -1).
Using the point-slope form, the equation of line l is:
y - (-1) = -2(x - 2) ⇒ y = -2x + 3.

Summary

Intersection of two lines:
Line l1: A1x + B1y + C1 = 0, l2: A2x + B2y + C2 = 0.
{ A1x + B1y + C1 = 0,
A2x + B2y + C2 = 0 } ⇒ Intersection point (x0, y0).

Exercise 9.4

Multiple Choice (For each question, only one of the four options is correct).
(1) The intersection point of lines l1: 3x - 2y - 3 = 0 and l2: x + y - 1 = 0 is ( ).
A. (1, 0) B. (1, 2) C. (-1, 0) D. (-1, 2)
(2) The equation of the line passing through the intersection of lines x - 3y + 4 = 0 and 2x + y + 5 = 0, and also passing through the origin, is ( ).
A. 19x - 9y = 0 B. 3x + 19y = 0 C. 19x - 3y = 0 D. 9x + 19y = 0
(3) Given sets A = {(x, y) | x + y = 2}, B = {(x, y) | x - y = 4}, then A ∩ B = ( ).
A. {3, -1} B. 3, -1 C. (3, -1) D. {(3, -1)}
(4) If the three lines 2x + 3y + 8 = 0, x - y - 1 = 0, x + ky + k + 1/2 = 0 intersect at a single point, then the value of k is ( ).
A. -2 B. -1/2 C. 2 D. 1/2

Fill in the blanks.
(1) The intersection point of lines x + y - 3 = 0 and 2x - y = 0 is ______.
(2) The intersection point of lines 4x + 3y - 5 = 0 and x + y + 1 = 0 is ______.
(3) The intersection point of lines x + 3y - 2 = 0 and x + 2y + 3 = 0 is ______.
(4) The intersection point of lines 2x - 3y - 1 = 0 and 3x + 4y + 2 = 0 is ______.
(5) If the three lines l1: ax + 2y + 6 = 0, l2: x + y - 4 = 0, l3: 2x - y + 1 = 0 intersect at the same point, then the real number a = ______.

Calculation Problems (Show your work and provide the result).
(1) Given line l is parallel to line l1: x + 3y - 2 = 0 and passes through the intersection of lines l2: 2x + 3y - 2 = 0 and l3: x + 2y - 1 = 0. Find the equation of line l.
(2) Given line l is parallel to line l1: 2x - y + 1 = 0 and passes through the intersection of lines l2: 2x + y = 0 and l3: x - 2y + 3 = 0. Find the equation of line l.
(3) Given line l is perpendicular to line l1: x + 2y - 1 = 0 and passes through the intersection of lines l2: 2x - y - 1 = 0 and l3: x + y - 1 = 0. Find the equation of line l.
(4) Given line l is perpendicular to line l1: 4x + 2y - 5 = 0 and passes through the intersection of lines l2: 2x + 4y - 3 = 0 and l3: 3x + y - 1 = 0. Find the equation of line l.

Chapter 9 Self-Assessment Test

Multiple Choice (For each question, only one of the four options is correct).
(1) If a line passes through points A(2, 4) and B(1, m) and has an inclination angle of 45°, then m = ( ).
A. 3 B. -3 C. 5 D. -1
(2) If lines x - ky - k = 0 and kx - y - k = 0 are parallel, then k = ( ).
A. 1 B. -1 C. 1 or -1 D. 2
(3) If line ax + (1-a)y = 3 is perpendicular to line 2x - 3y + 1 = 0, then a = ( ).
A. 3 B. 1 C. 3/5 D. 2/3
(4) If the inclination angle of a line is α = 120°, then the slope of this line is ( ).
A. √3 B. -√3 C. √3/2 D. ±√3
(5) Given line l: ax + y - 2 - a = 0 has equal x-intercept and y-intercept. The value of a is ( ).
A. 1 B. -1 C. -2 or -1 D. -2 or 1
(6) The equation of the line passing through points A(4, 0) and B(0, -3) is ( ).
A. 3x - 4y - 12 = 0 B. 3x + 4y - 12 = 0 C. 4x - 3y + 12 = 0 D. 4x + 3y + 12 = 0
(7) The equation of the line with an inclination angle of 135° and a y-intercept of -1 is ( ).
A. x - y + 1 = 0 B. x - y - 1 = 0 C. x + y - 1 = 0 D. x + y + 1 = 0
(8) The positional relationship between lines x + 3y - 5 = 0 and 3x - y - 1 = 0 is ( ).
A. Coincident B. Parallel C. Perpendicular D. Intersecting but not perpendicular
(9) The positional relationship between lines 2x - y + 1 = 0 and 4x - 2y + 3 = 0 is ( ).
A. Coincident B. Parallel


Chapter 10: Conic Sections

10.1 Circles

I. Standard Equation of a Circle

Definition 1: The path of a moving point in a plane that is at a fixed distance from a fixed point is called a circle. The fixed point is the center, and the fixed distance is the radius.

We call the equation (x-a)^2 + (y-b)^2 = r^2 the standard equation of a circle. (a, b) is the center and r is the radius.

Example 1: The circle (x-1)^2 + (y-2)^2 = 25 has center ______ and radius r = ______.
Solution: Center is (1, 2), r = 5.

Example 2: The circle (x+3)^2 + (y-5)^2 = 16 has center ______ and radius r = ______.
Solution: Center is (-3, 5), r = 4.

Example 3: The circle (x+2)^2 + (y+3)^2 = 5 has center ______ and radius r = ______.
Solution: Center is (-2, -3), r = sqrt(5).

II. General Equation of a Circle

Definition 2: The equation x^2 + y^2 + Dx + Ey + F = 0 (with D^2+E^2-4F > 0) is the general equation of a circle.

We can rewrite it as (x + D/2)^2 + (y + E/2)^2 = (D^2 + E^2 - 4F)/4.
So, the center is (-D/2, -E/2) and the radius is r = sqrt(D^2+E^2-4F) / 2.

Example 4: Given the circle x^2 + y^2 + 4x - 6y + 1 = 0, find its center and radius.
Solution: D=4, E=-6, F=1.
Center = (-D/2, -E/2) = (-2, 3).
r = sqrt(4^2 + (-6)^2 - 4) / 2 = sqrt(48)/2 = 2*sqrt(3).

Example 5: Write the general equation x^2 + y^2 - 2x + 4y - 2 = 0 in standard form.
Solution: D=-2, E=4, F=-2.
Center = (-D/2, -E/2) = (1, -2).
r = sqrt((-2)^2 + 4^2 + 8) / 2 = sqrt(28)/2 = sqrt(7).
Standard equation: (x-1)^2 + (y+2)^2 = 7.

Example 6: The line 2x+y-1=0 intersects the circle x^2 + y^2 - 4x - 2y = 0 at points A and B. Find distance |AB|.
Solution: Find intersection points.
From line: y = -2x+1.
Substitute into circle: x^2 + (-2x+1)^2 -4x -2(-2x+1)=0.
Simplify: 5x^2 -4x -1=0. Solutions: x1 = 1, x2 = -1/5.
Corresponding y: y1 = -1, y2 = 7/5.
Points: A(1, -1), B(-1/5, 7/5).
|AB| = sqrt((1+1/5)^2 + (-1-7/5)^2) = sqrt((6/5)^2 + (-12/5)^2) = sqrt(36/25 + 144/25) = sqrt(180/25) = (6*sqrt(5))/5.

Summary

Standard form: (x-a)^2+(y-b)^2 = r^2. Center (a,b), radius r.

General form: x^2+y^2+Dx+Ey+F=0 (D^2+E^2-4F>0). Center (-D/2, -E/2), radius r = sqrt(D^2+E^2-4F)/2.

Exercise 10.1

Multiple Choice.
(1) Equation of circle with center A(2,-3), radius 5: ( ).
A. (x+2)^2+(y-3)^2=25
B. (x-2)^2+(y+3)^2=25
C. (x-2)^2+(y+3)^2=5
D. (x+2)^2+(y-3)^2=5
(2) Center and radius of (x-2)^2 + y^2 = 2: ( ).
A. (2,0), 2
B. (-2,0), 2
C. (0,2), sqrt(2)
D. (2,0), sqrt(2)
(3) Center and radius of x^2+y^2+4x=0: ( ).
A. (-2,0), 2
B. (0,-2), 4
C. (2,0), 2
D. (0,2), 4
(4) If circle equation is (x-1)(x+2)+(y-2)(y+4)=0, center is ( ).
A. (1,-1)
B. (1/2, 1)
C. (-1,2)
D. (-1/2, -1)
(5) If x^2+y^2-2x+y+m=0 is a circle, m range is ( ).
A. (-∞, 5)
B. (-∞, 5/4)
C. (-∞, 3/2)
D. (3/2, +∞)
(6) For circle x^2+y^2-2x+6y+8=0, which line passes through its center? ( ).
A. 2x-y-1=0
B. 2x+y+1=0
C. 2x-y+1=0
D. 2x+y-1=0

Fill in blanks.
(1) Circle (x+2)^2+(y-1)^2=9: center ______, radius ______.
(2) Circle (x+3)^2+(y+5)^2=16: center ____, radius ____.
(3) Circle (x-4)^2+(y-6)^2=4: center , radius ____.
(4) If x^2+y^2+Dx+Ey+F=0 has center (2,-4), radius 4, then D=, E=, F=.
(5) Circle 2x^2+2y^2+6x-4y-3=0: center ____, radius ____, standard form ________.

Solve.
(1) Line x-y+2=0 intersects circle x^2+y^2+x-y-1=0 at P, Q. Find |PQ|.
(2) Circle C passes through A(3,1) and B(5,3). Its center lies on line y=x. Find its equation.
(3) Circle C passes through (1,0), center on positive x-axis. Line l: y=x-1 intercepts a chord of length 2*sqrt(2). Find equation of line through center perpendicular to l.

10.2 Ellipse

Definition: The locus of a point where the sum of its distances from two fixed points (foci) is a constant is an ellipse.

Let F1, F2 be foci, |F1F2| = 2c (c>0). Let a be a constant with a > c. The set of points P with |PF1| + |PF2| = 2a is an ellipse.

Place foci on x-axis, origin at midpoint. Then standard equation is: x^2/a^2 + y^2/b^2 = 1 (where b^2 = a^2 - c^2, a > b > 0). Foci are on x-axis.

Place foci on y-axis, origin at midpoint. Then standard equation is: x^2/b^2 + y^2/a^2 = 1 (where b^2 = a^2 - c^2, a > b > 0). Foci are on y-axis.

I. Ellipse x^2/a^2 + y^2/b^2 = 1 (a>b>0)

Vertices: A1(-a,0), A2(a,0), B1(0,-b), B2(0,b).
Foci: F1(-c,0), F2(c,0). Focal length = 2c.
Major axis: A1A2, length 2a.
Minor axis: B1B2, length 2b.
Eccentricity: e = c/a.

Note: a^2 = b^2 + c^2. Foci are on x-axis.

Example 1: F1, F2 are foci of ellipse x^2/9 + y^2/4 = 1. P on ellipse. If |PF1|=2, find |PF2|.
Solution: a^2=9, so a=3. |PF1|+|PF2| = 2a = 6. So |PF2| = 6-2 = 4.

Example 2: Find foci and eccentricity of ellipse x^2/25 + y^2/16 = 1.
Solution: a^2=25, b^2=16. c^2 = a^2-b^2 = 9, c=3.
Foci: (-3,0) and (3,0).
Eccentricity e = c/a = 3/5.

II. Ellipse x^2/b^2 + y^2/a^2 = 1 (a>b>0)

Vertices: B1(-b,0), B2(b,0), A1(0,-a), A2(0,a).
Foci: F1(0,-c), F2(0,c). Focal length = 2c.
Major axis: A1A2, length 2a.
Minor axis: B1B2, length 2b.
Eccentricity: e = c/a.

Note: a^2 = b^2 + c^2. Foci are on y-axis.

Example 3: Find foci and eccentricity of ellipse x^2/4 + y^2/5 = 1.
Solution: a^2=5, b^2=4. c^2 = a^2-b^2 = 1, c=1.
Foci: (0,-1) and (0,1).
Eccentricity e = c/a = 1/sqrt(5) = sqrt(5)/5.

Example 4: Ellipse 3x^2+4y^2=12 has foci ( ).
A. (0, ±2) B. (±2, 0) C. (0, ±sqrt(3)) D. (±1, 0)
Solution: Divide by 12: x^2/4 + y^2/3 = 1. a^2=4, b^2=3, c=sqrt(4-3)=1. Foci on x-axis: (±1,0). Answer D.

Summary

Ellipse x^2/a^2 + y^2/b^2 = 1 (a>b>0):

Foci on x-axis: (±c,0), c=sqrt(a^2-b^2).

e = c/a.

Ellipse x^2/b^2 + y^2/a^2 = 1 (a>b>0):

Foci on y-axis: (0,±c), c=sqrt(a^2-b^2).

e = c/a.

Exercise 10.2

Drawing.
(1) Draw ellipse x^2/100 + y^2/64 = 1, label vertices and foci.
(2) Draw ellipse x^2/16 + y^2/25 = 1, label vertices and foci.

Multiple Choice.
(1) Ellipse: major axis length 8, e=3/4. Its standard eq is ( ).
A. x^2/16 + y^2/7 = 1
B. x^2/16 + y^2/7 = 1 OR x^2/7 + y^2/16 = 1
C. x^2/16 + y^2/25 = 1
D. x^2/16 + y^2/25 = 1 OR x^2/25 + y^2/16 = 1
(2) On ellipse x^2/25 + y^2 = 1, P to one focus distance is 2. Distance to other focus is ( ).
A. 5 B. 6 C. 7 D. 8
(3) Distance between foci of ellipse 2x^2+3y^2=12 is ( ).
A. 2sqrt(10) B. sqrt(10) C. sqrt(2) D. 2sqrt(2)
(4) Center at origin, right focus F(1,0), e=1/2. Standard eq is ( ).
A. x^2/3 + y^2/4 = 1 B. x^2/4 + y^2/sqrt(3)=1 C. x^2/4 + y^2/3 = 1 D. x^2/4 + y^2=1
(5) Eccentricity of x^2 + 4y^2 = 1 is ( ).
A. sqrt(3)/2 B. 3/4 C. sqrt(2)/2 D. 2/3
(6) The graph of x^2/4 + y^2/16 = 1 is ( ).
(Descriptions of graphs omitted)

Fill in blanks.
(1) For ellipse x^2/25 + y^2/16 = 1, |PF1|+|PF2| = ____.
(2) Ellipse x^2/8 + y^2/4 = 1: foci ____, e = ____.
(3) Ellipse x^2/40 + y^2/121 = 1: foci ____, e = ____.
(4) Ellipse x^2/5 + y^2/2 = 1: foci ____, e = ____.
(5) Ellipse 8x^2+3y^2=24: foci ____, e = ____.
(6) Symmetry axes are coordinate axes. One focus (0,7), one vertex (9,0). Standard eq is ____.
(7) Ellipse x^2/m + y^2/4 = 1 (m>0), focal distance 2, then m = ____.
(8) Ellipse x^2/25 + y^2/m^2 = 1 (m>0) has left focus (-4,0), then m = ____.

Solve.
(1) For ellipse x^2/100 + y^2/64 = 1, find vertices, foci, major/minor axis lengths, e.
(2) Ellipse C: x^2/a^2 + y^2/b^2=1 (a>b>0) has left focus (-2,0), e=sqrt(6)/3. Find standard eq.
(3) Line y=x+1 intersects ellipse C: mx^2 + y^2 = 2 (m>1) at A,B. If OA ⟂ OB, find m.

10.3 Hyperbola

Definition: The locus of a point where the absolute difference of its distances from two fixed points (foci) is a constant is a hyperbola.

Let F1, F2 be foci, |F1F2| = 2c (c>0). Let a be a constant with a < c. The set of points P with ||PF1| - |PF2|| = 2a is a hyperbola.

Place foci on x-axis, origin at midpoint. Standard eq: x^2/a^2 - y^2/b^2 = 1 (b^2 = c^2 - a^2, b>0).

Place foci on y-axis, origin at midpoint. Standard eq: y^2/a^2 - x^2/b^2 = 1 (b^2 = c^2 - a^2, b>0).

I. Hyperbola x^2/a^2 - y^2/b^2 = 1 (a>0,b>0)

Vertices: A1(-a,0), A2(a,0).
Foci: F1(-c,0), F2(c,0). Focal length = 2c.
Transverse axis (real): A1A2, length 2a.
Conjugate axis (imaginary): B1B2 (B1(0,-b), B2(0,b)), length 2b.
Asymptotes: y = ± (b/a)x.
Eccentricity: e = c/a.

Note: c^2 = a^2 + b^2. Foci on x-axis. Asymptotes from x^2/a^2 - y^2/b^2 = 0.

Example 1: Foci at (-5,0) and (5,0). | |PF1| - |PF2| | = 4. Find standard eq.
Solution: c=5, 2a=4 so a=2. b^2 = c^2 - a^2 = 25-4=21. Eq: x^2/4 - y^2/21 = 1.

Example 2: For x^2/9 - y^2/16 = 1, find asymptotes, foci, e.
Solution: a=3, b=4. Asymptotes: y = ±(4/3)x.
c = sqrt(a^2+b^2) = sqrt(9+16)=5. Foci: (-5,0),(5,0). e = c/a = 5/3.

II. Hyperbola y^2/a^2 - x^2/b^2 = 1 (a>0,b>0)

Vertices: A1(0,-a), A2(0,a).
Foci: F1(0,-c), F2(0,c). Focal length = 2c.
Transverse axis: A1A2, length 2a.
Conjugate axis: B1B2 (B1(-b,0), B2(b,0)), length 2b.
Asymptotes: y = ± (a/b)x.
Eccentricity: e = c/a.

Note: c^2 = a^2 + b^2. Foci on y-axis. Asymptotes from y^2/a^2 - x^2/b^2 = 0.

Example 3: For y^2/11 - x^2/5 = 1, find asymptotes, foci, e.
Solution: a=sqrt(11), b=sqrt(5). Asymptotes: y = ±(sqrt(11)/sqrt(5))x = ±(sqrt(55)/5)x.
c = sqrt(11+5)=4. Foci: (0,-4),(0,4). e = c/a = 4/sqrt(11) = (4*sqrt(11))/11.

Example 4: Hyperbola 5y^2 - 3x^2 = 15 has foci ( ).
A. (0, ±sqrt(2)) B. (±sqrt(2),0) C. (0, ±2sqrt(2)) D. (±2sqrt(2),0)
Solution: Divide by 15: y^2/3 - x^2/5 = 1. a^2=3, b^2=5, c=sqrt(3+5)=sqrt(8)=2*sqrt(2). Foci on y-axis: (0, ±2*sqrt(2)). Answer C.

Summary

Hyperbola x^2/a^2 - y^2/b^2 = 1:

Foci on x-axis: (±c,0), c=sqrt(a^2+b^2).

Asymptotes: y = ±(b/a)x.

e = c/a.

Hyperbola y^2/a^2 - x^2/b^2 = 1:

Foci on y-axis: (0,±c), c=sqrt(a^2+b^2).

Asymptotes: y = ±(a/b)x.

e = c/a.

Exercise 10.3

Drawing.
(1) Draw x^2/36 - y^2/64 = 1, label asymptotes, vertices, foci.
(2) Draw y^2/2 - x^2 = 1, label asymptotes, vertices, foci.

Multiple Choice.
(1) Which eq is hyperbola with foci on x-axis? ( )
A. x^2 - y^2/4 = 1
B. y^2/4 - x^2 = 1
C. y^2/4 + x^2 = 1
D. y^2 + x^2/4 = 1
(2) Which eq is hyperbola with foci on y-axis? ( )
A. x^2/25 - y^2/9 = 1
B. x^2/25 - y^2/9 = -1
C. y^2/16 - x^2/9 = 1
D. x^2/16 + y^2/9 = 1
(3) Which hyperbola has asymptotes y=±2x? ( )
A. x^2 - y^2/4 = 1
B. x^2/4 - y^2 = 1
C. x^2 - y^2/2 = 1
D. x^2/2 - y^2 = 1
(4) Foci of x^2/10 - y^2/6 = 1: ( ).
A. (±2,0) B. (0,±2) C. (±4,0) D. (0,±4)
(5) Foci of y^2/2 - x^2/5 = 1: ( ).
A. (±sqrt(3),0) B. (0,±sqrt(3)) C. (±sqrt(7),0) D. (0,±sqrt(7))
(6) Graph of y^2/9 - x^2/16 = 1 is ( ).

Fill in blanks.
(1) Hyperbola x^2/4 - y^2/6 = 1: asymptotes ____, foci ____, e = ____.
(2) Hyperbola y^2/8 - x^2/8 = 1: asymptotes ____, foci ____, e = ____.
(3) Hyperbola y^2/3 - x^2/9 = 1: asymptotes ____, foci ____, e = ____.
(4) Hyperbola 4x^2 - 9y^2 = 36: asymptotes ____, foci ____, e = ____.
(5) Hyperbola 4y^2 - 7x^2 = 28: asymptotes ____, foci ____, e = ____.

Solve.
(1) For 5x^2 - 3y^2 = 15, find transverse/conjugate axis lengths, asymptotes, foci, e.
(2) Asymptotes y=±(1/2)x. Foci are vertices on major axis of ellipse x^2/16 + y^2=1. Find hyperbola eq.
(3) Hyperbola x^2/a^2 - y^2/b^2=1 has e = 2*sqrt(3)/3. Line through A(a,0) and B(0,-b) has distance from origin = sqrt(3)/2. Find its eq.

10.4 Parabola

Definition: The locus of a point equidistant from a fixed point (focus) and a fixed line (directrix) is a parabola.

Let F be focus, l be directrix (F not on l). |PF| = distance from P to l.

Let distance from F to l be p (p>0). Vertex at midpoint.

Possible standard equations (vertex at origin):

Opening up: x^2 = 2py (p>0). Focus (0, p/2), directrix y = -p/2.

Opening down: x^2 = -2py (p>0). Focus (0, -p/2), directrix y = p/2.

Opening right: y^2 = 2px (p>0). Focus (p/2, 0), directrix x = -p/2.

Opening left: y^2 = -2px (p>0). Focus (-p/2, 0), directrix x = p/2.

Eccentricity e = 1 for all parabolas.

Example 1: For x^2 = 4y, find focus and directrix.
Solution: 2p=4, p=2. Focus (0,1), directrix y = -1.

Example 2: For x^2 = -8y, find focus and directrix.
Solution: 2p=8, p=4. Focus (0,-2), directrix y = 2.

Example 3: For y^2 = 12x, find focus and directrix.
Solution: 2p=12, p=6. Focus (3,0), directrix x = -3.

Example 4: For y^2 = -16x, find focus and directrix.
Solution: 2p=16, p=8. Focus (-4,0), directrix x = 4.

Summary

x^2 = 2py (p>0): Focus (0, p/2), directrix y = -p/2.

x^2 = -2py (p>0): Focus (0, -p/2), directrix y = p/2.

y^2 = 2px (p>0): Focus (p/2, 0), directrix x = -p/2.

y^2 = -2px (p>0): Focus (-p/2, 0), directrix x = p/2.

e = 1.

Exercise 10.4

Drawing.
(1) Draw x^2 = 16y, label focus and directrix.
(2) Draw x^2 = -4y, label focus and directrix.
(3) Draw y^2 = 20x, label focus and directrix.
(4) Draw y^2 = -4x, label focus and directrix.

Multiple Choice.
(1) Directrix of y = 4x^2 is ( ).
A. x=-1 B. y=-1 C. x=-1/16 D. y=-1/16
(2) Focus of y^2 = 4x is ( ).
A. (0,2) B. (0,1) C. (2,0) D. (1,0)
(3) Parabola with focus at the right vertex of hyperbola x^2/16 - y^2/9=1 has eq ( ).
A. y^2=16x B. y^2=-16x C. y^2=8x D. y^2=-8x
(4) Distance from focus to directrix of y^2=8x is ( ).
A. 1 B. 2 C. 4 D. 8
(5) Vertex at origin, directrix x=2. Parabola eq is ( ).
A. y^2=4x B. x^2=-8y C. y^2=8x D. y^2=-8x
(6) Focus of x^2 = (1/2)y is ( ).
A. (1/2, 0) B. (0, 1/2) C. (1/8, 0) D. (0, 1/8)
(7) Opening direction of x^2 = -4y is ( ).
A. left B. right C. up D. down
(8) Graph of x^2 = -3y is ( ).

Fill in blanks.
(1) y^2 = x: focus ____, directrix ____.
(2) y^2 = -40x: focus ____, directrix ____.
(3) x^2 = 24y: focus ____, directrix ____.
(4) x^2 = -2y: focus ____, directrix ____.
(5) x^2 = -6y: focus ____, directrix ____.
(6) y^2 = 60x: focus ____, directrix ____.

Solve.
(1) Line with slope 2 intersects y^2=4x at A,B. If |AB|=5, find line eq.
(2) For parabola y^2=6x, focus F, directrix l. P on parabola, PA ⟂ l (A on l). If line AF has slope -sqrt(3), find |PF|.

Chapter 10 Self-Assessment Test

Multiple Choice.
(1) Center of x^2+y^2-4x-6y+1=0 is ( ).
A. (2,3) B. (-2,-3) C. (-2,3) D. (2,-3)
(2) Foci of x^2/4 + y^2/3 = 1: ( ).
A. (0,±1) B. (±1,0) C. (0,±2) D. (±2,0)
(3) Foci of x^2/16 - y^2/20 = 1: ( ).
A. (0,±4) B. (±4,0) C. (0,±6) D. (±6,0)
(4) Focus of x^2 = -16y is ( ).
A. (0,4) B. (0,-4) C. (4,0) D. (-4,0)
(5) Radius of x^2+y^2=4 is ( ).
A. 1 B. 2 C. 3 D. 4
(6) Hyperbola y^2 - x^2/4 = 1 has foci on ( ).
A. x-axis B. y-axis C. line y=x D. line y=-x

Fill in blanks.
(1) Focal distance of ellipse x^2/12 + y^2/8 = 1 is ____.
(2) Focus of y^2 = -x is ____.
(3) Eccentricity of y^2/2 - x^2/7 = 1 is ____.
(4) Circle x^2+y^2-2x-4y+3=0: center ____, radius ____.
(5) Circle (x+3)^2+(y-5)^2=3: center ____, radius ____.
(6) Directrix of x^2 = 12y is ____.
(7) Minor axis length of x^2/9 + y^2/12 = 1 is ____.
(8) Asymptotes of x^2/2 - y^2/4 = 1 are ____.
(9) Eccentricity of y^2/4 + x^2/9 = 1 is ____.

Solve.
(1) Ellipse C: x^2/a^2+y^2/b^2=1 (a>b>0) has e=sqrt(3)/2 and passes through B(0,1). Find its eq.
(2) Parabola vertex at origin, focus on y-axis. Line x-2y-1=0 cuts a chord of length sqrt(15). Find parabola eq.
(3) Hyperbola symmetric about axes, distance between vertices 2, foci on y-axis, distance from focus to asymptote is sqrt(2). Find its eq.


Chapter 11: Plane Vectors

11.1 Concepts of Plane Vectors

I. Definition of a Plane Vector

Definition 1: A quantity in a plane that has both magnitude (size) and direction is called a plane vector, or simply a vector. The magnitude is also called the modulus.

We often use a directed line segment (an arrow) to represent a vector. The length of the segment represents the magnitude, and the direction of the arrow represents the direction.

As shown in Figure 11.1-1, the directed line segment (vector) starting at point A (initial point) and ending at point B (terminal point) is written as AB. The modulus of AB is written as |AB|.

A vector whose initial and terminal points are the same is called the zero vector, written as 0. Its direction is undefined. A vector with length equal to 1 is called a unit vector.

Vectors can also be represented by letters a, b, c, etc. The modulus of vector a is |a|.

II. Relationships Between Vectors

Definition 2: If two vectors a and b have equal magnitude and the same direction, they are called equal, written as a = b.

As shown in Figure 11.1-2, in parallelogram ABCD, AB = DC and AD = BC.

Definition 3: If two vectors a and b have the same or opposite directions, they are said to be parallel, written as a // b.

As shown in Figure 11.1-3, a // b // c.

Definition 4: Any set of parallel vectors can be translated onto the same line, so parallel vectors are also called collinear vectors.
It is stated: The zero vector is parallel to any vector.

Summary

A quantity with both magnitude and direction is a vector. Vectors are represented by two uppercase letters (e.g., AB) or one lowercase letter (e.g., a). The magnitude is |a|.

a = b if they have equal magnitude and same direction.

a // b if they have the same or opposite direction.

Parallel vectors are also called collinear vectors.

Exercise 11.1

True/False (√ for correct, × for incorrect).
(1) For vectors a and b, if |a| = |b|, then a = b. ( )
(2) If a and b are both unit vectors, then a = b. ( )
(3) If vectors a and b have the same direction, then a = b. ( )
(4) If vectors a and b have opposite directions, then they are collinear. ( )
(5) If a // b, then a and b have the same direction. ( )
(6) If AB // BC, then points A, B, C are collinear. ( )
(7) For vectors a, b, c, if a // b and b // c, then a // c. ( )

In triangle ABC, D, E, F are midpoints of sides AB, BC, CA respectively. Write vectors equal to AD, BE, CF.

11.2 Linear Operations on Vectors

I. Addition of Vectors

Definition 1: Let a and b be two given vectors. In the plane, pick any point A. Draw AB = a, BC = b. Connect A to C. The vector AC is called the sum of a and b. The sum is written as a + b. So, a + b = AB + BC = AC.

This method is called the Triangle Rule for vector addition (Figure 11.2-1).

Definition 2: Let a and b be two non-parallel vectors. Pick point A. Draw AB = a, AC = b. Construct parallelogram ABDC using AB and AC as adjacent sides. Connect A to D. The vector AD is the sum of a and b: a + b = AB + AC = AB + BD = AD.

This method is called the Parallelogram Rule for vector addition (Figure 11.2-2).

Vector addition satisfies:
(1) a + 0 = a.
(2) a + b = b + a (Commutative Law).
(3) (a + b) + c = a + (b + c) (Associative Law).

Example 1: Simplify.
(1) AB + BA = AA = 0.
(2) AB + BC + CA = AC + CA = AA = 0.
(3) AB + DE + CD + BC = (AB + BC) + (CD + DE) = AC + CE = AE.

II. Subtraction of Vectors

Definition 3: The vector with the same magnitude as AB but opposite direction is called the negative vector of AB, written as -AB.
Clearly, -AB = BA, and AB + (-AB) = AB + BA = AA = 0.

Vector subtraction is the inverse of addition: a - b = a + (-b).

Example 2: Simplify OA - OB.
Solution: OA - OB = OA + (-OB) = OA + BO = BO + OA = BA.

Note: Subtracting two vectors with the same initial point gives a vector from the terminal point of the second to the terminal point of the first.

Example 3: Simplify AB - AC - CD.
Solution: AB - AC - CD = CB - CD = DB.

III. Scalar Multiplication of a Vector

Definition 4: The product of a real number λ (scalar) and a vector a is a vector, written λa. Its magnitude and direction are:
(1) |λa| = |λ| * |a|.
(2) If λ > 0, λa has same direction as a. If λ < 0, λa has opposite direction to a.

Scalar multiplication satisfies:
(1) λ(μa) = (λμ)a.
(2) (λ + μ)a = λa + μa.
(3) λ(a + b) = λa + λb, for λ, μ ∈ ℝ.

Example 4: Simplify.
(1) 3(a+b) - 2(a-b) = 3a+3b - 2a+2b = a + 5b.
(2) 4a - 5(a-b) - 2b = 4a - 5a + 5b - 2b = -a + 3b.
(3) -2(a+b-c) - (a+c) + 4b = -2a-2b+2c - a - c + 4b = -3a + 2b + c.

Example 5: Let e be a unit vector. a = 2e, b = -4e. Find |a|, |b|, |3a - 2b|.
Solution: |a| = |2e| = 2|e| = 2.
|b| = |-4e| = 4|e| = 4.
3a - 2b = 3(2e) - 2(-4e) = 6e + 8e = 14e. |14e| = 14|e| = 14.

Example 6: In parallelogram ABCD, diagonals AC and BD intersect at O. AB = a, AD = b. Express OC and DO in terms of a, b.
Solution: AC = a + b, DB = a - b.
So, OC = (1/2)AC = (1/2)(a + b).
DO = (1/2)DB = (1/2)(a - b).

Theorem: For vectors a, b with a ≠ 0, a and b are parallel ⇔ there exists a real number λ such that b = λa.

Example 7: For points A, B, C, let OA = a - b, OB = 2a - 3b, OC = 3a - 5b. Prove A, B, C are collinear.
Proof: AB = OB - OA = (2a-3b) - (a-b) = a - 2b.
AC = OC - OA = (3a-5b) - (a-b) = 2a - 4b = 2(a - 2b) = 2AB.
So AB // AC, thus A, B, C are collinear.

Summary

Vector Addition:
a. Triangle Rule: AB + BC = AC.
b. Parallelogram Rule: AB + AC = AD.
c. Properties: a+0=a; a+b=b+a; (a+b)+c = a+(b+c).

Vector Subtraction: AB - AC = CB.

Scalar Multiplication:
a. |λa| = |λ||a|. Direction: same if λ>0, opposite if λ<0.
b. Properties: λ(μa)=(λμ)a; (λ+μ)a=λa+μa; λ(a+b)=λa+λb.

Theorem: a // b (with a ≠ 0) ⇔ b = λa for some real λ.

Exercise 11.2

Simplify.
(1) PB + BC.
(2) AM + MN + NP.
(3) EC + FE + CB + BF.
(4) BC - BD.
(5) PA - PB - BC.
(6) AB - AC + BD.
(7) MC + DA - DB - MA.
(8) AC + DB - AB + CD.

Simplify.
(1) 4(a+b) - 2(a-b).
(2) (1/2)(a+b) - (3/2)(a-b).
(3) 2a-3b+8(a-b).
(4) a+3b-5c-2(a+b-2c).
(5) 2a-(b-3c)+4(a+2b-c).
(6) 3(a+2b-c) - (b-2c)+5a.

In triangle ABC, D, E, F are midpoints of AB, BC, CA respectively. AB = a, AC = b. Express BD, AF, DF in terms of a, b.

e is a unit vector. a = -3e, b = 6e, c = 5e. Find |a|, |b|, |c|, |5a-3b+2c|.

In parallelogram ABCD, E,F,G,H are midpoints of AB,BC,CD,DA respectively. Prove: EF = HG.

11.3 Coordinates and Operations of Vectors

I. Definition of Vector Coordinates

Theorem (Fundamental Theorem of Plane Vectors): If a and b are two non-parallel vectors in a plane, then for any vector c in that plane, there exists a unique ordered pair of real numbers (x, y) such that c = xa + yb.

Definition: Let a be any vector. Set up a rectangular coordinate system xOy. Take unit vectors i and j along the positive x-axis and y-axis respectively. By the fundamental theorem, there is a unique pair (x, y) such that a = x i + y j. We call (x, y) the coordinates of vector a, written as a = (x, y). x is the x-coordinate, y is the y-coordinate.

Clearly, i = (1, 0), j = (0, 1), 0 = (0, 0).

Note 1: Translating a vector so its tail is at the origin O, its head gives the coordinates (x, y). |a| = sqrt(x^2 + y^2).

Example 1: Points A(2,2), B(2,-1), C(1,-5). Then OA = (2,2), |OA| = 2*sqrt(2); OB = (2,-1), |OB| = sqrt(5); OC = (1,-5), |OC| = sqrt(26).

II. Coordinate Operations

If a = (x1, y1), b = (x2, y2), then:
a ± b = (x1 ± x2, y1 ± y2).
λa = (λx1, λy1), λ ∈ ℝ.

Example 2: a=(3,-2), b=(-4,5). Find:
(1) a+b = (-1, 3).
(2) (3/2)a = (9/2, -3).
(3) 5a - 3b = (15,-10) - (-12,15) = (27, -25).

Example 3: For points A(x1,y1), B(x2,y2), find AB.
Solution: AB = OB - OA = (x2-x1, y2-y1).

Note 2: The coordinates of a vector equal (end point coordinates) - (start point coordinates).
Note 3: a = b ⇔ x1=x2 and y1=y2.

Example 4: Three forces F1=(3,4), F2=(2,-5), F3=(x,y) have resultant F1+F2+F3=0. Find F3.
Solution: (3,4)+(2,-5)+(x,y)=(0,0) ⇒ (5+x, -1+y) = (0,0). So x=-5, y=1. F3=(-5,1).

III. Coordinates of Parallel Vectors

Let a=(x1,y1) ≠ 0, b=(x2,y2). By the theorem: a // b ⇔ b = λa for some λ ∈ ℝ.
In coordinates: (x2, y2) = λ(x1, y1) = (λx1, λy1). So x2 = λx1, y2 = λy1.
This implies x1*y2 = x2*y1. (If λ=0, it's still true).
If x1≠0 and y1≠0, we also have a // b ⇔ x2/x1 = y2/y1.

Example 5: Vectors a=(-1, x) and b=(-x, 2) are parallel and same direction. Find x.
Solution: Since parallel, (-1)*2 = x*(-x) ⇒ x^2 = 2 ⇒ x = ±√2.
Same direction, so λ>0. Check: For x=√2, b=(-√2,2) = √2*(-1, √2) = √2 * a (√2>0). For x=-√2, λ would be negative. So x = √2.

Example 6: Which vector is parallel to (-3, 2)? ( )
A. (-2,3) B. (-6,5) C. (3,2) D. (9,-6)
Solution: Check ratios: For D, 9/(-3) = -3 and -6/2 = -3. So D. (Others don't match).

Example 7: Points A(-1,-1), B(1,3), C(1,5), D(2,7). Are AB and CD parallel?
Solution: AB = (1-(-1), 3-(-1)) = (2,4).
CD = (2-1, 7-5) = (1,2).
Since (2,4) = 2*(1,2), so AB = 2 CD, thus parallel.

Example 8: In triangle ABC, D and E are midpoints of AB and AC. Prove DE // BC and |DE| = (1/2)|BC|.
Proof: AD = (1/2)AB, AE = (1/2)AC.
So DE = AE - AD = (1/2)AC - (1/2)AB = (1/2)(AC - AB) = (1/2)BC.
Thus DE // BC and |DE| = (1/2)|BC|.

Summary

a = (x, y) means a = x i + y j, where i=(1,0), j=(0,1). |a| = sqrt(x^2+y^2).

Operations: (x1,y1) ± (x2,y2) = (x1±x2, y1±y2). λ(x1,y1) = (λx1, λy1).

For points A(x1,y1), B(x2,y2): AB = (x2-x1, y2-y1).

a = b ⇔ coordinates are equal.

a // b (with a≠0) ⇔ x1*y2 = x2*y1. If x1≠0, y1≠0, also ⇔ x2/x1 = y2/y1.

Exercise 11.3

Fill in blanks.
(1) A(2,-1), B(3,5). AB = ____, |AB| = ____.
(2) C(-2,3), D(3,-4). DC = ____, |DC| = ____.
(3) A(0,1), B(1,2), C(3,4). AB - 2BC = ____, |AB - 2BC| = ____.

a=(2,-5), b=(-3,6). Find coordinates of:
(1) a+b. (2) a-b. (3) -5a. (4) (3/4)b.
(5) -2a+4b. (6) (3/2)a - (1/2)b.

M(3,-2), N(-5,-1). MP = (1/2)MN. Find point P coordinates.

Which vector is parallel to (3, -4)? ( )
A. (-3,-4) B. (-4,3) C. (6,8) D. (-12,16)

Points A(-2,1), B(1,2), C(x,1) are collinear. Find x.

a=(1,2), b=(2,3). Real numbers x,y satisfy xa + yb = (3,4). Find x,y.

In trapezoid ABCD, AB//DC. M,N are midpoints of diagonals AC and BD. Prove MN // DC.

11.4 Scalar Product (Dot Product) of Vectors

I. Definition

Definition 1: Let a and b be non-zero vectors. Pick point O, draw OA=a, OB=b. The angle ∠AOB (between 0° and 180°) is called the angle between a and b, denoted <a, b>.
If <a, b> = 90°, then a and b are perpendicular (orthogonal), written a ⟂ b. The zero vector is perpendicular to any vector.

Note: <a, b> = <b, a>; 0° ≤ <a, b> ≤ 180°.

Definition 2: For vectors a and b, the scalar product (dot product, inner product) is defined as:
a · b = |a| |b| cos(<a, b>).

Note 2: Properties from definition:
(1) Result is a scalar (real number).
(2) a ⟂ b ⇔ a · b = 0.
(3) a · a = |a|^2. Sometimes written a^2 = |a|^2.

Example 1: |a|=4, |b|=√3, <a,b>=30°. Find a·b.
Solution: a·b = 4 * √3 * cos30° = 4√3 * (√3/2) = 6.

Example 2: |a|=3, |b|=8, a·b=-12. Find <a,b>.
Solution: cos(<a,b>) = (a·b)/(|a||b|) = -12/(24) = -1/2. So <a,b> = 120°.

II. Properties
(1) a·b = b·a (Commutative).
(2) λ(a·b) = (λa)·b = a·(λb), λ ∈ ℝ.
(3) (a+b)·c = a·c + b·c (Distributive).

Example 3: |a|=2, |b|=√2. Find (a+b)·(a-b).
Solution: = a·a - a·b + b·a - b·b = |a|^2 - |b|^2 = 4 - 2 = 2.

Example 4: |a|=√5, |b|=3, a·b=-2. Find (a+b)^2 and |a+b|.
Solution: (a+b)^2 = (a+b)·(a+b) = a·a + 2(a·b) + b·b = |a|^2 + 2(a·b) + |b|^2 = 5 + 2(-2) + 9 = 10.
So |a+b| = sqrt(10).

III. Scalar Product in Coordinates

Let a=(x1,y1), b=(x2,y2). Then:
a · b = x1*x2 + y1*y2. (Coordinate formula)

Note 3: Consequences:
(1) |a|^2 = x1^2 + y1^2, so |a| = sqrt(x1^2 + y1^2).
(2) a ⟂ b ⇔ x1*x2 + y1*y2 = 0.
(3) cos(<a,b>) = (a·b)/(|a||b|) = (x1*x2+y1*y2) / [ sqrt(x1^2+y1^2) * sqrt(x2^2+y2^2) ].

Example 5: a=(3,-4), b=(-2,1). Find a·b, |a|, |b|, cos(<a,b>).
Solution: a·b = 3*(-2) + (-4)*1 = -6-4=-10.
|a| = sqrt(9+16)=5.
|b| = sqrt(4+1)=√5.
cos(<a,b>) = -10 / (5*√5) = -2/√5 = -2√5/5.

Example 6: Which vector is perpendicular to (2,-5)? ( )
A. (4,-10) B. (10,4) C. (-2,5) D. (-3,6)
Solution: Check dot product with (2,-5):
A: 2*4 + (-5)*(-10)=8+50=58 ≠0.
B: 2*10 + (-5)*4=20-20=0. So B.

Summary

a·b = |a||b| cos(<a,b>).

0° ≤ <a,b> ≤ 180°, and <a,b>=<b,a>.

In coordinates: a=(x1,y1), b=(x2,y2):
a. a·b = x1*x2 + y1*y2.
b. a ⟂ b ⇔ x1*x2 + y1*y2 = 0.
c. |a| = sqrt(x1^2 + y1^2).
d. cos(<a,b>) = (x1*x2+y1*y2) / [ sqrt(x1^2+y1^2) * sqrt(x2^2+y2^2) ].

Properties: Commutative, scalar factor, distributive over addition.

Exercise 11.4

True/False.
(1) If a=0, then a·b=0 for any b. ( )
(2) If a≠0, then a·b≠0 for any non-zero b. ( )
(3) If a≠0 and a·b=0, then b=0. ( )
(4) If a·b=0, then at least one of a, b is 0. ( )
(5) If a≠0 and a·b = a·c, then b=c. ( )
(6) a·b = a·c implies b=c only if a≠0. ( )
(7) For any a,b,c, (a·b)·c ≠ a·(b·c). ( )
(8) For any a, a·a = |a|^2. ( )

|a|=2, |b|=5, a·b=-3. Find |a+b|, |a-b|.

|a|=12, |b|=9, a·b = -54√2. Find <a,b>.

a·b = -3, a·c = 4. Find:
(1) a·(2b+c). (2) (3b-4c)·a.

For given a, b, find a·b, |a|, |b|, cos(<a,b>).
(1) a=(-1,2), b=(2,3).
(2) a=(4,1), b=(-5,3).
(3) a=(-3,-6), b=(6,-3).
(4) a=(-2,4), b=(3,-1).

Determine if each pair is perpendicular.
(1) a=(-1,1), b=(-2,2).
(2) a=(-3,5), b=(2,7).
(3) a=(3,-10), b=(20,6).
(4) a=(-1/2, 5/2), b=(5,1).

a=(15,6), |b|=√29, a ⟂ b. Find coordinates of b.

Chapter 11 Self-Assessment Test

Simplify.
(1) MA + AB - MC.
(2) DA + PB - PC + BD.
(3) 2b+(a-3c)-3(a-3b-4c).
(4) 2(a-3b+c)-(2b+c)-b.

A(2,-1), B(-3,2), C(3,-1). Find:
AC = ____, BC = ____, 3AC - 2BC = ____, |3AC-2BC| = ____.

a=(-2,-1), b=(1,-3). Find coordinates and magnitude of:
(1) 2a-4b. (2) (1/3)a - (2/3)b.

M(1,-2), N(3,1). MP = 2MN. Find P.

Which vector is parallel to (-1,2)? ( )
A. (2,-4) B. (3,6) C. (0,1) D. (-4,-8)

A(-1,3), B(2,-1), C(x,4) collinear. Find x.

For given a, b, find a·b, |a|, |b|, cos(<a,b>), and <a,b> (use arccos if not special).
(1) a=(0,1), b=(1,-1).
(2) a=(2,-1), b=(-3,3).

Which vector is perpendicular to (3,2)? ( )
A. (-3,-2) B. (6,-4) C. (4,-6) D. (9,6)

|a|=3, |b|=6, a·b=-2. Find |2a+b|, |a-3b|.

|a|=1, |b|=2, <a,b>=120°. Find |5a-2b|.

|a|=1, a·b=-1. Find a·(3a-2b).


Chapter 12: Space Vectors
We learned about plane vectors in Chapter 11. The definitions, operation rules, and properties of vectors can be extended to space simply by removing the restriction "in the plane." For example, we define a space vector as "a quantity in space that has both magnitude and direction." The triangle rule and parallelogram rule are still applicable to the addition of space vectors, and so on. Therefore, the definitions of space vectors, relationships between space vectors, and the definitions and properties of addition, subtraction, and scalar multiplication for space vectors can be referred to in the corresponding sections of Chapter 11. The vectors mentioned in this chapter, unless otherwise specified, all refer to space vectors.

12.1 Spatial Rectangular Coordinate System

I. Definition of a Spatial Rectangular Coordinate System

Definition: In space, arbitrarily choose a point O. Using point O as the origin, construct a plane rectangular coordinate system xOy. Through point O, draw a line perpendicular to the xOy plane, with the same unit length as the x-axis and y-axis. This is the z-axis. This establishes a spatial rectangular coordinate system, denoted as Oxyz.

In the spatial coordinate system Oxyz, point O is called the coordinate origin. The x-axis, y-axis, and z-axis are called the horizontal axis, vertical axis, and depth axis, respectively, collectively known as the coordinate axes. Usually, the positive directions of the three coordinate axes satisfy the right-hand rule: extend your right hand, with the thumb pointing in the positive x-direction, the fingers pointing in the positive y-direction, and the direction your palm faces is the positive z-direction. A coordinate system that satisfies the right-hand rule is called a right-handed system. Unless otherwise stated, the spatial coordinate systems mentioned in this book are all right-handed systems, as shown in Figure 12.1-1.

Any two of the three coordinate axes determine a plane, called a coordinate plane. The plane determined by the x-axis and y-axis is called the xOy plane, the plane determined by the y-axis and z-axis is called the yOz plane, and the plane determined by the z-axis and x-axis is called the zOx plane. The three coordinate planes divide space into eight parts, each called an octant, labeled I, II, ..., VIII for the first octant, second octant, ..., eighth octant, as shown in Figure 12.1-2.

II. Coordinates of a Point in a Spatial Coordinate System

Let M be a point in a spatial coordinate system. Through point M, draw planes perpendicular to the x-axis, y-axis, and z-axis, respectively. They intersect the coordinate axes at points P, Q, and R, and the coordinates of P, Q, and R on the x-axis, y-axis, and z-axis are x, y, and z, respectively. This yields a unique ordered triple of real numbers (x, y, z), called the coordinates of point M, denoted as M(x, y, z), as shown in Figure 12.1-3. The numbers x, y, and z are all called coordinate components of point M, with x called the abscissa of M, y called the ordinate of M, and z called the applicate of M.

1. Coordinates of Special Points

Origin O coordinates: (0, 0, 0);
Points on the x-axis: (x, 0, 0);
Points on the y-axis: (0, y, 0);
Points on the z-axis: (0, 0, z);
Points on the xOy plane: (x, y, 0);
Points on the yOz plane: (0, y, z);
Points on the zOx plane: (x, 0, z).

For example, as shown in Figure 12.1-3, point P on the x-axis has coordinates (x, 0, 0), and point A on the xOy plane has coordinates (x, y, 0).

Example 1: Among the following points, which is on the x-axis? ( ), on the y-axis? ( ), on the z-axis? ( ), on

Full tranlation of ch12
Here is the full, complete translation of Chapter 12 from the PDF you provided, with all available content.

Chapter 12: Space Vectors

We learned about plane vectors in Chapter 11. The definitions, operation rules, and properties of vectors can be extended to space simply by removing the restriction "in the plane." For example, we define a space vector as "a quantity in space that has both magnitude and direction." The triangle rule and parallelogram rule are still applicable to the addition of space vectors, and so on. Therefore, the definitions of space vectors, relationships between space vectors, and the definitions and properties of addition, subtraction, and scalar multiplication for space vectors can be referred to in the corresponding sections of Chapter 11. The vectors mentioned in this chapter, unless otherwise specified, all refer to space vectors.

12.1 Spatial Rectangular Coordinate System

I. Definition of a Spatial Rectangular Coordinate System

Definition: In space, arbitrarily choose a point O. Using point O as the origin, construct a plane rectangular coordinate system xOy. Through point O, draw a line perpendicular to the xOy plane, with the same unit length as the x-axis and y-axis. This is the z-axis. This establishes a spatial rectangular coordinate system, denoted as Oxyz.

In the spatial coordinate system Oxyz, point O is called the coordinate origin. The x-axis, y-axis, and z-axis are called the horizontal axis, vertical axis, and depth axis, respectively, collectively known as the coordinate axes. Usually, the positive directions of the three coordinate axes satisfy the right-hand rule: extend your right hand, with the thumb pointing in the positive x-direction, the fingers pointing in the positive y-direction, and the direction your palm faces is the positive z-direction. A coordinate system that satisfies the right-hand rule is called a right-handed system. Unless otherwise stated, the spatial coordinate systems mentioned in this book are all right-handed systems, as shown in Figure 12.1-1.

Any two of the three coordinate axes determine a plane, called a coordinate plane. The plane determined by the x-axis and y-axis is called the xOy plane, the plane determined by the y-axis and z-axis is called the yOz plane, and the plane determined by the z-axis and x-axis is called the zOx plane. The three coordinate planes divide space into eight parts, each called an octant, labeled I, II, ..., VIII for the first octant, second octant, ..., eighth octant, as shown in Figure 12.1-2.

II. Coordinates of a Point in a Spatial Coordinate System

Let M be a point in a spatial coordinate system. Through point M, draw planes perpendicular to the x-axis, y-axis, and z-axis, respectively. They intersect the coordinate axes at points P, Q, and R, and the coordinates of P, Q, and R on the x-axis, y-axis, and z-axis are x, y, and z, respectively. This yields a unique ordered triple of real numbers (x, y, z), called the coordinates of point M, denoted as M(x, y, z), as shown in Figure 12.1-3. The numbers x, y, and z are all called coordinate components of point M, with x called the abscissa of M, y called the ordinate of M, and z called the applicate of M.

1. Coordinates of Special Points

Origin O coordinates: (0, 0, 0);
Points on the x-axis: (x, 0, 0);
Points on the y-axis: (0, y, 0);
Points on the z-axis: (0, 0, z);
Points on the xOy plane: (x, y, 0);
Points on the yOz plane: (0, y, z);
Points on the zOx plane: (x, 0, z).

For example, as shown in Figure 12.1-3, point P on the x-axis has coordinates (x, 0, 0), and point A on the xOy plane has coordinates (x, y, 0).

Example 1: Among the following points, which is on the x-axis? ( ), on the y-axis? ( ), on the z-axis? ( ), on the xOy plane? ( ), on the yOz plane? ( ), on the zOx plane? ( ).

A. (1, 0, -3)
B. (0, 0, 2)
C. (1, -1, 0)
D. (3, 0, 0)
E. (0, 4, -2)
F. (0, -1, 0)

Solution: Based on the forms of coordinates for special points, the answers are, in order: D, F, B, C, E, A.

2. Characteristics of Points in Each Octant

As shown in Figure 12.1-2, the first octant is formed by the positive x-axis, positive y-axis, and positive z-axis.

The region formed: If a point M(x, y, z) lies in the first octant, then its coordinates satisfy x > 0, y > 0, z > 0.
The second octant is the region bounded by the negative x-axis, positive y-axis, and positive z-axis. If a point M(x, y, z) lies in the second octant, then its coordinates satisfy x < 0, y > 0, z > 0.
This continues for all octants. The signs of coordinates in each octant are shown in Table 12.1-1.

Table 12.1-1 Signs of coordinate components for points in each octant:

Octant I: (+, +, +)
Octant II: (-, +, +)
Octant III: (-, -, +)
Octant IV: (+, -, +)
Octant V: (+, +, -)
Octant VI: (-, +, -)
Octant VII: (-, -, -)
Octant VIII: (+, -, -)

Example 2: Among the following points, which one lies in the second octant?
A. (4, 2, -3)
B. (-1, 3, -2)
C. (-1, -3, 1)
D. (-3, 2, 5)

Solution: The answer is D.

Distances from a point to coordinate planes, coordinate axes, and the origin:

(1) Distance from point (x, y, z) to the xOy-plane: |z|
(2) Distance to the yOz-plane: |x|
(3) Distance to the zOx-plane: |y|
(4) Distance to the x-axis: sqrt(y^2 + z^2)
(5) Distance to the y-axis: sqrt(x^2 + z^2)
(6) Distance to the z-axis: sqrt(x^2 + y^2)
(7) Distance to the origin O: sqrt(x^2 + y^2 + z^2)
(8) Distance from (x, y, z) to (x0, y0, z0): sqrt((x - x0)^2 + (y - y0)^2 + (z - z0)^2)

Example 3: Let A(-1, 3, -2) be a point in space, then:

(1) Distance from A to the xOy-plane: |z| = 2
(2) Distance from A to the yOz-plane: |x| = 1
(3) Distance from A to the zOx-plane: |y| = 3
(4) Distance from A to the x-axis: sqrt(y^2 + z^2) = sqrt(3^2 + (-2)^2) = sqrt(13)
(5) Distance from A to the y-axis: sqrt(x^2 + z^2) = sqrt((-1)^2 + (-2)^2) = sqrt(5)
(6) Distance from A to the z-axis: sqrt(x^2 + y^2) = sqrt((-1)^2 + 3^2) = sqrt(10)
(7) Distance from A to the origin O: sqrt(x^2 + y^2 + z^2) = sqrt(14)
(8) Distance from A to point B(1, -2, -1): sqrt((-1 - 1)^2 + (3 + 2)^2 + (-2 + 1)^2) = sqrt(30)

Symmetric points:
Let A(x, y, z) be a point in space. Then:
(1) If point B is symmetric to A about the xOy-plane, then B = (x, y, -z)
(2) Symmetric about the yOz-plane: B = (-x, y, z)
(3) Symmetric about the zOx-plane: B = (x, -y, z)
(4) Symmetric about the x-axis: B = (x, -y, -z)
(5) Symmetric about the y-axis: B = (-x, y, -z)
(6) Symmetric about the z-axis: B = (-x, -y, z)
(7) Symmetric about the origin O: B = (-x, -y, -z)

Example: Point (1, 2, 4) symmetric about the x-axis is (1, -2, -4); symmetric about the zOx-plane is (1, -2, 4).

Summary:

Understand the definition of a 3D coordinate system.

Understand the coordinates of a point in 3D and locate points in the system.

Know the sign patterns in each octant.

Calculate distances from a point to planes, axes, and the origin; know the distance formula between two points.

Find symmetric points about planes, axes, and the origin.

Exercises 12.1:

In the following points, identify which are in the first octant, second octant, third octant, fourth octant, fifth octant, sixth octant, seventh octant, eighth octant, on the x-axis, on the y-axis, on the z-axis, on the xOy-plane, on the yOz-plane, on the zOx-plane.

Points: (0, -1, 1), (-2, 0, -1), (8, 0, 0), (-3, 3, 1), (3, 2, -4), (-2, 4, 0), (1, 3, 1), (0, 1, 0), (2, -4, 1), (6, -2, -1), (0, 0, -2), (-1, 2, -3), (-2, -3, -2), (-1, -3, 5)

Find the distances from the following points to the xOy-plane, yOz-plane, zOx-plane, x-axis, y-axis, z-axis, and the origin O.

(1) (2, -1, -4)
(2) (1, -2, 6)
(3) (3, -2, -1)
(4) (2, -4, 3)

Find the symmetric points of the following points about the xOy-plane, yOz-plane, zOx-plane, x-axis, y-axis, z-axis, and the origin O.

(1) (3, 2, 5)
(2) (6, -2, 7)
(3) (4, -5, -8)
(4) (-2/3, -1, 2)

Find the distance between the following pairs of points.

(1) (1, -1, -2) and (2, 3, 0)
(2) (1, -2, 3) and (0, -2, 5)
(3) (1, -2, -1) and (-4, 2, 1)
(4) (6, -2, -5) and (8, -3, -4)

12.2 Coordinates and operations of space vectors

Definition of coordinates for space vectors

Definition 1: If a set of vectors in space can be moved to lie in the same plane, they are called coplanar; otherwise, they are called non-coplanar.

Example: In a rectangular box, vectors BC can be moved to lie in plane AA'D'D, so vectors AA', DD', BC are coplanar. But vector AB starts at A in plane AA'D'D and ends at B outside it, so vectors AA', DD', AB are non-coplanar.

Theorem 1 (Coplanar vectors theorem): If two vectors a and b in a plane are not collinear, then vectors a, b, c are coplanar if and only if there exists a unique pair of real numbers (x, y) such that c = x a + y b.

Theorem 2 (Space vectors fundamental theorem): If three vectors a, b, c in space are non-coplanar, then for any vector u in space, there exists a unique triple (x, y, z) such that u = x a + y b + z c.

Definition 2: Let a be any vector in space. Establish a 3D coordinate system Oxyz, with unit vectors i, j, k along the positive x, y, z axes. By the space vectors fundamental theorem, there exists a unique triple (x, y, z) such that a = x i + y j + z k. We call (x, y, z) the coordinates of a, written a = (x, y, z). x is called the x-coordinate, y the y-coordinate, z the z-coordinate. x, y, z are called components.

Clearly, i = (1, 0, 0), j = (0, 1, 0), k = (0, 0, 1), 0 = (0, 0, 0).

Note 1: The coordinates of vector a = (x, y, z) are actually the coordinates of its endpoint when the starting point is moved to the origin O. |a| = sqrt(x^2 + y^2 + z^2).

Example: If point A has coordinates (-1, 2, 3), then |OA| = sqrt(14).

Coordinate operations of space vectors

Given vectors a = (x1, y1, z1), b = (x2, y2, z2), then:
a ± b = (x1 ± x2, y1 ± y2, z1 ± z2)
λ a = (λ x1, λ y1, λ z1), where λ is a real number.

Example 1: Given a = (1, -2, -3), b = (-4, 5, 2), find:

(1) a + b = (1-4, -2+5, -3+2) = (-3, 3, -1)
(2) 2a = (2, -4, -6)
(3) 3a - 2b = (3, -6, -9) - (-8, 10, 4) = (11, -16, -13)

Note 2: For points A(x1, y1, z1), B(x2, y2, z2), vector AB = B - A = (x2 - x1, y2 - y1, z2 - z1).

Example 2: If A(2, -1, -2), B(3, 1, 4), then AB = (1, 2, 6).

Note 3: Given vectors a = (x1, y1, z1), b = (x2, y2, z2), then a = b if and only if x1 = x2, y1 = y2, z1 = z2.

Example 3: Given vectors a = (2, y, -1), b = (x, 3, -5), c = (4, 1, z), and a + b + c = 0. Find x, y, z.

Solution: (2, y, -1) + (x, 3, -5) + (4, 1, z) = (x+6, y+4, z-6) = (0, 0, 0)
So: x+6=0, y+4=0, z-6=0
Therefore: x = -6, y = -4, z = 6.

Coordinates of parallel space vectors

Note 4: Let vector a = (x1, y1, z1) ≠ 0, b = (x2, y2, z2). Then:
a // b ⇔ b = λ a ⇔ x2 = λ x1, y2 = λ y1, z2 = λ z1, for some real number λ.
In particular, if x1, y1, z1 are all non-zero, then a // b ⇔ x2/x1 = y2/y1 = z2/z1.

Example 4: Let a = (1, 0, 2), b = (2, 0, 4). Are a and b parallel?
Solution: b = 2a, so yes, a // b.

Example 5: Which vector is parallel to (1, -4, 2)?
A. (1, 4, 2)
B. (-2, 8, -4)
C. (3, 12, 6)
D. (3, -12, 4)

Solution:
A: 1/1 ≠ 4/(-4), so no.
B: (-2)/1 = 8/(-4) = (-4)/2, so yes.
C: 3/1 ≠ 12/(-4), so no.
D: (-12)/(-4) ≠ 4/2, so no.
Answer: B.

Example 6: Given points A(-1, -1, 2), B(1, 3, 5), C(2, 3, -3), D(8, 15, 6), are vectors AB and CD parallel?
Solution:
AB = B - A = (2, 4, 3)
CD = D - C = (6, 12, 9) = 3(2, 4, 3) = 3 AB
So yes, AB // CD.

Summary:

Coplanar and non-coplanar definitions.

Coordinates of a space vector: a = (x, y, z), |a| = sqrt(x^2 + y^2 + z^2).

If a = (x1, y1, z1), b = (x2, y2, z2), then:
a ± b = (x1 ± x2, y1 ± y2, z1 ± z2)
λ a = (λ x1, λ y1, λ z1), λ ∈ R.

If A(x1, y1, z1), B(x2, y2, z2) are two points, then:
AB = B - A = (x2 - x1, y2 - y1, z2 - z1).

If a = (x1, y1, z1) ≠ 0, b = (x2, y2, z2), then:
a // b ⇔ b = λ a ⇔ x2 = λ x1, y2 = λ y1, z2 = λ z1.
In particular, if x1, y1, z1 are all non-zero, then a // b ⇔ x2/x1 = y2/y1 = z2/z1.

Exercises 12.2:

Find the coordinates and magnitudes of the following vectors.
(1) Given points A(2, 2, -1) and B(-1, 1, -1), find vector AB and |AB|.
(2) Given points C(-2, 1, -1) and D(3, -1, 1), find vector DC and |DC|.
(3) Given points A(0, -3, 1), B(2, 0, -3), C(1, 1, -2), find 2AB - BC and |2AB - BC|.

Given vectors a = (2, 3, -1), b = (-3, 4, 1), find the coordinates of:
(1) a + b
(2) a - b
(3) (2/3)a
(4) -3b
(5) 3a - 5b
(6) (3/4)a - (1/4)b

Given points M(3, -2, 2), N(-5, -1, 4), and MA = -2 MN, find point A.

Which vector is parallel to (3, -4, 1)?
A. (-3, 4, 2)
B. (6, -8, -2)
C. (-9, 12, -3)
D. (-12, 16, 4)

Determine if the following vector pairs are parallel.
(1) (0, 1, 0) and (0, -2, 0)
(2) (1, -2, 0) and (1, 1, 0)
(3) (1, 0, 3) and (2, 1, 6)
(4) (1, 2, -3) and (-2, 4, 6)
(5) (-3, 2, -1) and (9, -6, 3)
(6) (1/2, 1, -3) and (1, 2, -6)

12.3 Dot product of space vectors

Definition and properties of the dot product

The definition and rules for the dot product of space vectors are similar to those for plane vectors.

Definition: Let a and b be two non-zero vectors in space. The quantity |a||b| cos(θ) is called the dot product (or inner product) of a and b, denoted a · b, where θ is the angle between a and b.

Note 1:
(1) The angle between a and b is the same as between b and a; 0° ≤ θ ≤ 180°.
(2) a ⟂ b ⇔ θ = 90°.
(3) The dot product is a scalar, not a vector.
(4) a ⟂ b ⇔ a · b = 0.
(5) a · a = |a|^2; if we write a · a = a^2, then a^2 = |a|^2.

Note 2: Properties of the dot product:
(1) a · b = b · a (commutative)
(2) λ (a · b) = (λ a) · b = a · (λ b), λ ∈ R
(3) (a + b) · c = a · c + b · c (distributive)

Example 1: Given |a| = 2, |b| = √2, θ = 45°, find a · b.
Solution: a · b = 2 * √2 * cos45° = 2√2 * (√2/2) = 2.

Example 2: Given |a| = 4, |b| = 3, a · b = -6√3, find θ.
Solution: cos θ = (a·b)/(|a||b|) = (-6√3)/(12) = -√3/2, so θ = 150°.

Example 3: Given |a| = 4, |b| = 1, a · b = -3, find (a - b)^2 and |a - b|.
Solution:
(a - b)^2 = (a - b)·(a - b) = a·a - 2a·b + b·b = |a|^2 - 2a·b + |b|^2 = 16 + 6 + 1 = 23.
|a - b| = sqrt(23).

Coordinate expression of the dot product

Given vectors a = (x1, y1, z1), b = (x2, y2, z2), then:
a · b = x1 x2 + y1 y2 + z1 z2 (dot product coordinate formula)

Note 3:
(1) |a|^2 = a^2 = a·a = x1^2 + y1^2 + z1^2 ⇒ |a| = sqrt(x1^2 + y1^2 + z1^2)
(2) a ⟂ b ⇔ x1 x2 + y1 y2 + z1 z2 = 0
(3) cos θ = (a·b)/(|a||b|) = (x1x2 + y1y2 + z1z2) / [sqrt(x1^2+y1^2+z1^2) sqrt(x2^2+y2^2+z2^2)]

Example 4: Given a = (1, -2, 2), b = (-2, 1, 1), find a·b, |a|, |b|, cos θ.
Solution:
a·b = 1*(-2) + (-2)*1 + 2*1 = -2
|a| = sqrt(1+4+4) = 3
|b| = sqrt(4+1+1) = √6
cos θ = (-2)/(3√6) = -√6/9.

Example 5: Which vector is perpendicular to (-1, 2, 1)?
A. (-2, -1, 2)
B. (-2, 4, 2)
C. (2, -6, 3)
D. (-1, -1, 1)

Solution:
Check dot product with (-1,2,1):
A: (-1)(-2) + 2*(-1) + 1*2 = 2 -2 +2 = 2 ≠ 0
B: (-1)(-2) + 2*4 + 1*2 = 2 + 8 + 2 = 12 ≠ 0
C: (-1)*2 + 2*(-6) + 1*3 = -2 -12 +3 = -11 ≠ 0
D: (-1)*(-1) + 2*(-1) + 1*1 = 1 -2 +1 = 0
Answer: D.

Summary:

Dot product: a·b = |a||b| cos θ.

θ is the angle between a and b; a ⟂ b ⇔ θ = 90°.

In coordinates:
(1) a·b = x1x2 + y1y2 + z1z2
(2) a ⟂ b ⇔ a·b = 0 ⇔ x1x2 + y1y2 + z1z2 = 0
(3) |a| = sqrt(x1^2 + y1^2 + z1^2)
(4) cos θ = (a·b)/(|a||b|) = (x1x2+y1y2+z1z2) / [sqrt(x1^2+y1^2+z1^2) sqrt(x2^2+y2^2+z2^2)]

Properties: commutative, scalar multiplication, distributive.

Exercises 12.3:

Given |a| = √2, |b| = 3, θ = 30°, find |a+b| and |a-b|.

Given |a| = 1, |b| = √3, a·b = 5, find |a+b| and |a-b|.

Given |a| = 2, |b| = √6, a·b = -3, find |2a+b| and |a - 3b|.

Given |a| = 1, |b| = 6, a·b = 3, find the angle θ between a and b.

Given |a| = √5, |b| = 2√5, a·b = -5, find the angle θ between a and b.

Given |a| = √2, a·b = -3, find a·(4a - 5b).

Given vectors a and b, find a·b, |a|, |b|, cos θ.
(1) a = (-1, 2, 1), b = (2, -1, 3)
(2) a = (4, 0, 1), b = (0, -1, 3)
(3) a = (-2, -1, 0), b = (3, -2, 1)
(4) a = (-2, 1, 4), b = (1, 3, -1)

Determine if the following vector pairs are perpendicular.
(1) a = (-1, 1, 0), b = (-2, -2, 3)
(2) a = (-3, 1, 5), b = (2, 6, 1)
(3) a = (3, -10, 2), b = (10, 2, -5)
(4) a = (-1, 2, 4), b = (5, 1, 1)

Self-test 12:

For point (-4, 1, 2), find distances to: xOy-plane, yOz-plane, zOx-plane, x-axis, y-axis, z-axis, origin O, and to point (-3, -1, 1).

For point (1, -2, -3), find symmetric points about: xOy-plane, yOz-plane, zOx-plane, x-axis, y-axis, z-axis, origin O.

Given points A(1, -2, 1), B(-2, 1, -1), C(-1, -1, 2), find:
AB, BC, and |3AB + BC|.

Given vectors a = (2, 3, -1), b = (-3, 4, 1), find coordinates and magnitudes of:
(1) -2a + 4b
(2) 3a - b

Given points M(1, -3, 2), N(-2, -1, 3), and MA = 3 MN, find point A.

Which vector is parallel to (2, -3, 1)?
A. (-2, -3, 1)
B. (6, -9, -2)
C. (6, -9, 3)
D. (-12, 18, 6)

Given |a| = 2, |b| = 4, θ = 120°, find |2a + 3b|.

Given |a| = 3, |b| = 1, a·b = -2, find |3a - 4b|.

Given |a| = 6, |b| = 3, a·b = -5, find (3a - b)·(a + 2b).

Given a = (-2, 1, 1), b = (1, -1, 4), find a·b, |a|, |b|, cos θ.

Which vector is perpendicular to (1, -4, 2)?
A. (-1, 4, -2)
B. (4, -16, 8)
C. (2, 1, -4)
D. (4, 2, 2)

Given vectors a = (2, 1, 2), b = (-2, x, 2), c = (4, -2, 1). If b ⟂ (a + c), find x.



Chapter 13: Space Planes and Space Lines

13.1 Equations of Space Planes

Definition 1: In space, if the coordinates (x, y, z) of every point on a plane satisfy F(x, y, z) = 0, and every point whose coordinates satisfy F(x, y, z) = 0 lies on that plane, then F(x, y, z) = 0 is the equation of the plane.

Definition 2: If a non-zero vector n is perpendicular to a plane Π, then n is a normal vector of Π.

Note: If n is a normal vector of plane Π, then λn is also a normal vector (λ ≠ 0).

Example: If n = (1/2, 3/2, -1) is a normal vector, then 2n = (1, 3, -2) is also a normal vector.

Let plane Π pass through point M0(x0, y0, z0) with normal vector n = (A, B, C) (A, B, C not all zero). Then for any point M(x, y, z) on the plane, vector M0M is perpendicular to n, so M0M · n = 0. Since M0M = (x - x0, y - y0, z - z0), we have:
A(x - x0) + B(y - y0) + C(z - z0) = 0.
This is the point-normal equation.

Rewriting:
Ax + By + Cz - (Ax0 + By0 + Cz0) = 0.
Let D = -(Ax0 + By0 + Cz0). Then:
Ax + By + Cz + D = 0.

Definition 3: The equation Ax + By + Cz + D = 0 (where A, B, C, D are constants and A, B, C not all zero) is called the general form equation of plane Π.

Note 2: If the plane equation is Ax + By + Cz + D = 0, then its normal vector is n = (A, B, C).

Example 1: Plane Π: 2x + 3y + 5z + 1 = 0. Find its normal vector.
Solution: n = (2, 3, 5).

Example 2: Plane Π passes through M0(1, -2, 3) with normal vector n = (2, -3, -4). Find its equation.
Solution: Equation: 2x - 3y - 4z + D = 0.
Substitute M0: 2*1 - 3*(-2) - 4*3 + D = 0 → 2 + 6 - 12 + D = 0 → D = 4.
Thus: 2x - 3y - 4z + 4 = 0.

Example 3: In a 3D coordinate system, triangular pyramid O-PQR has P(2,0,0), Q(0,3,0), R(0,0,4). Find the equation of plane PQR.
Solution: Let n = (A, B, C) be a normal vector. Then n ⟂ PQ and n ⟂ PR.
PQ = (-2, 3, 0), PR = (-2, 0, 4).
n · PQ = -2A + 3B = 0
n · PR = -2A + 4C = 0
Solve: B = (2/3)A, C = (1/2)A.
Choose A = 6, then B = 4, C = 3. So n = (6, 4, 3).
Plane equation: 6x + 4y + 3z + D = 0.
Substitute P(2,0,0): 12 + D = 0 → D = -12.
Equation: 6x + 4y + 3z - 12 = 0.

Summary

A normal vector n is perpendicular to the plane.

If n is a normal vector, then λn is also a normal vector (λ ≠ 0).

General form: Ax + By + Cz + D = 0, normal vector n = (A, B, C).

Exercises 13.1

Find normal vector n for each plane Π:
(1) x + 4y - 3z - 1 = 0
(2) x - y + 2z - 4 = 0
(3) 2x + y - 3 = 0
(4) y + 3z - 2 = 0
(5) x + 3z = 0
(6) x + 5 = 0
(7) y - 1 = 0
(8) z + 2 = 0

Find the equation of plane Π satisfying:
(1) Through M0(-1, 2, 1), normal vector n = (2, -3, 4)
(2) Through M0(-2, 0, 1), n = (2, -3, 0)
(3) Through M0(0, 2, -3), n = (1, -4, 0)
(4) Through M0(0, 2, 0), n = (1, -3, -2)
(5) Through M0(-3, 2, 5), n = (3, 0, 0)
(6) Through P(2,1,0), Q(-1,0,1), R(1,-1,4)
(7) Through P(1,0,0), Q(-1,3,2), R(0,-2,1)
(8) Through P(1,-1,0), Q(2,0,1), R(-2,3,0)

13.2 Position Relations Between Two Planes

1. Parallel and Perpendicular Planes

Let n1, n2 be normal vectors of planes Π1, Π2 respectively. Then:
(1) n1 // n2 ⇔ Π1 // Π2, or Π1 and Π2 coincide.
(2) n1 ⟂ n2 ⇔ Π1 ⟂ Π2.

If Π1: A1x + B1y + C1z + D1 = 0, Π2: A2x + B2y + C2z + D2 = 0, then:

(1) Π1 // Π2 (and not coinciding) ⇔ there exists λ ≠ 0 such that:
A2 = λA1, B2 = λB1, C2 = λC1, and D2 ≠ λD1.
In particular, if A1B1C1D1 ≠ 0, then:
Π1 // Π2 ⇔ A2/A1 = B2/B1 = C2/C1 ≠ D2/D1.

(2) Π1 and Π2 coincide ⇔ there exists λ ≠ 0 such that:
A2 = λA1, B2 = λB1, C2 = λC1, D2 = λD1.
In particular, if A1B1C1D1 ≠ 0, then:
Π1 and Π2 coincide ⇔ A2/A1 = B2/B1 = C2/C1 = D2/D1.

(3) Π1 ⟂ Π2 ⇔ A1A2 + B1B2 + C1C2 = 0.

Note: If n1 and n2 are not parallel, then Π1 and Π2 intersect. If n1 ⟂ n2, then Π1 ⟂ Π2.

Example 1: Determine the relation between each pair of planes:
(1) Π1: 2x - 3y + z - 2 = 0, Π2: 4x - 6y + 2z - 4 = 0
Solution: 4/2 = (-6)/(-3) = 2/1 = (-4)/(-2) → All equal, so they coincide.

(2) Π1: x + 2y - z - 3 = 0, Π2: 2x + 4y - 2z - 4 = 0
Solution: 2/1 = 4/2 = (-2)/(-1) = 2, but -4/(-3) ≠ 2 → So parallel, not coinciding.

(3) Π1: x + 3y = 0, Π2: 3x + 9y - 5 = 0
Solution: n1 = (1,3,0), n2 = (3,9,0). n2 = 3n1, but D2 ≠ 3D1 (since D1=0, D2=-5). So parallel.

(4) Π1: x - 2y + z - 3 = 0, Π2: 4x + y - 2z + 1 = 0
Solution: Check: 1*4 + (-2)*1 + 1*(-2) = 4 - 2 - 2 = 0 → Perpendicular.

Example 2: Plane Π passes through M0(1, -2, 3) and is parallel to Π1: 3x - 4y + 2z - 1 = 0. Find Π's equation.
Solution: Since parallel, they share normal vector. Take n = (3, -4, 2).
Equation: 3x - 4y + 2z + D = 0.
Substitute M0: 3*1 - 4*(-2) + 2*3 + D = 0 → 3 + 8 + 6 + D = 0 → D = -17.
Thus: 3x - 4y + 2z - 17 = 0.

2. Angle Between Two Planes

Let n1, n2 be normal vectors of Π1, Π2. Let θ be the angle between Π1 and Π2, 0° ≤ θ ≤ 90°. Define:
θ = <n1, n2> if 0° ≤ <n1, n2> ≤ 90°,
θ = 180° - <n1, n2> if 90° ≤ <n1, n2> ≤ 180°.
If Π1 // Π2 or coincide, θ = 0°. If Π1 ⟂ Π2, θ = 90°.

If n1 = (A1, B1, C1), n2 = (A2, B2, C2), then:
cos θ = |cos <n1, n2>| = |n1·n2| / (|n1||n2|) = |A1A2 + B1B2 + C1C2| / (√(A1²+B1²+C1²) √(A2²+B2²+C2²))

Example 3: Find cos θ for:
(1) Π1: 2x - 2y + z = 0, Π2: x - 2y + z - 3 = 0
Solution: n1=(2,-2,1), n2=(1,-2,1).
n1·n2 = 2*1 + (-2)*(-2) + 1*1 = 2+4+1=7.
|n1| = √(4+4+1)=3, |n2|=√(1+4+1)=√6.
cos θ = 7/(3√6) = 7√6/18.

(2) Π1: x - y + z - 7 = 0, Π2: y - 2z + 5 = 0
Solution: n1=(1,-1,1), n2=(0,1,-2).
n1·n2 = 0 + (-1)*1 + 1*(-2) = -3.
|n1| = √3, |n2| = √5.
cos θ = |-3|/(√3 √5) = 3/√15 = √15/5.

Example 4: Find angle between Π1: x - y - 2 = 0, Π2: x - 2y + 2z + 1 = 0.
Solution: n1=(1,-1,0), n2=(1,-2,2).
n1·n2 = 1*1 + (-1)*(-2) + 0*2 = 1+2=3.
|n1|=√2, |n2|=√(1+4+4)=3.
cos θ = 3/(√2 * 3) = 1/√2 = √2/2 → θ = 45°.

Summary

For Π1: A1x+B1y+C1z+D1=0, Π2: A2x+B2y+C2z+D2=0:

Π1 // Π2 (not coinciding) ⇔ A2/A1 = B2/B1 = C2/C1 ≠ D2/D1 (if denominators ≠0).

Π1 and Π2 coincide ⇔ A2/A1 = B2/B1 = C2/C1 = D2/D1.

Π1 and Π2 intersect ⇔ n1 and n2 are not parallel.

Π1 ⟂ Π2 ⇔ A1A2 + B1B2 + C1C2 = 0.

If n1, n2 are normal vectors, θ is angle between planes (0°≤θ≤90°), then:
cos θ = |n1·n2|/(|n1||n2|) = |A1A2+B1B2+C1C2| / (√(A1²+B1²+C1²) √(A2²+B2²+C2²)).

Exercises 13.2

Determine position relations between planes:
(1) Π1: x - 2y + 3z + 1 = 0, Π2: 2x - 4y + 6z - 5 = 0
(2) Π1: 2x + 3y - z - 3 = 0, Π2: 4x + 6y - 2z - 6 = 0
(3) Π1: x + 3 = 0, Π2: x - 5 = 0
(4) Π1: 2y + 4z - 2 = 0, Π2: y + 2z - 1 = 0
(5) Π1: 2x + y + 3 = 0, Π2: x - 2y + 1 = 0
(6) Π1: x + y + 2z - 2 = 0, Π2: x - 3y + z - 6 = 0

Find plane Π equation:
(1) Through M0(-3, -5, 1), parallel to Π1: x - 2y + 6z - 1 = 0
(2) Through M0(2, -3, 4), parallel to Π1: 5x - 3y - 2z - 5 = 0

Find cos θ for:
(1) Π1: x - y + z + 3 = 0, Π2: x - 2y - 2z + 3 = 0
(2) Π1: x - 3y + z - 7 = 0, Π2: 2x - z + 1 = 0
(3) Π1: 2y - z - 3 = 0, Π2: x - 2y - z - 5 = 0
(4) Π1: x - 4y = 0, Π2: y - z - 1 = 0

Find angle θ between planes:
(1) Π1: 2x - y + z - 5 = 0, Π2: x + y + 2z + 3 = 0
(2) Π1: x + y - 1 = 0, Π2: x - y + 3 = 0
(3) Π1: z - 3 = 0, Π2: z + 1 = 0

13.3 Distance Between Parallel Planes

1. Distance from a point to a plane

Let P(x0, y0, z0) be a point not on plane Π: Ax + By + Cz + D = 0. The distance from P to Π is:
d(P, Π) = |Ax0 + By0 + Cz0 + D| / √(A² + B² + C²)

Example 1: Find distance from P(-1, 2, 3) to Π: 2x - y + 2z - 1 = 0.
Solution: d = |2*(-1) + (-1)*2 + 2*3 - 1| / √(4+1+4) = | -2 -2 +6 -1| / 3 = |1|/3 = 1/3.

2. Distance between two parallel planes

Let Π1: Ax + By + Cz + D1 = 0, Π2: Ax + By + Cz + D2 = 0. Then distance from Π1 to Π2 is:
d(Π1, Π2) = |D2 - D1| / √(A² + B² + C²)

Example 2: Find distance between Π1: 2x + y - 3z + 2 = 0 and Π2: 2x + y - 3z - 3 = 0.
Solution: d = | -3 - 2 | / √(4+1+9) = 5 / √14 = (5√14)/14.

Example 3: Find distance between Π1: x + y - 2z + 1 = 0 and Π2: 2x + 2y - 4z + 3 = 0.
Solution: Rewrite Π2: x + y - 2z + 3/2 = 0.
d = | (3/2) - 1 | / √(1+1+4) = (1/2) / √6 = √6 / 12.

Summary

Distance from P(x0,y0,z0) to Π: Ax+By+Cz+D=0:
d = |Ax0+By0+Cz0+D| / √(A²+B²+C²).

Distance between parallel planes Π1: Ax+By+Cz+D1=0, Π2: Ax+By+Cz+D2=0:
d = |D2 - D1| / √(A²+B²+C²).

Exercises 13.3

Find distance from point P to plane Π:
(1) P(-2,1,-1), Π: x+y-z+2=0
(2) P(2,0,-3), Π: 2x+y-2z+1=0
(3) P(-3,1,0), Π: y-2z+1=0
(4) P(1,1,-1), Π: x+3y-z-2=0
(5) P(2,-1,5), Π: x+2y-3=0

Find distance between parallel planes Π1 and Π2:
(1) Π1: x-y-3z+2=0, Π2: x-y-3z-1=0
(2) Π1: √2 x - y + z - 3 = 0, Π2: √2 x - y + z - 5 = 0
(3) Π1: √3 y - z - 1 = 0, Π2: √3 y - z + 3 = 0
(4) Π1: x - √2 y + z + 1 = 0, Π2: 2x - 2√2 y + 2z - 1 = 0
(5) Π1: 6x + 3y - 9z + 1 = 0, Π2: 2x + y - 3z - 2 = 0

13.4 Equations of Space Lines

Definition: If a non-zero vector s is parallel to line L, then s is called a direction vector of L.

Note 1: If s is a direction vector of L, then λs is also a direction vector (λ ≠ 0).

Example: If s = (6, 3, -9) is a direction vector, then (1/3)s = (2, 1, -3) is also a direction vector.

Let line L pass through point M0(x0, y0, z0) with direction vector s = (m, n, p). Then for any point M(x, y, z) on L, vector M0M is parallel to s, so there exists λ ≠ 0 such that M0M = λ s. This gives:
x = x0 + λ m,
y = y0 + λ n,
z = z0 + λ p.
These are the parametric equations of the line.

If m, n, p are all non-zero, we get:
(x - x0)/m = (y - y0)/n = (z - z0)/p.
This is the point-direction form (or symmetric form).

If one component is zero, say p = 0, then:
(x - x0)/m = (y - y0)/n, and z = z0.

If two components are zero, say m = n = 0, then:
x = x0, y = y0, and (z - z0)/p.

Note 2: When m, n, p are not all zero, (x-x0)/m = (y-y0)/n = (z-z0)/p represents the line through (x0,y0,z0) with direction vector (m,n,p).

Example: (x-1)/2 = (y+2)/0 = (z-3)/(-3) is line through (1,-2,3) with direction vector (2,0,-3).

Example 1: Line L passes through M0(2,1,4) with s = (-2,1,3). Find point-direction and parametric equations.
Solution: Point-direction: (x-2)/(-2) = (y-1)/1 = (z-4)/3.
Parametric: Let that equal λ. Then:
x = 2 - 2λ,
y = 1 + λ,
z = 4 + 3λ.

Example 2: L through M0(0,-3,2) with s = (3,-2,0). Find equations.
Solution: Point-direction: x/3 = (y+3)/(-2) = (z-2)/0.
Parametric: x = 3λ, y = -3 - 2λ, z = 2.

Example 3: Line L: (x-1)/0 = (y-3)/2 = (z+4)/(-3). Find a point on L and its direction vector.
Solution: Point: (1,3,-4). Direction vector: (0,2,-3).

Example 4: Line L: (x+1)/2 = (3y-3)/4 = (z-5)/(-2). Find a point and direction vector.
Solution: Rewrite: (x+1)/2 = (y-1)/(4/3) = (z-5)/(-2).
So point: (-1,1,5). Direction vector: (2, 4/3, -2).

Summary

A direction vector s is parallel to the line.

If line passes through M0(x0,y0,z0) with direction vector s=(m,n,p), then:
Parametric: x = x0 + λ m, y = y0 + λ n, z = z0 + λ p.
Point-direction: (x-x0)/m = (y-y0)/n = (z-z0)/p.

Exercises 13.4

For given M0 and s, find point-direction and parametric equations of line L:
(1) M0(-1,-1,2), s=(1,-2,2)
(2) M0(2,-5,3), s=(-3,-2,1)
(3) M0(-2,1,0), s=(4,-2,6)
(4) M0(1,-5,2), s=(3,-4,0)
(5) M0(-1,3,4), s=(2,0,-3)
(6) M0(2,-3,1), s=(0,0,-3)

For given line L, find a point M0 and direction vector s:
(1) L: (x-3)/1 = (y+2)/3 = (z-1)/(-6)
(2) L: (x+1)/(-2) = (y+2)/1 = (z-4)/(-5)
(3) L: (x+3)/(-4) = y/0 = (z-2)/(-8)
(4) L: (x-5)/0 = y/2 = z/0
(5) L: x = 3 - 4λ, y = -2 + 5λ, z = 2 - 3λ
(6) L: x = -2λ, y = 2 + 4λ, z = 3

13.5 Position Relations Between Lines

1. Parallel and perpendicular lines

Let s1, s2 be direction vectors of lines L1, L2 respectively. Then:
(1) s1 // s2 ⇔ L1 // L2, or L1 and L2 coincide.
(2) s1 ⟂ s2 ⇔ L1 ⟂ L2.

If L1, L2 have direction vectors s1 = (m1, n1, p1), s2 = (m2, n2, p2), and L1 and L2 are distinct, then:
L1 // L2 ⇔ s1 // s2 ⇔ m1, n1, p1 are proportional to m2, n2, p2.
L1 ⟂ L2 ⇔ s1 ⟂ s2 ⇔ m1m2 + n1n2 + p1p2 = 0.

Example 1: Line L2 passes through M0(4,-2,3) and is parallel to L1: (x-2)/3 = (y+1)/5 = (z-3)/(-2). Find L2.
Solution: Since parallel, direction vector s = (3,5,-2).
Equation: (x-4)/3 = (y+2)/5 = (z-3)/(-2).

Example 2: Which line L2 is perpendicular to L1: (x-1)/2 = (y+3)/1 = (z-4)/(-5)?
A. L2: x/4 = (y+1)/2 = (z-1)/(-10)
B. L2: (x-2)/3 = y/2 = (z+1)/(-1)
C. L2: (x-5)/4 = y/(-2) = (z+2)/3
D. L2: (x-1)/(-5) = (y+1)/0 = (z+1)/(-2)

Solution: s1 = (2,1,-5).
Check dot product with each s2:
A: s2=(4,2,-10), dot= 8+2+50=60 ≠0 → not ⟂.
B: s2=(3,2,-1), dot=6+2+5=13 ≠0 → not ⟂.
C: s2=(4,-2,3), dot=8-2-15=-9 ≠0 → not ⟂.
D: s2=(-5,0,-2), dot=-10+0+10=0 → ⟂.
Answer: D.

2. Angle between two lines

Let s1, s2 be direction vectors of L1, L2. Let φ be angle between L1 and L2, 0° ≤ φ ≤ 90°. Define:
φ = <s1, s2> if 0° ≤ <s1, s2> ≤ 90°,
φ = 180° - <s1, s2> if 90° ≤ <s1, s2> ≤ 180°.
If L1 // L2 or coincide, φ = 0°. If L1 ⟂ L2, φ = 90°.

If s1 = (m1,n1,p1), s2 = (m2,n2,p2), then:
cos φ = |cos <s1,s2>| = |s1·s2|/(|s1||s2|) = |m1m2 + n1n2 + p1p2| / (√(m1²+n1²+p1²) √(m2²+n2²+p2²))

Example 3: Find cos φ for:
(1) L1: x/(-2) = (y-3)/3 = (z+1)/(-1), L2: (x-7)/2 = (y+3)/1 = (z-1)/(-2)
Solution: s1=(-2,3,-1), s2=(2,1,-2).
s1·s2 = -4+3+2=1.
|s1|=√(4+9+1)=√14, |s2|=√(4+1+4)=3.
cos φ = 1/(3√14) = √14/42.

(2) L1: (x+2)/(-1) = (y-3)/3 = (z+1)/0, L2: (x+8)/(-2) = (y-9)/(-1) = (z+1)/1
Solution: s1=(-1,3,0), s2=(-2,-1,1).
s1·s2 = 2 - 3 + 0 = -1.
|s1|=√(1+9)=√10, |s2|=√(4+1+1)=√6.
cos φ = |-1|/(√10 √6) = 1/√60 = √15/30.

Example 4: Find angle between L1: (x-5)/(-1) = (y+6)/1 = (z-1)/0, L2: (x-3)/1 = (y+5)/0 = (z-4)/1.
Solution: s1=(-1,1,0), s2=(1,0,1).
s1·s2 = -1+0+0 = -1.
|s1|=√2, |s2|=√2.
cos φ = |-1|/(√2 √2) = 1/2 → φ = 60°.

Summary

For lines L1, L2 with direction vectors s1=(m1,n1,p1), s2=(m2,n2,p2), distinct:

L1 // L2 ⇔ s1 // s2 ⇔ m1,n1,p1 proportional to m2,n2,p2.

L1 ⟂ L2 ⇔ s1 ⟂ s2 ⇔ m1m2 + n1n2 + p1p2 = 0.

Angle φ between lines (0° ≤ φ ≤ 90°):
cos φ = |s1·s2|/(|s1||s2|) = |m1m2+n1n2+p1p2| / (√(m1²+n1²+p1²) √(m2²+n2²+p2²)).

Exercises 13.5

Find line L2 satisfying:
(1) Through M0(1,-4,0), parallel to L1: (x-7)/1 = (y+3)/8 = (z-1)/(-2)
(2) Through M0(3,-1,5), parallel to L1: (x-5)/3 = (y+1)/6 = z/(-5)
(3) Through M0(0,0,-2), parallel to L1: (x-2)/(-3) = (y+3)/4 = z/5
(4) Through M0(3,0,-4), parallel to L1: (x-1)/0 = (y-8)/(-5) = (z+4)/2

Find cos φ for:
(1) L1: x/(-1) = (y-1)/2 = (z+1)/(-2), L2: (x-4)/(-2) = (y+3)/1 = (z-1)/2
(2) L1: (x+1)/2 = (y-2)/(-1) = (z+1)/0, L2: (x-√2)/3 = (y+3)/0 = (z-1)/(-4)
(3) L1: (x-3)/3 = (y-5)/0 = (z+1)/2, L2: (x-4)/0 = (y+2)/2 = (z-6)/(-3)
(4) L1: (x-4)/1 = (y+5)/(-1) = z/(-2), L2: (x-2)/2 = (y+3)/(-2) = (z-9)/1

Find angle between:
L1: (x-5)/1 = (y-8)/(-4) = (z+4)/1, L2: (x+4)/(-2) = (y-9)/2 = (z+1)/1

13.6 Position Relations Between a Line and a Plane

1. Line parallel or perpendicular to a plane

Let s = (m, n, p) be direction vector of line L, and n = (A, B, C) be normal vector of plane Π. Then:
(1) L // Π, or L lies in Π ⇔ s ⟂ n ⇔ s·n = 0 ⇔ mA + nB + pC = 0.
(2) L ⟂ Π ⇔ s // n ⇔ m, n, p proportional to A, B, C.

Example 1: Which plane is parallel to line L: (x-1)/2 = (y-3)/(-1) = (z+2)/4?
A. 6x - 3y + 12z + 1 = 0
B. 3x - y + 2z - 5 = 0
C. 4x + 4y - z + 3 = 0
D. x - 2y + z + 4 = 0

Solution: s = (2, -1, 4).
Check dot product with each plane's normal vector:
A: n=(6,-3,12), dot=12+3+48=63≠0 → not parallel.
B: n=(3,-1,2), dot=6+1+8=15≠0 → not parallel.
C: n=(4,4,-1), dot=8-4-4=0 → s ⟂ n, so L // Π (and point (1,3,-2) not on plane).
D: n=(1,-2,1), dot=2+2+4=8≠0 → not parallel.
Answer: C.

Example 2: Find line L through M(2,-1,5) perpendicular to plane Π: 3x + 2y - 4z - 1 = 0.
Solution: Since L ⟂ Π, s // n = (3,2,-4). Take s = (3,2,-4).
Equation: (x-2)/3 = (y+1)/2 = (z-5)/(-4).

2. Angle between a line and a plane

Let s = (m,n,p) be direction vector of L, n = (A,B,C) normal vector of Π. If s·n ≠ 0, then L intersects Π at one point. Let φ be angle between L and Π, 0° ≤ φ ≤ 90°, and:
sin φ = |cos <s, n>| = |s·n|/(|s||n|) = |mA + nB + pC| / (√(m²+n²+p²) √(A²+B²+C²))

Example 3: Find angle between L: (x+1)/1 = (y-2)/1 = (z+3)/2 and Π: 2x - y + z - 5 = 0.
Solution: s = (1,1,2), n = (2,-1,1).
s·n = 2 -1 +2 = 3.
|s| = √6, |n| = √6.
sin φ = |3|/(√6 √6) = 3/6 = 1/2 → φ = 30°.

Summary

Let s = (m,n,p) be direction vector of L, n = (A,B,C) normal vector of Π.

(1) L // Π or L lies in Π ⇔ s ⟂ n ⇔ s·n = 0 ⇔ mA+nB+pC=0.
(2) L ⟂ Π ⇔ s // n ⇔ m,n,p proportional to A,B,C.

If φ is angle between L and Π (0° ≤ φ ≤ 90°), then:
sin φ = |s·n|/(|s||n|) = |mA+nB+pC| / (√(m²+n²+p²) √(A²+B²+C²)).

Exercises 13.6

Determine if line and plane are parallel or perpendicular:
(1) L: (x-1)/2 = y/3 = (z+1)/(-2), Π: 4x+6y-4z-5=0
(2) L: (x+2)/(-3) = (y-3)/2 = (z+1)/0, Π: 2x+3y-7z-1=0
(3) L: (x-2)/4 = y/(-2) = (z+3)/2, Π: 4x+5y-3z+3=0
(4) L: (x-2)/5 = y/(-2) = (z+3)/(-3), Π: 10x-4y-6z+1=0

Find sin φ for:
(1) L: (x-1)/1 = (y+3)/(-2) = (z+1)/(-2), Π: x+y-3z-5=0
(2) L: (x+2)/(-1) = (y-4)/2 = (z+5)/0, Π: 2x-y-z-1=0
(3) L: (x-6)/2 = (y+1)/0 = (z+2)/(-1), Π: x-y+3=0
(4) L: (x+5)/0 = (y-3)/2 = (z+1)/(-1), Π: √2 x - y - z + 1 = 0

Find angle between:
L: (x-6)/2 = (y-3)/1 = (z+1)/(-1), Π: 3x+6y+3z-5=0

Self-test 13

Find plane Π through M0(2,-5,3) with normal vector n = (-2,-1,3).

Find plane Π through P(3,-1,1), Q(2,-5,1), R(-4,3,1).

Find plane Π through M0(-4,-1,2) parallel to Π1: x-5y-3z+3=0.

Find cos θ between Π1: 2x-y+2z+6=0 and Π2: 3x-y+5z+1=0.

Find distance from P(-3,2,-1) to Π: x+2y-z+6=0.

Find distance between parallel planes:
(1) Π1: x-4z+2=0, Π2: x-4z-1=0
(2) Π1: 4x-y-2z+1=0, Π2: 8x-2y-4z+3=0

For given M0 and s, find point-direction and parametric equations of L:
(1) M0(2,-1,3), s=(-1,-2,3)
(2) M0(0,-3,2), s=(-2,0,1)
(3) M0(-2,1,4), s=(2,0,0)

Find line L2 through M0(-1,-2,6) parallel to L1: (x-1)/3 = (y+1)/(-4) = (z-4)/(-5).

Find angle between L1: (x-3)/(-2) = (y-2)/1 = (z+4)/(-1) and L2: (x-2)/(-1) = (y+1)/1 = (z-5)/2.

Find angle between L: (x-3)/(-2) = (y+1)/2 = (z+4)/1 and Π: x-z-1=0.


Chapter 14: Limits of Sequences and Functions

14.1 Limits of Sequences

1. Definition of a sequence limit

Definition: Let {a_n} be a sequence, and a a constant. If as n increases indefinitely (n approaches infinity), a_n approaches a indefinitely (i.e., |a_n - a| approaches 0), then a is called the limit of the sequence {a_n} as n approaches infinity, or the sequence {a_n} converges to a, written as lim_{n→∞} a_n = a. Otherwise, the limit lim_{n→∞} a_n does not exist.

Note 1: lim_{n→∞} a_n = a ⇔ "n → ∞ ⇒ a_n → a".

Example 1: lim_{n→∞} 1/n = 0.
Example 2: lim_{n→∞} ((-1)^n)/n = 0.
Example 3: lim_{n→∞} 1/(2^n) = 0.
Example 4: lim_{n→∞} C = C (C constant).
Example 5: The limit of sequence {n} does not exist.
Example 6: The limit of sequence {(-1)^n} does not exist.

2. Arithmetic operations for sequence limits

Theorem: Let {a_n} and {b_n} be two sequences. If lim_{n→∞} a_n = a and lim_{n→∞} b_n = b, then:
(1) lim_{n→∞} (a_n + b_n) = a + b.
(2) lim_{n→∞} (a_n - b_n) = a - b.
(3) lim_{n→∞} (a_n * b_n) = a * b.
Special case: lim_{n→∞} (C * a_n) = C * a (C constant).
(4) If b_n ≠ 0 for all n ∈ ℤ⁺ and b ≠ 0, then lim_{n→∞} (a_n / b_n) = a / b.

Example 7: lim_{n→∞} (1/n + 1/(2^n)) = 0 + 0 = 0.
Example 8: lim_{n→∞} (1 - 3/(2^n)) = 1 - 0 = 1.
Example 9: lim_{n→∞} (2n³ + 3)/(n³ - 2n) = 2/1 = 2.
Calculation: Divide numerator and denominator by n³: (2 + 3/n³)/(1 - 2/n²) → (2+0)/(1-0) = 2.
Example 10: lim_{n→∞} (n³ - 4n)/(3n⁵ + 3n² - 5) = 0/3 = 0.
Divide by n⁵: (1/n² - 4/n⁴)/(3 + 3/n³ - 5/n⁵) → (0-0)/(3+0-0) = 0.

Note 2:
(1) lim_{n→∞} (a_0 nᵏ + a_1 nᵏ⁻¹ + ... + a_k) / (b_0 nˡ + b_1 nˡ⁻¹ + ... + b_l) where k, l ∈ ℤ⁺, a_0 ≠ 0, b_0 ≠ 0:
= 0 if k < l,
= a_0 / b_0 if k = l,
= ∞ if k > l.
(2) Warning: lim_{n→∞} (1/n + 1/n + ... + 1/n) (n terms) ≠ lim_{n→∞} 1/n + ... + lim_{n→∞} 1/n = 0.
Actually, lim_{n→∞} n*(1/n) = 1.

Example 11: Find lim_{n→∞} (1+2+3+...+n)/n².
Solution: Sum = n(n+1)/2. So limit = lim_{n→∞} (n²+n)/(2n²) = 1/2.

Summary

a is limit of {a_n} as n→∞ ⇔ lim_{n→∞} a_n = a ⇔ "n→∞ ⇒ a_n→a".

Arithmetic operations for limits (as given in theorem).

Formula for rational sequences: as above in Note 2 (1).

Exercises 14.1

Find the limits:

lim_{n→∞} ((3n+1)/(2n) + 1/(2^n))

lim_{n→∞} (1/(3n²) - 3/(2n+5))

lim_{n→∞} 4/((2n+1)³)

lim_{n→∞} (4n²+1)/(3n²-n)

lim_{n→∞} (2n⁴ - n² + 1)/(5n⁴ - 1)

lim_{n→∞} (3n⁷ + 3n⁵ + 1)/(n⁷ - 5n⁶)

lim_{n→∞} (n²⁰ - 3n¹⁵ + 6n⁹ + 3)/(n²¹ - n⁶ + 8)

lim_{n→∞} (2n⁸ - 10n⁵ + 2n³ - 3)/(3n¹¹ - 3n³ + 1)

lim_{n→∞} (2n⁶ - 5n³ + 2n² - 1)/(3n¹⁰ - 3n³ + 5n + 4)

lim_{n→∞} (1/n + 1/n + 1/n)

lim_{n→∞} (1+3+5+...+(2n-1))/(2n²+1)

lim_{n→∞} (2+4+8+...+2ⁿ)/(2ⁿ⁺²)

14.2 Limits of Functions

1. Limit of a function as x → x₀

Definition 1: Let a, δ ∈ ℝ, δ > 0. The set {x | |x - a| < δ} is called the δ-neighborhood of point a, denoted U(a, δ). a is the center, δ the radius.
U(a, δ) = (a - δ, a + δ).
U̇(a, δ) = {x | 0 < |x - a| < δ} = (a - δ, a) ∪ (a, a + δ) is the deleted neighborhood of a.
U₋(a, δ) = (a - δ, a] is the left neighborhood.
U₊(a, δ) = [a, a + δ) is the right neighborhood.
U̇₋(a, δ) = (a - δ, a) is the left deleted neighborhood.
U̇₊(a, δ) = (a, a + δ) is the right deleted neighborhood.

Definition 2: Let function f(x) be defined in some deleted neighborhood of x₀, and A a constant. If in that deleted neighborhood, as x approaches x₀ from both sides (|x - x₀| → 0, but x ≠ x₀), f(x) approaches A (|f(x) - A| → 0), then A is called the limit of f(x) as x approaches x₀, written lim_{x→x₀} f(x) = A. Otherwise, the limit does not exist.

Note 1: lim_{x→x₀} f(x) = A ⇔ "x → x₀ ⇒ f(x) → A".

Example 1: lim_{x→x₀} C = C (C constant).
Example 2: lim_{x→x₀} x = x₀.
Example 3: f(x) = { x, x ≠ 0; 1, x = 0 }. Find lim_{x→0} f(x).
Solution: lim_{x→0} f(x) = lim_{x→0} x = 0.

2. Arithmetic operations for limits (x → x₀)

Theorem 1: If lim_{x→x₀} f(x) = A, lim_{x→x₀} g(x) = B, then:
(1) lim_{x→x₀} [f(x) + g(x)] = A + B.
(2) lim_{x→x₀} [f(x) - g(x)] = A - B.
(3) lim_{x→x₀} [f(x) * g(x)] = A * B.
Special case: lim_{x→x₀} [C * f(x)] = C * A (C constant).
(4) If g(x) ≠ 0 and B ≠ 0, then lim_{x→x₀} [f(x)/g(x)] = A / B.

Example 4: lim_{x→1} (3x+1) = 4.
Example 5: lim_{x→x₀} x² = x₀².
Example 6: lim_{x→x₀} xⁿ = x₀ⁿ (n ∈ ℤ⁺).
Example 7: lim_{x→1} (2x² - 3x + 1) = 0.
Example 8: lim_{x→0} (5x⁴ - 2x³ + 1)/(x² + 1) = 1/1 = 1.
Example 9: lim_{x→2} (x² - 4)/(x - 2) = lim_{x→2} (x+2) = 4.

3. Left-hand and right-hand limits

Definition 3: Let f(x) be defined in some left deleted neighborhood of x₀, A a constant. If as x approaches x₀ from the left (x < x₀ and x₀ - x → 0), f(x) approaches A, then A is the left-hand limit, written lim_{x→x₀⁻} f(x) = A.

Definition 4: Similarly for right-hand limit: lim_{x→x₀⁺} f(x) = A.

Example: f(x) = { x+1, x < 0; x, x ≥ 0 }. Then lim_{x→0⁻} f(x) = 1, lim_{x→0⁺} f(x) = 0.

Example 10: f(x) = { 2x² + 1, x < 1; -3x, x ≥ 1 }. Find lim_{x→1⁻} f(x) and lim_{x→1⁺} f(x).
Solution: lim_{x→1⁻} f(x) = 3, lim_{x→1⁺} f(x) = -3.

Theorem 2: lim_{x→x₀} f(x) = A ⇔ lim_{x→x₀⁻} f(x) = lim_{x→x₀⁺} f(x) = A.

Example 11: f(x) = { -1, x < 0; 0, x = 0; 1, x > 0 }. Does limit exist at x=0?
Solution: lim_{x→0⁻} f(x) = -1, lim_{x→0⁺} f(x) = 1. Not equal, so limit does not exist.

Example 12: f(x) = { ax + 1, x < 2; 3x - 2, x ≥ 2 }. If limit exists at x=2, find a.
Solution: lim_{x→2⁻} f(x) = 2a + 1, lim_{x→2⁺} f(x) = 4. For limit to exist, 2a+1 = 4 → a = 3/2.

4. Limit as x → ∞

Definition 5: Let f(x) be defined for |x| sufficiently large, A a constant. If as |x| → ∞ (x → +∞ and x → -∞), f(x) → A, then A is the limit of f(x) as x → ∞, written lim_{x→∞} f(x) = A.

Note 2: lim_{x→∞} f(x) = A ⇔ "x → ∞ ⇒ f(x) → A".

Example 13: lim_{x→∞} 1/x = 0.
Example 14: If |a| < 1, then lim_{x→∞} aˣ = 0.

5. Arithmetic operations for limits (x → ∞)

Theorem 3: If lim_{x→∞} f(x) = A, lim_{x→∞} g(x) = B, then:
(1) lim_{x→∞} [f(x) + g(x)] = A + B.
(2) lim_{x→∞} [f(x) - g(x)] = A - B.
(3) lim_{x→∞} [f(x) * g(x)] = A * B.
Special case: lim_{x→∞} [C * f(x)] = C * A.
(4) If g(x) ≠ 0 and B ≠ 0, then lim_{x→∞} [f(x)/g(x)] = A / B.

Example 15: lim_{x→∞} (2x² + 1)/(x⁴ - x³ + 1) = 0.
Example 16: lim_{x→∞} (5x⁵ - 3x² + 2)/(x⁵ - x⁴ - 4) = 5.

Note 3:
(1) lim_{x→∞} (a_0 xᵏ + a_1 xᵏ⁻¹ + ... + a_k) / (b_0 xˡ + b_1 xˡ⁻¹ + ... + b_l) =
0 if k < l,
a_0/b_0 if k = l,
∞ if k > l.
(2) lim_{x→+∞} f(x) = A ⇔ "x → +∞ ⇒ f(x) → A".
lim_{x→-∞} f(x) = A ⇔ "x → -∞ ⇒ f(x) → A".
Example: lim_{x→+∞} arctan x = π/2, lim_{x→-∞} arctan x = -π/2.
(3) lim_{x→∞} f(x) = A ⇔ lim_{x→-∞} f(x) = lim_{x→+∞} f(x) = A.

Summary

Limits as x → x₀:

Neighborhood definitions.

Limit definition: lim_{x→x₀} f(x) = A ⇔ "x→x₀ ⇒ f(x)→A".

Left-hand and right-hand limits.

Limit exists iff left and right limits exist and are equal.

Arithmetic operations.

Limits as x → ∞:

Limit definition: lim_{x→∞} f(x) = A ⇔ "x→∞ ⇒ f(x)→A".

Left/right limits at infinity.

Arithmetic operations.

Formula for rational functions.

Formula for rational functions limit as x→∞ (as in Note 3 (1)).

Exercises 14.2

Find limits:
(1) lim_{x→2} (3x - 1)
(2) lim_{x→1} (2x² + 5)
(3) lim_{x→-1} (-5x + 1)
(4) lim_{x→0} (2x⁴ - 3x³ + 2)/(x² + 1)
(5) lim_{x→-4} (x² - 16)/(x + 4)
(6) lim_{x→5} (x² - 25)/(x - 5)

f(x) = { 3 sin x, x < 0; 2x², x ≥ 0 }. Find lim_{x→0⁻} f(x), lim_{x→0⁺} f(x), and determine if lim_{x→0} f(x) exists.

f(x) = { 5x² - 3x + 1, x < 0; 2ˣ, x ≥ 0 }. Find lim_{x→0⁻} f(x), lim_{x→0⁺} f(x), and determine if limit exists.

f(x) = { 3ˣ + 1, x < 2; x², x ≥ 2 }. Find lim_{x→2⁻} f(x), lim_{x→2⁺} f(x), and determine if limit exists.

f(x) = { 7 sin x + 2a, x < 0; 3x - 2, x ≥ 0 }. If limit exists at x=0, find a.

f(x) = { 3x² - 1, x < 3; ax + 2, x ≥ 3 }. If limit exists at x=3, find a.

Find limits:
(1) lim_{x→∞} (4x⁴ - 3x³ + 2)/(10x⁵ + 1)
(2) lim_{x→∞} (3x² - 1)/(5x² + 1)
(3) lim_{x→∞} (3x³ + 6x - 1)/(4x⁶ - 5)
(4) lim_{x→∞} (2x⁴ - 3x³ + 2)/(4x⁴ - 4x + 9)
(5) lim_{x→∞} (x² + 3x - 7)/(5x⁴ + 3)
(6) lim_{x→1} (12x¹⁰ - 1)/(6x¹⁰ + x⁶ + 5) (Note: probably x→∞, but written x→1; if x→1, just substitute).
(7) lim_{x→∞} [2 + (3/4)ˣ]
(8) lim_{x→∞} (eˣ - 5)
(9) lim_{x→∞} (3ˣ - 4ˣ)/(3ˣ + 4ˣ)

14.3 Continuous Functions

Definition 1: Let f(x) be defined in some neighborhood of x₀. If lim_{x→x₀} f(x) = f(x₀), then f(x) is continuous at x₀.

Example: f(x) = x is continuous at any x₀.

Example 1: f(x) = { x+1, x ≠ 0; 2, x = 0 }. Is f continuous at x₀=0?
Solution: lim_{x→0} f(x) = 1 ≠ f(0)=2, so not continuous.

Definition 2: f(x) is left-continuous at x₀ if lim_{x→x₀⁻} f(x) = f(x₀).
Definition 3: f(x) is right-continuous at x₀ if lim_{x→x₀⁺} f(x) = f(x₀).

Theorem: f(x) is continuous at x₀ ⇔ f(x) is both left-continuous and right-continuous at x₀. That is, lim_{x→x₀⁻} f(x) = lim_{x→x₀⁺} f(x) = f(x₀).

Example 2: f(x) = { sin x + 1, x < 0; 1, x = 0; 2ˣ, x > 0 }. Is f continuous at x₀=0?
Solution: lim_{x→0⁻} f(x)=1, lim_{x→0⁺} f(x)=1, f(0)=1. So yes, continuous.

Example 3: f(x) = { ax + 1, x < 1; b, x = 1; x² - 3, x > 1 }. If f is continuous at x₀=1, find a and b.
Solution: lim_{x→1⁻} f(x) = a+1, lim_{x→1⁺} f(x) = -2. For continuity, a+1 = -2 = b. So a = -3, b = -2.

Note: Basic elementary functions (constant, power, exponential, logarithmic, trigonometric, inverse trigonometric) are continuous on their domains.

Summary

f continuous at x₀ ⇔ lim_{x→x₀} f(x) = f(x₀).

f left-continuous at x₀ ⇔ lim_{x→x₀⁻} f(x) = f(x₀).
f right-continuous at x₀ ⇔ lim_{x→x₀⁺} f(x) = f(x₀).

f continuous at x₀ ⇔ left and right limits exist and equal f(x₀).

Exercises 14.3

Fill in blanks:
(1) lim_{x→π/2} (sin x + 2 cos x) = ?
(2) lim_{x→π/4} (3 tan x - 2) = ?
(3) lim_{x→0} (sin x - 2 cos x) = ?
(4) lim_{x→4} [(1/2)ˣ - log₂ x] = ?
(5) lim_{x→1/2} (2 arcsin x - arccos x) = ?
(6) lim_{x→-√3} (arctan x - arccot x) = ?

f(x) = { 3x+1, x < 0; 4ˣ, x ≥ 0 }. Is f continuous at x₀=0?

f(x) = { 2/x + 5, x < 2; (1/2)x³ + 2, x ≥ 2 }. Is f continuous at x₀=2?

f(x) = { x - 1, x < 3; 3, x = 3; log₃ x + 1, x > 3 }. Is f continuous at x₀=3?

f(x) = { x + 3a, x < -1; -3, x = -1; 3x - 2b, x > -1 }. If f continuous at x₀=-1, find a and b.

f(x) = { (1/2)x - 1, x < 1; a - 2b, x = 1; x + b, x > 1 }. If f continuous at x₀=1, find a and b.

14.4 Two Important Limits

Theorem 1: lim_{x→0} (sin x)/x = 1.

Example 1: Find lim_{x→0} (tan x)/x.
Solution: lim_{x→0} (sin x)/(x cos x) = 1 * 1 = 1.

Note 1: If □ → 0, then lim (sin □)/□ = 1 (where □ is an expression in x).

Example 2: lim_{x→1} sin(x-1)/(x-1) = 1.
Example 3: lim_{x→∞} x sin(1/x) = 1.
Example 4: lim_{x→0} (arcsin x)/x = 1. (Substitution: let arcsin x = t, then x = sin t, as x→0, t→0, limit = lim_{t→0} t/sin t = 1.)
Example 5: lim_{x→0} (sin 2x)/x = 2.
Example 6: lim_{x→0} (1 - cos x)/x² = 1/2.
Example 7: lim_{n→∞} n tan(2/n) = 2.

Theorem 2: lim_{x→∞} (1 + 1/x)ˣ = e.

Note 2: If □ → 0, then lim (1 + □)^{1/□} = e.

Example 8: lim_{x→0} (1 + x)^{1/x} = e.
Example 9: lim_{x→0} (1 + x/2)^{1/x} = e^{1/2}.
Example 10: lim_{x→∞} (1 - 3/x)ˣ = e⁻³.

Theorem 3: If lim f(x) = A, lim g(x) = B, then lim f(x)^{g(x)} = Aᴮ (for x→x₀ or x→∞).

Example 11: lim_{x→∞} ((x+3)/(x+1))ˣ = e².
Example 12: lim_{n→∞} (1 + 1/(n²+2n))^{2n²} = e².

Summary

Important limit formulas:
(1) lim_{x→0} (sin x)/x = 1.
(2) If □ → 0, then lim (sin □)/□ = 1.
(3) lim_{x→∞} (1 + 1/x)ˣ = e.
(4) If □ → 0, then lim (1 + □)^{1/□} = e.

Exercises 14.4

Find limits:
(1) lim_{x→0} (sin 4x)/x
(2) lim_{x→0} (tan 3x)/x
(3) lim_{x→0} (sin 2x)/(sin 3x)
(4) lim_{x→∞} x sin(2/x)
(5) lim_{x→0} (arctan 5x)/x
(6) lim_{x→2} sin(x-2)/(x²-4)
(7) lim_{x→∞} (sin 2x - sin x)/(3x)
(8) lim_{x→0} (tan x - sin x)/(5x³)
(9) lim_{x→∞} (cos 2x - cos x)/(2x²)

Find limits:
(1) lim_{x→0} (1 + 2x)^{1/x}
(2) lim_{x→0} (1 - 3x)^{2/x}
(3) lim_{x→∞} ((1+x)/x)^{3x}
(4) lim_{x→∞} ((3x-1)/(3x+1))ˣ
(5) lim_{x→∞} (1 + x/(x²+1))^{2x}
(6) lim_{x→∞} (1 - 1/(2x²-1))^{x²}
(7) lim_{n→∞} ((3n+1)/(3n+3))ⁿ
(8) lim_{x→0} ln(1+2x)/x
(9) lim_{x→0} (1 - 3x)^{2/sin x}

14.5 Infinitesimals (Infinitesimals)

1. Definition of infinitesimals

Definition 1: If lim_{x→x₀} α(x) = 0 (or lim_{x→∞} α(x) = 0), then α(x) is an infinitesimal as x → x₀ (or x → ∞).

Example: sin x is infinitesimal as x→0; 1/x is infinitesimal as x→∞.

Definition 2: Let α(x), β(x) be infinitesimals as x → x₀ (or x→∞).
(1) If lim α(x)/β(x) = 0, then α(x) is a higher-order infinitesimal relative to β(x), denoted α(x) = o(β(x)).
(2) If lim α(x)/β(x) = C ≠ 0 (C constant), then α(x) and β(x) are same-order infinitesimals.
If C = 1, they are equivalent infinitesimals, denoted α(x) ∼ β(x).

Example:
(1) lim_{x→0} (sin² x)/x = 0, so sin² x = o(x).
(2) lim_{x→0} (x²+2x)/x = 2, so x²+2x and x are same-order.
(3) lim_{x→0} (sin x)/x = 1, so sin x ∼ x.

Note: Common equivalent infinitesimals as x→0:
sin x ∼ x, tan x ∼ x, arcsin x ∼ x, arctan x ∼ x,
1 - cos x ∼ (1/2)x², ln(1+x) ∼ x, eˣ - 1 ∼ x,
aˣ - 1 ∼ x ln a, (1+x)ᵃ - 1 ∼ a x.
If □ → 0, then sin □ ∼ □, tan □ ∼ □, etc.

2. Substitution of equivalent infinitesimals

Theorem (Infinitesimal substitution): If α(x) ∼ β(x), then:
(1) If lim f(x) α(x) = A, then lim f(x) β(x) = A.
(2) If lim f(x)/α(x) = A, then lim f(x)/β(x) = A.

Example 1: lim_{x→0} [x ln(1+x)]/x².
Solution: ln(1+x) ∼ x, so limit = lim x*x / x² = 1.

Example 2: lim_{x→0} (1 - cos x)/(sin 3x tan 4x).
Solution: 1-cos x ∼ x²/2, sin 3x ∼ 3x, tan 4x ∼ 4x. So limit = (x²/2)/(12x²) = 1/24.

Example 3: lim_{x→0} [(2+x)(e^{x²}-1)]/(arcsin x²).
Solution: e^{x²}-1 ∼ x², arcsin x² ∼ x², 2+x → 2. So limit = 2x²/x² = 2.

Example 4: lim_{x→0} [x arctan 2x]/[√(1-2x²)-1].
Solution: arctan 2x ∼ 2x, √(1-2x²)-1 ∼ (1/2)(-2x²) = -x². So limit = (x * 2x)/(-x²) = -2.

Summary

α(x) infinitesimal as x→x₀ ⇔ lim α(x)=0.

Comparison:

Higher-order: lim α/β = 0 ⇔ α = o(β).

Same-order: lim α/β = C ≠ 0.

Equivalent: C=1 ⇔ α ∼ β.

Common equivalent infinitesimals (x→0): as listed.

Substitution theorem: If α ∼ β, then can substitute in products/quotients.

Exercises 14.5

Find limits:

lim_{x→0} (sin 2x)/(tan 3x)

lim_{x→0} (arcsin 4x)/(arctan 5x)

lim_{x→0} (1 - cos 4x²)/(3x⁴)

lim_{x→0} ln(1+4x⁵)/x⁵

lim_{x→0} ((1+x)⁵ - 1)/(2x)

lim_{x→0} ((1-4x²)^{1/4} - 1)/(2x²)

lim_{x→0} ((1+cos x)(1-cos x))/(5x²)

lim_{x→0} (2x³(1+x³))/(e^{x³} - 1)

lim_{x→0} ln(1-2x)/(sin 3x)

lim_{x→0} ((2ˣ-1)(3+x²))/ln(1+6x)

lim_{x→0} (∛(1-3x³) - 1)/(x² tan x)

Self-test 14

Find limits:
(1) lim_{n→∞} ((2n³-1)/n³ + 1/3ⁿ)
(2) lim_{n→∞} (6n¹⁰ - n⁵ + 7n + 3)/(8n¹⁵ - n⁶ - 8n²)
(3) lim_{n→∞} (2+4+6+...+2n)/(2n²)
(4) lim_{x→0} (2x³ - 3)/(5x² + 1)
(5) lim_{x→1} (x³ - 1)/(x - 1)
(6) lim_{x→∞} (3x³ + x - 1)/(2x⁴ + x)
(7) lim_{x→∞} (9ˣ + 6ˣ)/(9ˣ - 6ˣ)
(8) lim_{x→1} sin(x-1)/[sin 5(x-1)]
(9) lim_{n→∞} n sin(3/(n+1))
(10) lim_{x→∞} (1 - x/(x²-1))^{x+1}
(11) lim_{n→∞} (1 + (n+1)/(n²+3))^{2n}
(12) lim_{x→0} (⁵√(1+4x³) - 1)/[sin² x ln(1+x)]
(13) lim_{x→0} [tan⁴ x (1-cos x)(1+x)] / [3(e^{x³}-1) arcsin x³]

f(x) = { 3x² - x + 1, x < 0; 4ˣ, x ≥ 0 }. Find lim_{x→0⁻} f(x), lim_{x→0⁺} f(x), and determine if lim_{x→0} f(x) exists.

f(x) = { 2x - 3a, x < 1; 1, x = 1; 3x - 4b, x > 1 }. If f continuous at x₀=1, find a and b.


Chapter 15: Derivatives

15.1 The Concept of the Derivative

1. Introductory Examples

Intro Example 1: An object moves along a straight line. Its displacement is given by s = s(t). Find the instantaneous velocity at time t0.
The displacement from time t0 to time t is s(t) - s(t0). The average velocity over this time interval is (s(t) - s(t0)) / (t - t0).
If the limit as t approaches t0 exists, we call it the instantaneous velocity v:
v = lim_{t→t0} (s(t) - s(t0)) / (t - t0).

Intro Example 2: Let y = f(x) be a smooth curve. Let M0(x0, f(x0)) be a point on the curve. Find the slope of the tangent line at M0.
Take another point M(x, f(x)) on the curve near M0. The slope of the secant line M0M is (f(x) - f(x0)) / (x - x0).
As M approaches M0 along the curve, if the secant line approaches a fixed line M0T, then M0T is the tangent line.
If the limit exists, the slope k of the tangent is:
k = lim_{x→x0} (f(x) - f(x0)) / (x - x0).

2. Definition of the Derivative

Definition 1: Let the function y = f(x) be defined in some neighborhood of x0. If the limit
lim_{x→x0} (f(x) - f(x0)) / (x - x0)
exists, then we say f(x) is differentiable at x0, and this limit value is called the derivative of f at x0. It is denoted as f'(x0), y'|{x=x0}, or dy/dx|{x=x0}. If the limit does not exist, f is not differentiable at x0.

Note 1:
(1) Geometric meaning: In Example 2, the slope of the tangent is f'(x0). In Example 1, the instantaneous velocity is s'(t0).
(2) f'(x0) = lim_{x→x0} (f(x) - f(x0)) / (x - x0).
(3) Let Δx = x - x0, then x = x0 + Δx. So:
f'(x0) = lim_{Δx→0} (f(x0 + Δx) - f(x0)) / Δx.
(4) The derivative function: f'(x) = lim_{Δx→0} (f(x + Δx) - f(x)) / Δx.

Example 1: Find the tangent line to the curve f(x) = x² at the point (3, 9).
Solution: The slope k = f'(3) = lim_{Δx→0} (f(3 + Δx) - f(3)) / Δx
= lim_{Δx→0} ((3 + Δx)² - 9) / Δx
= lim_{Δx→0} (9 + 6Δx + (Δx)² - 9) / Δx
= lim_{Δx→0} (6Δx + (Δx)²) / Δx
= lim_{Δx→0} (6 + Δx) = 6.
Using point-slope form: y - 9 = 6(x - 3), so the tangent line is y = 6x - 9.

Example 2: Find the derivative of f(x) = sin x.
Solution: f'(x) = lim_{Δx→0} (sin(x + Δx) - sin x) / Δx
= lim_{Δx→0} (2 cos((x+Δx + x)/2) sin((x+Δx - x)/2)) / Δx
= lim_{Δx→0} (2 cos(x + Δx/2) sin(Δx/2)) / Δx
= lim_{Δx→0} cos(x + Δx/2) * (sin(Δx/2)) / (Δx/2)
= lim_{Δx→0} cos(x + Δx/2) * lim_{Δx→0} (sin(Δx/2)) / (Δx/2)
= cos x * 1 = cos x.

Note 2: Basic derivative formulas:
(1) (C)' = 0, where C is a constant.
(2) (xᵃ)' = a xᵃ⁻¹.
(3) (aˣ)' = aˣ ln a, and especially (eˣ)' = eˣ.
(4) (logₐ x)' = 1 / (x ln a), and especially (ln x)' = 1/x.
(5) (sin x)' = cos x, (cos x)' = -sin x.
(6) (arcsin x)' = 1 / √(1 - x²), (arccos x)' = -1 / √(1 - x²),
(arctan x)' = 1 / (1 + x²), (arccot x)' = -1 / (1 + x²).

3. Left-hand and Right-hand Derivatives

Definition 2: Let f(x) be defined in some left neighborhood of x0. If the limit
lim_{x→x0⁻} (f(x) - f(x0)) / (x - x0)
exists, we say the left-hand derivative exists at x0, and this limit is called the left-hand derivative, denoted f'₋(x0).

Definition 3: Let f(x) be defined in some right neighborhood of x0. If the limit
lim_{x→x0⁺} (f(x) - f(x0)) / (x - x0)
exists, we say the right-hand derivative exists at x0, and this limit is called the right-hand derivative, denoted f'₊(x0).

Note 3:
(1) f'₋(x0) = lim_{x→x0⁻} (f(x) - f(x0)) / (x - x0) = lim_{Δx→0⁻} (f(x0 + Δx) - f(x0)) / Δx.
(2) f'₊(x0) = lim_{x→x0⁺} (f(x) - f(x0)) / (x - x0) = lim_{Δx→0⁺} (f(x0 + Δx) - f(x0)) / Δx.

Theorem: f(x) is differentiable at x0 if and only if both the left-hand derivative f'₋(x0) and the right-hand derivative f'₊(x0) exist and are equal. In that case, f'(x0) = f'₋(x0) = f'₊(x0).

Example 3: Let f(x) = { x³, if x ≤ 0; x², if x > 0 }.
(1) Find f'₋(0) and f'₊(0).
(2) Is f(x) differentiable at x0 = 0?
Solution:
(1) f'₋(0) = lim_{x→0⁻} (f(x) - f(0)) / (x - 0) = lim_{x→0⁻} (x³ - 0) / x = lim_{x→0⁻} x² = 0.
f'₊(0) = lim_{x→0⁺} (f(x) - f(0)) / (x - 0) = lim_{x→0⁺} (x² - 0) / x = lim_{x→0⁺} x = 0.
(2) Since f'₋(0) = f'₊(0) = 0, f(x) is differentiable at 0, and f'(0) = 0.

Example 4: Let f(x) = { x³, if x ≤ 0; sin x, if x > 0 }.
(1) Find f'₋(0) and f'₊(0).
(2) Is f(x) differentiable at x0 = 0?
Solution:
(1) f'₋(0) = lim_{x→0⁻} (f(x) - f(0)) / (x - 0) = lim_{x→0⁻} (x³ - 0) / x = lim_{x→0⁻} x² = 0.
f'₊(0) = lim_{x→0⁺} (f(x) - f(0)) / (x - 0) = lim_{x→0⁺} (sin x - 0) / x = lim_{x→0⁺} (sin x)/x = 1.
(2) Since f'₋(0) ≠ f'₊(0) (0 ≠ 1), f(x) is not differentiable at 0.

Summary

The derivative f'(x0) is defined as the limit of the difference quotient: lim_{x→x0} (f(x) - f(x0))/(x - x0), if it exists.

Geometric meaning: f'(x0) is the slope of the tangent line to the curve y = f(x) at the point (x0, f(x0)).

Basic derivative formulas for elementary functions.

A function is differentiable at a point if and only if its left-hand and right-hand derivatives at that point exist and are equal.

The geometric meaning of the derivative: the instantaneous speed of an object in straight-line motion with changing speed, the slope of the tangent line to a curve at a point.

(1) f'(x0) = lim (x -> x0) [f(x) - f(x0)]/(x - x0) = lim (Δx -> 0) [f(x0 + Δx) - f(x0)]/Δx.

(2) y' = dy/dx = f'(x) = lim (Δx -> 0) [f(x + Δx) - f(x)]/Δx — the derivative function.

(3) Left derivative: f'(x0) = lim (x -> x0-) [f(x) - f(x0)]/(x - x0) = lim (Δx -> 0-) [f(x0 + Δx) - f(x0)]/Δx.

Right derivative: f'(x0) = lim (x -> x0+) [f(x) - f(x0)]/(x - x0) = lim (Δx -> 0+) [f(x0 + Δx) - f(x0)]/Δx.

The function f(x) is differentiable at x0 if and only if the left and right derivatives at x0 both exist and are equal.

Derivative formulas for basic elementary functions:

(1) (C)' = 0;

(2) (x^a)' = a x^(a-1);

(3) (a^x)' = a^x ln a, in particular, (e^x)' = e^x;

(4) (log_a x)' = 1/(x ln a), in particular, (ln x)' = 1/x;

(5) (sin x)' = cos x, (cos x)' = -sin x;

(6) (arcsin x)' = 1/sqrt(1-x^2), (arccos x)' = -1/sqrt(1-x^2), (arctan x)' = 1/(1+x^2),

(arccot x)' = 1/(1+x^2)

Exercise 15.1

Find the derivative f'(x0) for each of the following f(x) at x0.

(1) f(x) = x, x0 = 2

(2) f(x) = x^3, x0 = 1

(3) f(x) = cos 3x, x0 = 0

(4) f(x) = sin 2x^3, x0 = 0

Find the tangent line to the curve y = f(x) at M0(x0, f(x0)) for the following.

(1) f(x) = 4x^2, M0(1, 2)

(2) f(x) = 5^x, M0(0, 1)

Let f(x) =
2x, for x <= 0,
x^4, for x > 0,

(1) Find f'-(0) and f'+(0);

(2) Is f(x) differentiable at x0=0?

Let f(x) =
3x, for x <= 1,
4, for x > 1,

(1) Find f'-(0) and f'+(0);

(2) Is f(x) differentiable at x0=0?

Let f(x) =
arcsin 3x, for x <= 0,
sin 3x, for x > 0,

(1) Find f'-(0) and f'+(0);

(2) Is f(x) differentiable at x0=0?

Let f(x) =
8^x, for x <= 0,
8x, for x > 0,

(1) Find f'-(0) and f'+(0);

(2) Is f(x) differentiable at x0=0?

15.2 Derivative Rules for Sum, Difference, Product, Quotient, and Chain Rule

I. Derivative Rules for Sum, Difference, Product, and Quotient

Theorem 1 Let f(x) and g(x) be differentiable at x. Then

(1) [f(x) ± g(x)]' = f'(x) ± g'(x).

(2) [f(x)g(x)]' = f'(x)g(x) + f(x)g'(x);

In particular, [C * f(x)]' = C * f'(x) (C is a constant).

(3) When g(x) ≠ 0, [f(x)/g(x)]' = [f'(x)g(x) - f(x)g'(x)] / g^2(x);

In particular, [1/g(x)]' = -g'(x)/g^2(x).

Example 1 Find the derivative of the following functions.

(1) y = 2x^3 - 3x + 1 (2) y = x^2 sin x

(3) y = tan x (4) y = sec x

Solution (1) y' = (2x^3 - 3x + 1)' = (2x^3)' - (3x)' + (1)' = 2 * 3x^2 - 3 = 6x^2 - 3.

(2) y' = (x^2 sin x)' = (x^2)' sin x + x^2 (sin x)' = 2x sin x + x^2 cos x.

(3) y' = (tan x)' = (sin x / cos x)' = [(sin x)' cos x - sin x (cos x)'] / cos^2 x = (cos^2 x + sin^2 x) / cos^2 x = 1 / cos^2 x = sec^2 x.

(4) y' = (sec x)' = (1/cos x)' = -(cos x)' / cos^2 x = sin x / cos^2 x = (sin x / cos x) * (1 / cos x) = tan x sec x.

II. Chain Rule

Definition Let y = f(u), u = φ(x). Then y = f(φ(x)) is called a composite function.

Theorem 2 Let u = φ(x) be differentiable at point x, and y = f(u) be differentiable at the corresponding point u. Then y = f(φ(x)) is differentiable at point x, and y' = (dy/du) * (du/dx) = f'(u) u' = f'(φ(x)) φ'(x).

Example 2 Find the derivative of the following functions.

(1) y = cos x^3

(2) y = sin^2 x

(3) y = (2x^2 + 1)^{10}

(4) y = e^{tan x + x}

(5) y = ln(sin^2 x + 3)

Solution (1) Let u = x^3, then y = cos u, so
y' = (cos u)' = (-sin u) u' = -sin(x^3) * (x^3)' = -3x^2 sin(x^3).

(2) Let u = sin x, then y = u^2, so
y' = (u^2)' = 2u u' = 2 sin x (sin x)' = 2 sin x cos x.

(3) Let u = 2x^2 + 1, then y = u^{10}, so
y' = (u^{10})' = 10 u^9 u' = 10(2x^2 + 1)^9 * (2x^2 + 1)' = 10(2x^2 + 1)^9 * 4x = 40x(2x^2 + 1)^9.

(4) Let u = tan x + x, then y = e^u, so
y' = (e^u)' = e^u u' = e^{tan x + x} (tan x + x)' = e^{tan x + x} (sec^2 x + 1).

(5) Let u = sin^2 x + 3, then y = ln u, so
y' = (ln u)' = (1/u) u' = [1/(sin^2 x + 3)] * (sin^2 x + 3)' = (2 sin x cos x) / (sin^2 x + 3).

Summary

Derivative rules for sum, difference, product, and quotient:
Let f(x) and g(x) be differentiable at point x. Then
(1) [f(x) ± g(x)]' = f'(x) ± g'(x).
(2) [f(x)g(x)]' = f'(x)g(x) + f(x)g'(x);
In particular, [C * f(x)]' = C * f'(x) (C is a constant).
(3) When g(x) ≠ 0,
[f(x)/g(x)]' = [f'(x)g(x) - f(x)g'(x)] / g^2(x);
In particular,
[1/g(x)]' = -g'(x)/g^2(x).

Chain rule:
Let u = φ(x) be differentiable at point x, and y = f(u) be differentiable at the corresponding point u. Then y = f(φ(x)) is differentiable at point x, and y' = (dy/du) * (du/dx) = f'(u) u' = f'(φ(x)) φ'(x).

Exercise 15.2

Find the derivative of the following functions.
(1) y = 2x^{10} + 3x^5 - 9 (2) y = sin x + 3 cos x - 1
(3) y = 2e^x cos x (4) y = 3 sin x * log_3 x
(5) y = cot x (6) y = csc x
(7) y = (x sin x - 1) / (2x^2) (8) y = (x^2 * 2^x) / (3 ln x)

Find the derivative of the following functions.
(1) y = sin(5x + 1) (2) y = tan^4 x
(3) y = sin(2/x) (4) y = (3x - 1)^{30}
(5) y = cos^5(2x - 1) (6) y = x cos(1/x)
(7) y = arctan(x^2 - 2) (8) y = arcsin(ln x + 3)

15.3 Higher-Order Derivatives

Definition 1 If the derivative f'(x) of a function f(x) is itself differentiable at x0, i.e., the limit
lim (x -> x0) [f'(x) - f'(x0)]/(x - x0)
exists, then this limit value is called the second derivative of f(x) at x0, denoted as f''(x0), y''|{x=x0}, or (d^2 y)/(dx^2)|{x=x0}. That is,
f''(x0) = lim (x -> x0) [f'(x) - f'(x0)]/(x - x0).

Note 1 (1) f''(x) is called the second derivative of f(x), f''(x) = (f'(x))';
(2) f'''(x) is called the third derivative of f(x), f'''(x) = (f''(x))';
(3) f^{(4)}(x) is called the fourth derivative of f(x), f^{(4)}(x) = (f'''(x))';
(4) f^{(n)}(x) is called the n-th derivative of f(x), f^{(n)}(x) = (f^{(n-1)}(x))' (n ≥ 5).
The derivatives of y = f(x) are denoted as y', y'', y''', y^{(4)}, y^{(5)}, ..., y^{(n)}, ... or
f'(x), f''(x), f'''(x), f^{(4)}(x), f^{(5)}(x), ..., f^{(n)}(x), ... or dy/dx, d^ny/dx^n (n=2, 3, ...), or df(x)/dx, d^nf(x)/dx^n (n=2, 3, ...).

Definition 2 Derivatives of the second order and above are all called higher-order derivatives.

Example 1 Let y = 2x^3 + 4x^2 - 5x - 3, find y^{(4)}.

Solution y' = (2x^3 + 4x^2 - 5x - 3)' = 6x^2 + 8x - 5,
y'' = (y')' = (6x^2 + 8x - 5)' = 12x + 8,
y''' = (y'')' = (12x + 8)' = 12,
y^{(4)} = (y''')' = (12)' = 0.

Note 2 The (n+1)th and higher derivatives of an n-th degree polynomial y = a_n x^n + a_{n-1} x^{n-1} + ... + a_1 x + a_0 are 0.

Example 2 Let y = sin x, find y^{(4)}.

Solution y' = (sin x)' = cos x,
y'' = (y')' = (cos x)' = -sin x,
y''' = (y'')' = (-sin x)' = -cos x,
y^{(4)} = (y''')' = (-cos x)' = -(-sin x) = sin x.

Example 3 Let y = ln(1+x), find y^{(n)}.

Solution y' = [ln(1+x)]' = 1/(1+x),
y'' = (1/(1+x))' = [(1+x)^{-1}]' = (-1)(1+x)^{-2},
y''' = [(-1)(1+x)^{-2}]' = (-1)(-2)(1+x)^{-3},
y^{(4)} = [(-1)(-2)(1+x)^{-3}]' = (-1)(-2)(-3)(1+x)^{-4},
...
y^{(n)} = (-1)(-2)(-3)...(-(n-1)) (1+x)^{-n} = (-1)^{n-1} (n-1)! (1+x)^{-n}.

Summary

Higher-order derivatives: derivatives of the second order and above.

(1) f''(x0) = lim (x -> x0) [f'(x) - f'(x0)]/(x - x0);
(2) f^{(n)}(x0) = lim (x -> x0) [f^{(n-1)}(x) - f^{(n-1)}(x0)]/(x - x0);
(3) f^{(n)}(x) = lim (Δx -> 0) [f^{(n-1)}(x + Δx) - f^{(n-1)}(x)]/Δx.

Exercise 15.3

Find the second derivative of the following functions.
(1) y = x^3 + 2x^5 (2) y = 3 sin x + 2 ln x
(3) y = x^2 cos x (4) y = (1 - 2x)/(1 + 2x)
(5) y = tan 2x (6) y = (1 + 2x^2) arctan x

Find the third derivative of the following functions.
(1) y = 3x^4 - 5x + 3 (2) y = x^3 ln x
(3) y = ln(1+x) (4) y = x^5 e^x

Find the n-th derivative of the following functions.
(1) y = (x+2)^7 (2) y = x^α (α is a constant)
(3) y = e^{6x} (4) y = 1/(x+3)

Chapter 15 Derivative

15.4 Derivatives of Implicit Functions

A function expressed in the form y = f(x) is called an explicit function. A function expressed in the form F(x, y) = 0 is called an implicit function.

Definition If for every x in an interval I, there exists a unique y such that F(x, y) = 0, then the equation F(x, y) = 0 defines an implicit function on the interval I.

For example, y = 2x^2 + 3x sin x - 5 is an explicit function, and x + y^3 - 1 = 0 defines an implicit function.

Question: If F(x, y) = 0 defines an implicit function y = y(x), then what is y'?

Answer: Differentiate both sides of the equation F(x, y) = 0 with respect to x, treating y as a function of x during differentiation, then solve for y'.

Example 1 Given that y = y(x) is defined implicitly by the equation e^y - x y + 1 = 0, find dy/dx.

Solution Differentiate both sides with respect to x: d/dx (e^y - x y + 1) = d/dx(0). This gives:
e^y (dy/dx) - y - x (dy/dx) = 0,
=> e^y (dy/dx) - x (dy/dx) = y,
=> (e^y - x) (dy/dx) = y,
=> dy/dx = y / (e^y - x).

Example 2 Given that y = y(x) is defined implicitly by the equation sin(x+y) + x = y, find dy/dx.

Solution Differentiate both sides with respect to x: d/dx [sin(x+y) + x] = dy/dx. This gives:
[1 + (dy/dx)] cos(x+y) + 1 = dy/dx,
=> (dy/dx) cos(x+y) - dy/dx = -1 - cos(x+y),
=> [cos(x+y) - 1] (dy/dx) = -1 - cos(x+y),
=> dy/dx = [1 + cos(x+y)] / [cos(x+y) - 1] = [1 + cos(x+y)] / [1 - cos(x+y)].

Example 3 Given that y = y(x) is defined implicitly by the equation y^3 - 3x^2 y + 2x - 1 = 0, find y' and y'|_{x=0}.

Solution Differentiate both sides with respect to x: (y^3 - 3x^2 y + 2x - 1)' = 0. This gives:
3y^2 y' - 6xy - 3x^2 y' + 2 = 0,
=> 3y^2 y' - 3x^2 y' = 6xy - 2,
=> 3(y^2 - x^2) y' = 6xy - 2,
=> y' = (6xy - 2) / [3(y^2 - x^2)].
From y^3 - 3x^2 y + 2x - 1 = 0, when x = 0, we get y = 1. So,
y'|_{x=0} = (6*0*1 - 2) / [3(1^2 - 0^2)] = -2/3.

Summary

Given F(x, y) = 0 defines an implicit function y = y(x), use the following method to find y':
(1) Differentiate both sides of the equation F(x, y) = 0 with respect to x, treating y as a function of x;
(2) Solve for y'.

Exercise 15.4

Given that y = y(x) is defined implicitly by ln y - 2y + 3x = 0, find dy/dx.

Given that y = y(x) is defined implicitly by e^x - x + 3y = 0, find dy/dx.

Given that y = y(x) is defined implicitly by sin y - 2x + y^2 = 5, find dy/dx.

Given that y = y(x) is defined implicitly by 5x^2 - 2xy + y^2 = 0, find dy/dx.

Given that y = y(x) is defined implicitly by x = cos(x - y), find y'.

Given that y = y(x) is defined implicitly by y = ln(x - y), find y'.

Given that y = y(x) is defined implicitly by (x^2)/3 - (y^2)/4 = 1, find dy/dx.

Given that y = y(x) is defined implicitly by 4xy + 5 = e^{x+y}, find dy/dx and dy/dx|_{x=0}.

Given that y = y(x) is defined implicitly by y^3 - 2xy^2 + 3x - 1 = 0, find y' and y'|_{x=0}.

15.5 Derivatives of Parametric Functions

Definition If the functional relationship between variables x and y is given by parametric equations { x = φ(t), y = ψ(t) }, we call this a parametric function.

Question: Let the parametric function y = y(x) be defined by { x = φ(t), y = ψ(t) }, where x = φ(t) is strictly monotonic and differentiable on some interval I, with φ'(t) ≠ 0, and y = ψ(t) is differentiable on I. Then what are dy/dx and d^2y/dx^2?

Answer:
dy/dx = (dy/dt) * (dt/dx) = (dy/dt) * (1/(dx/dt)) = (dy/dt) / (dx/dt) = ψ'(t) / φ'(t);
d^2y/dx^2 = d/dx (dy/dx) = d/dt (ψ'(t)/φ'(t)) * (dt/dx) = [ψ''(t) φ'(t) - ψ'(t) φ''(t)] / [φ'(t)]^2 * (1/φ'(t)) = [ψ''(t) φ'(t) - ψ'(t) φ''(t)] / [φ'(t)]^3.

Example 1 Given the function y = y(x) defined by parametric equations { x = 2 cos t, y = 3 sin t }, find dy/dx and d^2y/dx^2.

Solution
dy/dx = ψ'(t)/φ'(t) = (3 sin t)'/(2 cos t)' = (3 cos t)/(-2 sin t) = -(3/2) cot t;
d^2y/dx^2 = d/dx (dy/dx) = d/dx (-(3/2) cot t) = -(3/2) * d(cot t)/dt * dt/dx = -(3/2) * (-csc^2 t) * (1/(dx/dt)) = (3/2 csc^2 t) * (1/(-2 sin t)) = -3/(4 sin^3 t).

Example 2 Given the function y = y(x) defined by parametric equations { x = 2t - 3, y = t^2 + 5 }, find dy/dx and d^2y/dx^2.

Solution
dy/dx = ψ'(t)/φ'(t) = (t^2 + 5)'/(2t - 3)' = (2t)/(2) = t;
d^2y/dx^2 = d/dx (dy/dx) = d/dx (t) = dt/dx = (dt/dt) * (dt/dx) = 1 * (1/(dx/dt)) = 1/2.

Example 3 Given the function y = y(x) defined by parametric equations { x = ln(t+1), y = e^t }, find dy/dx|_{t=1}.

Solution
dy/dx = ψ'(t)/φ'(t) = (e^t)' / [ln(t+1)]' = e^t / [1/(t+1)] = (t+1) e^t,
so dy/dx|_{t=1} = (1+1) e^1 = 2e.

Summary

Let y = y(x) be a parametric function defined by { x = φ(t), y = ψ(t) }, where x = φ(t) is strictly monotonic and differentiable on some interval I, with φ'(t) ≠ 0, and y = ψ(t) is differentiable on I. Then
(1) dy/dx = ψ'(t) / φ'(t);
(2) d^2y/dx^2 = [ψ''(t) φ'(t) - ψ'(t) φ''(t)] / [φ'(t)]^3.

Exercise 15.5

Given the function y = y(x) defined by parametric equations { x = t^3, y = t^6 }, find dy/dx.

Given the function y = y(x) defined by parametric equations { x = sin t + 1, y = cos 3t }, find dy/dx.

Given the function y = y(x) defined by parametric equations { x = 4t^5, y = 2 - t }, find dy/dx and d^2y/dx^2.

Given the function y = y(x) defined by parametric equations { x = 4/t, y = ln t }, find dy/dx and d^2y/dx^2.

Given the function y = y(x) defined by parametric equations { x = arctan t, y = 3t^2 + 1 }, find dy/dx.

Given the function y = y(x) defined by parametric equations { x = 5t^3 - 4t + 1, y = 3t^2 - 2t - 5 }, find dy/dx|_{t=0}.

Given the function y = y(x) defined by parametric equations { x = 2t + sin t, y = 1 - cos t }, find dy/dx|_{t=π/2}.

Given the function y = y(x) defined by parametric equations { x = 5e^t + 1, y = e^{-t} - 3 }, find dy/dx|_{t=1}.

Self-Test 15

Let f(x) =
x^3 - 5x^2, for x ≤ 0,
3x^2, for x > 0,
(1) Find f'-(0) and f'+(0);
(2) Is f(x) differentiable at x0 = 0?

Let f(x) =
x^2 sin(3/x), for x ≠ 0,
0, for x = 0,
find f'(0).

Find the derivative of the following functions.
(1) y = (x^3 + 1) tan x
(2) y = (e^x sin 2x) / x
(3) y = arctan(4x) * cos^2(2x - 1)

Let y = x^5 ln x, find y^{(4)}.

Let y = 1/(2x+1), find y^{(n)}.

Given that y = y(x) is defined implicitly by 4xy + 1 = e^{x-y} - 2x, find dy/dx and dy/dx|_{x=0}.

Given the function y = y(x) defined by parametric equations { x = 3t^2 - 1, y = 1 - 2t }, find dy/dx|_{t=1}.

Given the function y = y(x) defined by parametric equations { x = t^5, y = tan 3t }, find dy/dx and d^2y/dx^2.


Chapter 16 Applications of the Derivative

16.1 Derivatives and the Monotonicity of Functions

We have already introduced the concept of monotonic functions in Section 3.3. Now we will study the monotonicity of functions using derivatives.

Theorem Let the function y = f(x) be continuous on the interval [a, b] and differentiable on (a, b).
(1) If f'(x) > 0 for all x in (a, b), then f(x) is strictly increasing on [a, b]. [a, b] is a strictly increasing interval, and also an increasing interval.
(2) If f'(x) < 0 for all x in (a, b), then f(x) is strictly decreasing on [a, b]. [a, b] is a strictly decreasing interval, and also a decreasing interval.

From the theorem above, we see that points where the derivative equals zero may be the boundaries between increasing and decreasing intervals.

Example 1 Find the monotonic intervals of the function y = |x|.

Solution From the definition of the derivative, the function is not differentiable at x = 0. From the graph of the function, the increasing interval of y = |x| is [0, +∞), and the decreasing interval is (-∞, 0].

From Example 1, we see that points where the derivative does not exist may also be boundaries between increasing and decreasing intervals.

Note Steps to find the monotonic intervals of y = f(x) using the derivative:
(1) Write down the domain of y = f(x).
(2) Find the derivative y' of y = f(x).
(3) Find points x0 where y' = 0 and points where y' does not exist. Use these x0 points to divide the domain into several intervals, and make a table.
(4) Determine the sign of y' on each resulting interval. If y' > 0, the corresponding interval is an increasing interval; if y' < 0, it is a decreasing interval.

Example 2 Find the monotonic intervals of the function y = 2x^2 - 4x + 1.

Solution The domain of the function is (-∞, +∞).
y' = 4x - 4 = 4(x - 1).
Setting y' = 0 gives x = 1.

Use x = 1 to divide the domain (-∞, +∞) into two intervals: (-∞, 1) and (1, +∞). Make a table:

x | (-∞, 1) | 1 | (1, +∞)
y' | - | 0 | +
y | decreasing | | increasing

So, the increasing interval of y = 2x^2 - 4x + 1 is [1, +∞), and the decreasing interval is (-∞, 1].

Example 3 Find the monotonic intervals of the function y = 4x^3 - 18x^2 + 24x - 5.

Solution The domain of the function is (-∞, +∞).
y' = 12x^2 - 36x + 24 = 12(x^2 - 3x + 2) = 12(x - 1)(x - 2).
Setting y' = 0 gives x1 = 1, x2 = 2.

Use x1 = 1, x2 = 2 to divide the domain into three intervals: (-∞, 1), (1, 2), (2, +∞). Make a table:

x | (-∞, 1) | 1 | (1, 2) | 2 | (2, +∞)
y' | + | 0 | - | 0 | +
y | increasing | | decreasing | | increasing

So, the increasing intervals of y = 4x^3 - 18x^2 + 24x - 5 are (-∞, 1] and [2, +∞). The decreasing interval is [1, 2].

Example 4 Find the monotonic intervals of the function y = x e^x.

Solution The domain of the function is (-∞, +∞).
y' = e^x + x e^x = (x + 1) e^x.
Setting y' = 0 gives x = -1 (since e^x is always positive).

Use x = -1 to divide the domain into two intervals: (-∞, -1) and (-1, +∞). Make a table:

x | (-∞, -1) | -1 | (-1, +∞)
y' | - | 0 | +
y | decreasing | | increasing

So, the increasing interval of y = x e^x is [-1, +∞), and the decreasing interval is (-∞, -1].

Example 5 Find the monotonic intervals of the function y = cube root of (x^2). (Note: The text says y = 3/x^2, but from context and solution, it's y = x^(2/3)).
Solution The domain of the function is (-∞, +∞).
y' = (2/3) x^{-1/3} = 2/(3 * cube root of x).
When x = 0, y' does not exist. Use x = 0 to divide the domain into two intervals: (-∞, 0) and (0, +∞). Make a table:

x | (-∞, 0) | 0 | (0, +∞)
y' | - | does not exist | +
y | decreasing | | increasing

So, the increasing interval of y = cube root of (x^2) is [0, +∞), and the decreasing interval is (-∞, 0].

Summary

Let the function y = f(x) be continuous on [a, b] and differentiable on (a, b).
(1) If f'(x) > 0 on (a, b), then f(x) is strictly increasing on [a, b], and [a, b] is a strictly increasing interval.
(2) If f'(x) < 0 on (a, b), then f(x) is strictly decreasing on [a, b], and [a, b] is a strictly decreasing interval.

Steps to find monotonic intervals using the derivative:
(1) Write down the domain of y = f(x).
(2) Find the derivative y'.
(3) Find points x0 where y' = 0 and points where y' does not exist. Use x0 to divide the domain into intervals and make a table.
(4) Determine the sign of y' on each interval. If y' > 0, it's an increasing interval; if y' < 0, it's a decreasing interval.

Exercise 16.1

Find the monotonic intervals of the following functions.
(1) y = x^2 - 6x + 3 (2) y = x^2 - 2x + 5
(3) y = x^2 + 8x (4) y = 3x^2 + 12x + 1
(5) y = 2x^3 - 9x^2 - 24x + 1 (6) y = 2x^3 - 15x^2 + 36x - 4
(7) y = x * cube root of [(x+1)^2] (Note: text says y = x∛(x+1)^2) (8) y = x + 1/x

Discuss the monotonicity of the function f(x) = a ln x + x, where a is a real constant.

16.2 Derivatives and Extreme Values (Maxima/Minima) of Functions

I. Derivatives and Extreme Values

Definition 1 Let the function f(x) be defined in some neighborhood U(x0) of point x0. If for every point x in the punctured neighborhood U°(x0), we have f(x) < f(x0) (or f(x) > f(x0)), then f(x0) is called a local maximum (or local minimum) of f(x), and x0 is called a local maximum point (or local minimum point).

Local maxima and minima are collectively called extreme values. Local maximum and minimum points are collectively called extreme points.

In Figure 16.2-1, x1 and x3 are local maximum points of f(x), and f(x1) and f(x3) are local maxima; x2 and x4 are local minimum points, and f(x2) and f(x4) are local minima.

Definition 2 If f'(x0) = 0, then x0 is called a stationary point of the function f(x).

Theorem 1 (Necessary Condition) Let f(x) be differentiable at point x0. If x0 is an extreme point of f(x), then f'(x0) = 0.

Note 1 Let f(x) be differentiable at x0. From Theorem 1, an extreme point must be a stationary point, but a stationary point is not necessarily an extreme point.
For example, let f(x) = x^3. Then f'(0) = 0, so x = 0 is a stationary point of f(x) = x^3, but x = 0 is not an extreme point.

Theorem 2 (Sufficient Condition) Let f(x) be continuous at point x0 and differentiable in some punctured neighborhood U(x0, δ) (δ is a positive constant).
(1) If f'(x) > 0 when x ∈ (x0 - δ, x0), and f'(x) < 0 when x ∈ (x0, x0 + δ), then x0 is a local maximum point of f(x).
(2) If f'(x) < 0 when x ∈ (x0 - δ, x0), and f'(x) > 0 when x ∈ (x0, x0 + δ), then x0 is a local minimum point of f(x).
(3) If the sign of f'(x) does not change in U(x0, δ), then x0 is not an extreme point of f(x).

Note 2 Steps to find extreme values of y = f(x) using the derivative:
(1) Write down the domain of y = f(x).
(2) Find the derivative y'.
(3) Find points where y' does not exist and stationary points x0. Use x0 to divide the domain into intervals and make a table.
(4) Determine the sign of y' on each interval. If y' > 0, f(x) is increasing; if y' < 0, f(x) is decreasing.
(5) According to Theorem 2, determine if x0 is an extreme point; if so, determine if it is a local maximum or minimum point.

Example 1 Find the extreme points and extreme values of f(x) = 2x^3 - 3x^2 - 12x + 3.

Solution The domain is (-∞, +∞).
f'(x) = 6x^2 - 6x - 36 = 6(x^2 - x - 6) = 6(x + 2)(x - 3).
Setting f'(x) = 0 gives x1 = -2, x2 = 3.

Use x1 = -2, x2 = 3 to divide the domain into three intervals: (-∞, -2), (-2, 3), (3, +∞). Make a table:

x | (-∞, -2) | -2 | (-2, 3) | 3 | (3, +∞)
f'(x) | + | 0 | - | 0 | +
f(x) | increasing | local max | decreasing | local min | increasing

So, the local maximum point is -2, with local maximum value f(-2) = -1. The local minimum point is 3, with local minimum value f(3) = -6.

Example 2 Find the extreme values of f(x) = sin x + cos x, x ∈ (0, 2π).

Solution The domain is (0, 2π).
f'(x) = cos x - sin x.
Setting f'(x) = 0 gives x1 = π/4, x2 = 5π/4.

Use these points to divide (0, 2π) into intervals: (0, π/4), (π/4, 5π/4), (5π/4, 2π). Make a table:

x | (0, π/4) | π/4 | (π/4, 5π/4) | 5π/4 | (5π/4, 2π)
f'(x) | + | 0 | - | 0 | +
f(x) | increasing | local max | decreasing | local min | increasing

So, the local maximum value is f(π/4) = √2, and the local minimum value is f(5π/4) = -√2.

Example 3 Find the extreme values of f(x) = (x+1) * cube root of (x-1). (Note: text has f(x) = (x+1)^(3/2) * (x-1), but solution implies f(x) = (x+1) * (x-1)^(1/3)).
Solution The domain is (-∞, +∞).
f'(x) = (x-1)^(1/3) + (x+1) * (1/3)(x-1)^(-2/3) = (2/3)(x-1)^(-2/3) (2x - 1).
When x1 = 1, f'(x) does not exist.
Setting f'(x) = 0 gives x2 = 1/2.

Use x1=1 and x2=1/2 to divide the domain: (-∞, 1/2), (1/2, 1), (1, +∞). Make a table:

x | (-∞, 1/2) | 1/2 | (1/2, 1) | 1 | (1, +∞)
f'(x) | - | 0 | + | does not exist | +
f(x) | decreasing | local min | increasing | | increasing

So, the function has a local minimum value f(1/2) = (3 * cube root of 4) / 4. (Note: The text result is 3∛4 / 4).

II. Derivatives and Absolute Maxima/Minima (Global Extrema)

Let the function y = f(x) be continuous on [a, b]. Then f(x) must have an absolute maximum and an absolute minimum on [a, b].

Steps to find absolute extrema of y = f(x) on [a, b] using the derivative:
(1) Find the derivative y'.
(2) Find points where y' does not exist and stationary points x0.
(3) Calculate the values f(x0), f(a), f(b). Compare them. The largest is the absolute maximum, and the corresponding point is the maximum point. The smallest is the absolute minimum, and the corresponding point is the minimum point.

Example 4 Find the absolute maximum and minimum and their points for f(x) = 2x^3 + 3x^2 - 2 on the interval [-2, 1].

Solution f'(x) = 6x^2 + 6x = 6x(x+1).
Setting f'(x) = 0 gives x1 = -1, x2 = 0.
Compute: f(-1) = -1, f(0) = -2, f(-2) = -6, f(1) = 3.
So, on [-2, 1], the absolute maximum value is 3 at x = 1; the absolute minimum value is -6 at x = -2.

Example 5 Find the absolute maximum and minimum of f(x) = x^2 - 2|x| on the interval [-2, 3].

Solution f(x) = {
x^2 - 2x, for x ≥ 0,
x^2 + 2x, for x < 0.
}
From the definition of the derivative, f(x) is not differentiable at x1 = 0.
Setting f'(x) = 0 gives: For x ≥ 0, derivative is 2x - 2 = 0 => x2 = 1. For x < 0, derivative is 2x + 2 = 0 => x3 = -1.
Compute: f(0) = 0, f(1) = -1, f(-1) = -1, f(-2) = 0, f(3) = 3.
So, the absolute maximum value on [-2, 3] is 3, and the absolute minimum value is -1.

Summary

Definition of local extreme: Let f(x) be defined in a neighborhood U(x0) of x0. If for all x in the punctured neighborhood U°(x0), f(x) < f(x0) (or f(x) > f(x0)), then f(x0) is a local maximum (or local minimum), and x0 is a local maximum point (or local minimum point).

Stationary point: If f'(x0) = 0, then x0 is a stationary point.

Master the necessary and sufficient conditions for local extrema.

Steps to find local extrema using derivatives (as in Note 2 above).

Steps to find absolute extrema on a closed interval [a, b] using derivatives (as in section II above).

Exercise 16.2

Find the local extreme points and values of the following functions.
(1) f(x) = x^2 - 5x - 14 (2) f(x) = x^2 - 4
(3) f(x) = 2x^3 + 3x^2 - 120x + 1 (4) f(x) = x^3 - 3x - 2
(5) f(x) = x^3 - 6x^2 + 9x + 4 (6) f(x) = (x - 2)^(3/?) (Note: text incomplete, likely (x-2)^(3/2) or similar)

Find the absolute maximum and minimum points and values of the following functions on the given intervals.
(1) f(x) = x^3 + 6x^2 - 1, x ∈ [-5, 1]
(2) f(x) = 3x^4 - 4x^3 + 1, x ∈ [-2, 2]
(3) f(x) = e^x - x + 3, x ∈ [-1, 1]
(4) f(x) = x - 2 cos x, x ∈ [0, 2π]

The function f(x) = x^3 - 3a x^2 + 2b x has a local minimum of -1 at x = 1. Find the values of a, b and the monotonic intervals of f(x).

It is known that x = 1 and x = 2 are two extreme points of the function f(x) = a ln x + b x^2 + x.
(1) Find the values of a and b.
(2) Determine whether x = 1 and x = 2 are local maximum or minimum points, and explain why.

Self-Test 16

Find the monotonic intervals of the following functions.
(1) y = x^2 + 4x + 1 (2) y = 2x^3 + 9x^2 - 60x - 5
(3) y = (2x - 1) e^x (4) y = x^2 + 2/x
(5) y = (3/4) ln x + sqrt(x+1) (for x > 0)

Find the local extreme points and values of the following functions.
(1) f(x) = x^3 - 6x^2 - 36x + 1 (2) y = (x^2 - 3) e^x
(3) y = (x + 5) * cube root of (x^2)

Find the absolute maximum and minimum points and values of the following functions on the given intervals.
(1) f(x) = x^3 + 3x^2 - 5, x ∈ [-3, 3]
(2) f(x) = x + 2 sin x, x ∈ [0, 2π]

It is known that x = -2 is an extreme point of the function f(x) = (x^2 + a x - 1) e^x. Find
(1) the value of a;
(2) the local minimum value of f(x).

For the function f(x) = 2x^3 - a x^2 + b,
(1) Discuss the monotonicity of f(x).
(2) Do there exist values of a, b such that f(x) has an absolute minimum of -1 and an absolute maximum of 1 on the interval [0, 1]? If so, find all such values of a, b; if not, explain why.


Chapter 17 Permutations and Combinations

17.1 The Addition Principle and Multiplication Principle

I. Addition Principle
If you can do a task in n different ways, and there are m1 ways for the first method, m2 ways for the second method, ..., mn ways for the nth method, then the total number of ways to do the task is N = m1 + m2 + ... + mn.

Example 1: From town A to town B, there are 4 trains, 8 buses, and 5 flights per day. How many different travel options in one day?
Answer: N = 4 + 8 + 5 = 17.

II. Multiplication Principle
If a task can be done in n steps, and step 1 has m1 ways, step 2 has m2 ways, ..., step n has mn ways, then the total number of ways is N = m1 x m2 x ... x mn.

Example 2: There are 3 roads from dorm to cafeteria, and 4 roads from cafeteria to classroom. How many routes from dorm to classroom via cafeteria?
Answer: N = 3 x 4 = 12.

Example 3: How many 3-digit numbers with no repeated digits can be made from 1,2,3,4?
Answer: Step 1: choose hundreds digit (4 choices). Step 2: choose tens digit (3 left). Step 3: choose ones digit (2 left). So N = 4 x 3 x 2 = 24.

Example 4: A group has 5 people: 2 girls, 3 boys. Choose 2 for a show, at least 1 girl. How many ways?
Answer: Two cases: (1) Both girls: 1 way. (2) One girl, one boy: Choose girl (2 ways), choose boy (3 ways) => 2 x 3 = 6 ways. Total: 1 + 6 = 7.

Summary:

Addition Principle: total = sum of ways for each separate method.

Multiplication Principle: total = product of ways for each step.

Exercise 17.1:

School has 5 PE electives and 4 art electives.
(1) Choose 1 course from 9. How many choices?
(2) Choose 2 courses: 1 PE, 1 art. How many choices?

How many 3-digit numbers with no repeated digits from 1,2,3,4,5?

From set {0,1,2,3,4}, pick two different numbers a and b to make a+bi. How many different imaginary numbers? (Imaginary means b ≠ 0).

Bookshelf: 3 English books, 4 Chinese books. Take 2 books. How many choices?

Expand (a1+a2+a3+a4)(b1+b2+b3)(c1+c2). How many terms?

17.2 Permutations and Permutation Number

I. Definitions
A permutation is selecting m objects from n different objects and arranging them in order.
Order matters. Two permutations are the same only if same objects in same order.

Example 1: From 1,2,3, pick two to make a 2-digit number. List: 12,21,13,31,23,32. Total 6.

Permutation number A_n^m (or P(n,m)) is the total number of such permutations.
Formulas:

A_n^m = n(n-1)(n-2)...(n-m+1)

A_n^n = n!

A_n^m = n! / (n-m)! (m ≤ n, positive integers)

Examples: A_5^3 = 5x4x3 = 60. A_5^5 = 5! = 120.

II. Applications

Example 2: How many 3-digit numbers with no repeated digits from 0,1,2,3,4,5,6?
Answer: Hundreds digit cannot be 0, so 6 choices. Then tens digit: 6 choices left. Then ones digit: 5 choices left. So 6 x 6 x 5 = 180.
Or: Choose hundreds digit from 1-6: A_6^1 = 6. Then arrange two from remaining 6 digits: A_6^2 = 6x5=30. Total = 6 x 30 = 180.

Example 3: How many 4-digit even numbers with no repeated digits from 0-6?
Answer: Even means last digit is 0,2,4,6.
Case 1: Last digit 0. Choose first three digits from 1-6: A_6^3 = 6x5x4 = 120.
Case 2: Last digit is 2,4, or 6 (3 choices). Thousands digit cannot be 0 or the last digit, so 5 choices. Then choose hundreds and tens from remaining 5 digits (including 0): A_5^2 = 5x4 = 20. So case 2: 3 x 5 x 20 = 300.
Total: 120 + 300 = 420.

Summary:

Permutation: order matters.

A_n^m = number of ways to arrange m out of n objects.

Formulas above.

Exercise 17.2:

Calculate:
(1) A_7^2 + A_4^3 (2) 2A_4^2 + A_3^3 (3) A_8^5 / A_5^5 (4) 5A_9^2 + A_10^2 (5) A_6^1 + A_6^2 + A_6^3 + A_6^4 + A_6^5 + A_6^6

Using digits 0-8, how many with no repeated digits:
(1) 4-digit numbers? (2) 4-digit numbers divisible by 5? (3) 5-digit even numbers? (4) 5-digit even numbers > 10000?

From 0,1,2,3,4,5, pick 3 different digits for a,b,c in y=ax^2+bx+c (a≠0). How many different functions?

17.3 Combinations and Combination Number

I. Definitions
A combination is selecting m objects from n different objects to form a group. Order does not matter.
Combination number C_n^m (or "n choose m") is the total number of such groups.
Formulas:

C_n^m = A_n^m / m!

C_n^m = n! / (m!(n-m)!)

C_n^m = C_n^(n-m) (symmetry)

C_(n+1)^m = C_n^m + C_n^(m-1) (Pascal's rule)

Also: C_n^0 = 1, C_n^1 = n, C_n^n = 1.

Example: C_5^2 = (5x4)/(2x1) = 10. C_5^3 = 10 (same).

II. Applications

Example 2: Bag has 6 white balls, 3 red balls. Pick 4 balls.
(1) Total ways: C_9^4 = 126.
(2) 2 white, 2 red: C_6^2 x C_3^2 = 15 x 3 = 45.
(3) 3 white, 1 red: C_6^3 x C_3^1 = 20 x 3 = 60.
(4) All white: C_6^4 = 15.

Summary:

Combination: order does not matter.

C_n^m = number of ways to choose m out of n objects.

Formulas above.

Exercise 17.3:

Calculate:
(1) C_10^7 (2) C_10^3 (3) C_9^6 (4) C_15^13 (5) C_8^3 + C_8^4 (6) C_9^5 + C_9^6 (7) C_6^0 + C_6^1 + C_6^2 + C_6^3 + C_6^4 + C_6^5 + C_6^6

12 soccer teams, each pair plays once. How many matches?

From 20 different elements, choose 5 to form a set. How many sets?

25 products: 20 good, 5 defective. Choose 5.
(1) Total ways? (2) All good? (3) All defective? (4) 3 good, 2 defective? (5) 4 good, 1 defective?

Give 15 different books to 3 students: A, B, C.
(1) Each gets 5 books. How many ways?
(2) One gets 6, one gets 5, one gets 4. How many ways?

17.4 Binomial Theorem

Theorem: For any positive integer n,
(a + b)^n = C_n^0 a^n + C_n^1 a^(n-1) b + ... + C_n^k a^(n-k) b^k + ... + C_n^n b^n.
The right side is the binomial expansion. It has n+1 terms.
General term: T_(k+1) = C_n^k a^(n-k) b^k, where k=0,1,...,n.
Note: C_n^0 + C_n^1 + ... + C_n^n = 2^n.

Example 1: Expand (x-3)^4.
Answer: = C_4^0 x^4 - C_4^1 x^3 * 3 + C_4^2 x^2 * 9 - C_4^3 x * 27 + C_4^4 * 81 = x^4 - 12x^3 + 54x^2 - 108x + 81.

Example 2: Expand (2√x - 1/√x)^6.
Answer: Rewrite as ((2x-1)/√x)^6 = (2x-1)^6 / x^3.
Expand (2x-1)^6 = 64x^6 - 192x^5 + 240x^4 - 160x^3 + 60x^2 - 12x + 1.
Divide by x^3: 64x^3 - 192x^2 + 240x - 160 + 60/x - 12/x^2 + 1/x^3.

Example 3: Find coefficient of x^3 in (x - 1/x)^9.
Answer: General term: T_(k+1) = C_9^k x^(9-k) (-1/x)^k = (-1)^k C_9^k x^(9-2k).
Set 9-2k = 3 => k=3.
Coefficient = (-1)^3 C_9^3 = -84.

Example 4: Show 99^10 - 1 is divisible by 100.
Proof: 99^10 - 1 = (100-1)^10 - 1. Expand (100-1)^10. Every term except the last has factor 100. The last term is 1. So (100-1)^10 - 1 = multiple of 100.

Summary:
Binomial Theorem formula above.

Exercise 17.4:

Expand:
(1) (x+2)^5 (2) (x-1)^6 (3) (2x-1)^3 (4) (x + 1/x)^4 (5) (1-√x)^5 (6) (x - 3/√x)^4

Find coefficient of x^3 and constant term in (2x-3)^6.

Find coefficient of x^6 and constant term in (x^2 - 1/x)^6.

Find coefficient of x^4 in (2x + 1/√x)^10.

Find coefficient of x^6 in (1+x)(3x-2)^7.

Self-Test 17:

20 products: 16 good, 4 defective.
(1) Choose 1 product. How many ways?
(2) Choose 2: 1 good, 1 defective. How many ways?
(3) Choose 5: 2 good, 3 defective. How many ways?
(4) Choose 6: 4 good, 2 defective. How many ways?

Using 0-5, how many 3-digit numbers with no repeated digits?

10 markers, 6 crayons. Choose 2 to give away. How many choices?

Expand (a1+...+a5)(b1+...+b4)(c1+c2+c3). How many terms?

Calculate:
(1) 2A_6^4 + A_4^4 (2) (3A_8^5)/(4A_7^6) (3) C_10^7 * C_9^4 (4) 2C_6^5 + C_6^6 * C_8^6

Using digits 0-9, how many with no repeated digits:
(1) 5-digit numbers? (2) divisible by 5? (3) odd numbers? (4) odd numbers < 20000?

Expand:
(1) (3x-2)^5 (2) (x + 2/√x)^6

Find coefficient of x^(-3) and constant term in (3x - 1/x^2)^9.



Chapter 18 Random Events and Probability

18.1 Random Events and Their Operations

I. Random Experiments and Sample Space

A phenomenon that always occurs under certain conditions is called a certain phenomenon (e.g., the sun rises in the east).

A phenomenon that can result in different outcomes under the same basic conditions is called a random phenomenon (e.g., tossing a fair coin can land heads or tails).

Definition 1: An observation or experiment conducted under identical conditions on a random phenomenon is called a random experiment.

Example 1: Tossing a fair coin and observing if it lands heads or tails is a random experiment.
Example 2: Rolling a fair die and observing the top face is a random experiment.
Example 3: Recording the lifespan of a lightbulb is a random experiment.

Definition 2: Each possible outcome of an experiment is called a sample point, denoted by ω. The set of all sample points is the sample space, denoted by Ω.

Examples:

For coin toss: Ω = {Heads, Tails}.

For die roll: Ω = {1, 2, 3, 4, 5, 6}.

For lightbulb lifespan: Ω = [0, +∞).

A sample space with a finite number of sample points is a finite sample space. One with infinitely many points is an infinite sample space.

II. Random Events

Definition 3: A non-empty proper subset of the sample space Ω is called a random event (or just event). Events are denoted by capital letters A, B, C,...

Example 4: For die roll (Ω={1,2,3,4,5,6}), subsets A={3}, B={1,3,5}, C={2,4,5}, D={1,2,4,5,6} are all random events.

Definition 4: We say an event A occurs if a sample point in A appears.

Definition 5: An event that always occurs in an experiment is a certain event. An event that never occurs is an impossible event.

The sample space Ω itself is the certain event.

The empty set ∅ is the impossible event.

III. Operations on Random Events

Relations and operations between events are the same as for sets.

Definition 6: Subset (Containment). If every sample point in event A is also in event B, then A is contained in B (or B contains A), written A ⊆ B (or B ⊇ A).
Example: In Example 4, A ⊆ B because A={3} is inside B={1,3,5}.

Definition 7: Equality. If A ⊆ B and B ⊆ A, then A = B.

Definition 8: Union (Sum Event). The event consisting of all sample points in A or in B (or both) is the union of A and B, written A ∪ B.
Example: B ∪ C = {1,2,3,4,5}.

Definition 9: Intersection (Product Event). The event consisting of sample points common to both A and B is the intersection of A and B, written A∩B or AB.
Example: B ∩ C = {5}.

Definition 10: Mutually Exclusive (Disjoint) Events. If events A and B have no common sample points, they are mutually exclusive, written A∩B = ∅.
Example: A∩C = ∅, so A and C are mutually exclusive.

Note: For mutually exclusive events, A ∪ B can be written as A + B.

Definition 11: Complementary Event. The event consisting of all sample points in Ω that are not in A is the complement of A, written Ā (or A').
Properties: Ā ∩ A = ∅ and A ∪ Ā = Ω.
Example: In Example 4, A={3} and D={1,2,4,5,6} are complements.
Note: Complementary events are always mutually exclusive, but mutually exclusive events are not necessarily complementary.

Example 5: Let A and B be two events. Express in symbols:
(1) Both A and B occur → AB
(2) Neither A nor B occurs → ĀB̄
(3) At least one of A or B occurs → A ∪ B
(4) A occurs but B does not → A B̄
(5) Exactly one of A or B occurs → A B̄ ∪ Ā B

IV. Properties of Event Operations
Same as set operations:

Commutative: A ∪ B = B ∪ A; AB = BA.

Associative: (A∪B)∪C = A∪(B∪C); (AB)C = A(BC).

Distributive: (A∪B)∩C = (A∩C)∪(B∩C); (A∩B)∪C = (A∪C)∩(B∪C).

De Morgan's Laws: 
A
∪
B
‾
=
A
ˉ
∩
B
ˉ
A∪B
 = 
A
ˉ
 ∩ 
B
ˉ
 ; 
A
∩
B
‾
=
A
ˉ
∪
B
ˉ
A∩B
 = 
A
ˉ
 ∪ 
B
ˉ
 .

Summary:

Know definitions: sample point, sample space, random event, certain event, impossible event, containment, equality, union, intersection, mutually exclusive, complement.

Event operations follow set operation rules.

Exercise 18.1:

Let A, B, C be three events. Express in symbols:
(1) All three occur → A∩B∩C
(2) Exactly one occurs → (A B̄ C̄) ∪ (Ā B C̄) ∪ (Ā B̄ C)
(3) At least one occurs → A ∪ B ∪ C

A worker makes 3 parts. Let A_i = {part i is合格品 (qualified)}, i=1,2,3. Express:
(1) B1 = {all 3 are qualified} → A1 A2 A3
(2) B2 = {exactly 1 is defective} → (Ā1 A2 A3) ∪ (A1 Ā2 A3) ∪ (A1 A2 Ā3)
(3) B3 = {at least 1 is qualified} → A1 ∪ A2 ∪ A3
(4) B4 = {all 3 are defective} → Ā1 Ā2 Ā3

Roll a fair die. Events: C_i={point is i}, i=1..6; D1={point ≤2}; D2={point >2}; D3={point >4}. Which statement is false?
A. C1 and C2 are mutually exclusive. (True, they can't both happen)
B. D1 ∪ D2 = Ω and D1∩D2 = ∅. (True, they partition Ω)
C. D3 ⊆ D2. (True, points >4 are also >2)
D. C2 and C3 are complementary. (False, complement of {2} is {1,3,4,5,6}, not {3})

Answer: D

Dance group: 3 boys, 2 girls. Pick 2 for competition.
A = {both are boys}, B = {both are girls}, C = {exactly 1 boy}, D = {at least 1 boy}.
Which relation is incorrect?
A. A ⊆ D (True, if both boys, at least 1 boy)
B. B ∩ D = ∅ (True, "both girls" and "at least 1 boy" can't happen together)
C. A ∪ C = D (True, "both boys" or "exactly 1 boy" = "at least 1 boy")
D. A ∪ B = B ∪ D (Check: A∪B = {both same gender}; B∪D = {both girls} ∪ {at least 1 boy} = all outcomes except "both boys"? Actually, B∪D includes all outcomes because D includes cases with boys and B includes both girls. So B∪D = Ω. A∪B = {both boys or both girls} which is not Ω (it excludes mixed groups). So D is false.)

Answer: D

18.2 Definition and Properties of Probability

I. Definition of Probability
Definition 1: For a random event A, the numerical value that describes its likelihood of occurring is called the probability of A, written P(A).

Note: P(∅)=0, P(Ω)=1.

Example 1: Roll a fair die. Probability of rolling a 3: P(3) = 1/6.

II. Properties of Probability
For any events A, B:

Additivity for Mutually Exclusive Events: If A∩B = ∅, then P(A ∪ B) = P(A) + P(B). (Also written P(A+B) = P(A)+P(B))
More generally, if A1, A2,..., An are pairwise mutually exclusive, then P(A1+A2+...+An) = P(A1)+P(A2)+...+P(An).

Complement: P(Ā) = 1 - P(A).

Difference: If A ⊆ B, then P(B \ A) = P(B) - P(A).

Monotonicity: If A ⊆ B, then P(A) ≤ P(B).

Bounds: 0 ≤ P(A) ≤ 1.

Inclusion-Exclusion: P(A ∪ B) = P(A) + P(B) - P(A∩B).

Example 2: Let A, B be events with P(A)=0.2, P(B)=0.6.
(1) Find P(Ā) = 1 - 0.2 = 0.8.
(2) If A∩B = ∅, find P(A∪B) = 0.2 + 0.6 = 0.8.
(3) If A ⊆ B, find P(B \ A) = 0.6 - 0.2 = 0.4.
(4) If P(A∩B) = 0.3, find P(A∪B) = 0.2 + 0.6 - 0.3 = 0.5.

Summary:

Probability P(A) measures likelihood.

Properties 1-6 above.

Exercise 18.2:

A student has 5 books: Math, Chinese, English, Physics, Chemistry. Pick one randomly.
(1) Probability it's English? → 1/5
(2) Probability it's a science book (Physics or Chemistry)? → 2/5

P(A)=0.1, P(B̄)=0.3.
(1) Find P(B) = 1 - 0.3 = 0.7.
(2) If A∩B = ∅, find P(A∪B) = 0.1 + 0.7 = 0.8.
(3) If A ⊆ B, find P(B \ A) = 0.7 - 0.1 = 0.6.
(4) If P(A∩B)=0.5, find P(A∪B) = 0.1 + 0.7 - 0.5 = 0.3.

Events A, B, C are pairwise mutually exclusive. P(A)=0.3, P(B)=0.4, P(C̄)=0.6. Find P(A+B+C).
First, P(C) = 1 - 0.6 = 0.4.
Since mutually exclusive, P(A+B+C) = 0.3 + 0.4 + 0.4 = 1.1? That's >1, check: 0.3+0.4+0.4=1.1, but probabilities sum to at most 1. Possibly error in given data? According to properties, if they are pairwise mutually exclusive and exhaustive? Not given. But formula P(A+B+C) = P(A)+P(B)+P(C) only if mutually exclusive. So answer would be 1.1, but probability cannot exceed 1. So maybe P(C̄)=0.6 means P(C)=0.4, but then sum=1.1 is impossible. Possibly they are not exhaustive. Problem might be flawed.

Bank queue statistics:
Number waiting: 0, 1, 2, 3, 4, ≥5.
Probability: 0.10, 0.15, 0.25, 0.35, 0.10, 0.05.
(1) P(at most 3 waiting) = P(0)+P(1)+P(2)+P(3) = 0.10+0.15+0.25+0.35 = 0.85.
(2) P(at least 2 waiting) = 1 - P(0 or 1) = 1 - (0.10+0.15) = 0.75.

Roll a fair die. P(any face)=1/6.
A = {odd number} = {1,3,5}, P(A)=3/6=1/2.
B = {number ≤3} = {1,2,3}, P(B)=3/6=1/2.
A∩B = {1,3}, P(A∩B)=2/6=1/3.
Find P(A∪B) = P(A)+P(B)-P(A∩B) = 1/2 + 1/2 - 1/3 = 1 - 1/3 = 2/3.

18.3 Classical Probability Model

I. Definition
Definition: A random experiment with a finite sample space Ω, where each sample point is equally likely, is called a classical probability model.

Example 1: Rolling a fair die: Ω={1,2,3,4,5,6}, each equally likely (1/6). It's classical.

II. Probability Calculation in Classical Model
For an event A: P(A) = (Number of sample points in A) / (Total number of sample points in Ω).

Example 2: Roll fair die.
(1) P(odd number) = |{1,3,5}| / 6 = 3/6 = 1/2.
(2) P(number > 4) = |{5,6}| / 6 = 2/6 = 1/3.

Example 3: Toss a fair coin twice. Ω = {(H,H), (H,T), (T,H), (T,T)}. P(at least one head) = 3/4.

Example 4: Bag: 3 white balls, 2 red balls. Pick 3 balls. P(exactly 1 red) = ?
Total ways to pick 3 from 5: C(5,3)=10.
Ways to pick exactly 1 red (and 2 white): Choose 1 red from 2: C(2,1)=2; choose 2 white from 3: C(3,2)=3. Total favorable = 2*3=6.
So P = 6/10 = 3/5.

Summary:

Classical model: finite sample space, equally likely outcomes.

P(A) = (# favorable outcomes) / (# total possible outcomes).

Exercise 18.3:

Pick a number from 1 to 15.
(1) P(odd) = 8/15? Numbers 1-15: odds are 1,3,5,7,9,11,13,15 → 8 numbers. So 8/15.
(2) P(multiple of 5) = 3/15 = 1/5 (5,10,15).
(3) P(number < 10) = 9/15 = 3/5 (numbers 1-9).

3合格品, 1 defective. Pick 2.
(1) P(both qualified) = C(3,2)/C(4,2) = 3/6 = 1/2.
(2) P(exactly 1 defective) = [C(1,1)*C(3,1)] / C(4,2) = (1*3)/6 = 1/2.

Bag: 5 white, 3 red. Pick 4.
(1) P(all white) = C(5,4)/C(8,4) = 5/70 = 1/14.
(2) P(3 white, 1 red) = [C(5,3)*C(3,1)] / C(8,4) = (10*3)/70 = 30/70 = 3/7.
(3) P(at least 2 red) = P(2 red) + P(3 red). C(8,4)=70.
P(2 red, 2 white) = [C(3,2)*C(5,2)]/70 = (3*10)/70=30/70.
P(3 red, 1 white) = [C(3,3)*C(5,1)]/70 = (1*5)/70=5/70.
Sum = 35/70 = 1/2.

Roll a fair die twice.
(1) P(sum > 8). Total outcomes: 6*6=36.
Sum>8 means sum=9,10,11,12.
Ways: 9: (3,6),(4,5),(5,4),(6,3) → 4
10: (4,6),(5,5),(6,4) → 3
11: (5,6),(6,5) → 2
12: (6,6) → 1
Total favorable = 4+3+2+1=10. P=10/36=5/18.
(2) P(|difference| < 3) = P(diff = 0,1,2).
Count: diff=0: (1,1)...(6,6) → 6.
diff=1: (1,2),(2,1),(2,3),(3,2),(3,4),(4,3),(4,5),(5,4),(5,6),(6,5) → 10.
diff=2: (1,3),(3,1),(2,4),(4,2),(3,5),(5,3),(4,6),(6,4) → 8.
Total = 6+10+8=24. P=24/36=2/3.
(3) P(at least one 6) = 1 - P(no 6) = 1 - (5/6 * 5/6) = 1 - 25/36 = 11/36.

18.4 Conditional Probability and Multiplication Rule

I. Conditional Probability
Definition: If P(B)>0, the probability of A given that B has occurred is the conditional probability P(A|B) = P(A∩B) / P(B).

Example 1: Roll a die twice. Find P(sum>9 | first roll is 6).
Let B={first roll=6}, A={sum>9}.
P(B)=1/6.
A∩B = {(6,4),(6,5),(6,6)} → 3 outcomes out of 36, so P(A∩B)=3/36=1/12.
Thus P(A|B)= (1/12) / (1/6) = (1/12)*(6/1)=1/2.

II. Multiplication Rule
From P(A|B)=P(A∩B)/P(B), we get:
P(A∩B) = P(A|B) P(B). (if P(B)>0)
Similarly, P(A∩B) = P(B|A) P(A). (if P(A)>0)

Example 2: 10 lottery tickets, 2 are winners.甲乙 draw one after another without replacement.
Let A={甲 wins}, B={乙 wins}.
(1) P(both win) = P(B|A) P(A) = (1/9)*(2/10)=2/90=1/45.
(2) P(甲 wins, 乙 loses) = P(B̄|A) P(A) = (8/9)*(2/10)=16/90=8/45.

III. Total Probability Formula and Bayes' Formula

Theorem (Total Probability): Suppose events A1, A2,..., An form a partition of Ω (i.e., they are pairwise mutually exclusive and their union is Ω), and P(Ai)>0 for all i. Then for any event B:
P(B) = Σ_{i=1}^n P(B|Ai) P(Ai).

Theorem (Bayes' Formula): If also P(B)>0, then
P(Ai|B) = [P(B|Ai) P(Ai)] / [Σ_{j=1}^n P(B|Aj) P(Aj)].

Example 3: Three factories produce items:

Factory 1: 40% of total, defect rate 1%.

Factory 2: 40% of total, defect rate 2%.

Factory 3: 20% of total, defect rate 4%.
Pick an item at random.
(1) P(defective) = ?
Let Ai = {item from factory i}, i=1,2,3. B = {defective}.
P(A1)=0.4, P(A2)=0.4, P(A3)=0.2.
P(B|A1)=0.01, P(B|A2)=0.02, P(B|A3)=0.04.
P(B) = 0.01*0.4 + 0.02*0.4 + 0.04*0.2 = 0.004 + 0.008 + 0.008 = 0.02.
(2) If item is defective, probability it's from factory 1?
P(A1|B) = [P(B|A1)P(A1)] / P(B) = (0.01*0.4)/0.02 = 0.004/0.02 = 0.2.

Summary:

Conditional probability: P(A|B)=P(A∩B)/P(B).

Multiplication rule: P(A∩B)=P(A|B)P(B)=P(B|A)P(A).

Total probability formula.

Bayes' formula.

Exercise 18.4:

Find P(B|A) given:
(1) P(A)=0.3, P(A∩B)=0.2 → P(B|A)=0.2/0.3=2/3.
(2) P(A)=0.8, P(A∩B)=0.5 → P(B|A)=0.5/0.8=5/8.

Find P(A∩B) and P(A∩B̄) given:
(1) P(B)=0.5, P(A|B)=0.3 → P(A∩B)=0.3*0.5=0.15. P(A∩B̄) = P(A) - P(A∩B) but P(A) not given? Possibly need more info. Maybe only find P(A∩B).
(2) P(B)=0.6, P(A|B)=0.4 → P(A∩B)=0.4*0.6=0.24.

Bag: 6 white, 2 red. Draw two without replacement.
(1) P(2nd white | 1st white) = After drawing one white, 5 white, 2 red left. So P=5/7.
(2) P(2nd red | 1st white) = 2/7.
(3) P(2nd white | 1st red) = After drawing one red, 6 white, 1 red left. So P=6/7.
(4) P(2nd red | 1st red) = 1/7.

Cards numbered 1-5. Draw two without replacement.
(1) P(2nd even | 1st even). If first even: possible evens: 2,4. After drawing one even, 1 even left out of 4 cards. So P=1/4.
(2) P(2nd odd | 1st even) = After first even, 3 odds left out of 4 cards. So P=3/4.

Factories:甲 40%,合格率 90%;乙 60%,合格率 95%. Pick one item.
(1) P(qualified) = 0.9*0.4 + 0.95*0.6 = 0.36 + 0.57 = 0.93.
(2) P(from乙 | qualified) = [0.95*0.6] / 0.93 = 0.57/0.93 = 57/93 = 19/31.

Factories:甲 300件,优质率 0.8;乙 300件,优质率 0.9;丙 400件,优质率 0.95. Total 1000 items. Pick one.
(1) P(优质品) = (300/1000)*0.8 + (300/1000)*0.9 + (400/1000)*0.95 = 0.24 + 0.27 + 0.38 = 0.89.
(2) P(from丙 | 优质品) = (0.38) / 0.89 = 38/89.

Bag: 20 white, 10 yellow balls. Two people draw one ball each without replacement. Find P(2nd person gets yellow).
Let B={2nd gets yellow}. Use total probability conditioning on first draw:
P(B) = P(B|first white)P(first white) + P(B|first yellow)P(first yellow)
= (10/29)*(20/30) + (9/29)*(10/30) = (200/870) + (90/870) = 290/870 = 29/87 = 1/3? Actually 290/870 simplifies to 29/87, which is not 1/3 (29/87=1/3 exactly? 29*3=87, yes 1/3). So P=1/3. Interesting: same as probability first person gets yellow (10/30=1/3). Symmetry.

18.5 Independence of Random Events

I. Definition of Independence
Definition: Two events A and B are independent if P(A∩B) = P(A) P(B).

Example 1: Roll a die twice. A={first roll=6}, B={second roll=5}. P(A)=1/6, P(B)=1/6, P(A∩B)=1/36. Since 1/36 = (1/6)*(1/6), they are independent.

II. Properties of Independence
Theorem 1: If A and B are independent, then:

Ā and B are independent.

A and B̄ are independent.

Ā and B̄ are independent.

Example 2: In Example 1, find P(Ā∩B). Since Ā and B independent, P(Ā∩B)=P(Ā)P(B)=(5/6)*(1/6)=5/36.

Theorem 2: If P(A)>0, then A and B independent ⇔ P(B|A) = P(B).

Note: Independence means occurrence of one does not affect probability of the other.

Example 3: Given P(B|A)=0.4, P(B̄)=0.6. Determine if A and B independent.
P(B)=1-0.6=0.4. Since P(B|A)=0.4 = P(B), they are independent.

Example 4: 甲 and 乙 shoot. P(甲 hits)=0.8, P(乙 hits)=0.9. They shoot independently. P(both hit)=0.8*0.9=0.72.

Note 2: Events A1, A2,..., An are mutually independent if the probability of any intersection of a subset of them equals the product of their individual probabilities. For three events A,B,C independent, we need:
P(A∩B)=P(A)P(B), P(A∩C)=P(A)P(C), P(B∩C)=P(B)P(C), and P(A∩B∩C)=P(A)P(B)P(C).

Example 5: 甲,乙,丙 take a final exam. P(甲 passes)=0.8, P(乙 passes)=0.7, P(丙 passes)=0.9, independently. P(all pass)=0.8*0.7*0.9=0.504.

Summary:

A and B independent ⇔ P(A∩B)=P(A)P(B).

If independent, then complements also independent.

If P(A)>0, independence ⇔ P(B|A)=P(B).

Mutual independence for multiple events requires all such product rules.

Exercise 18.5:

Determine if A and B independent:
(1) P(A∩B)=0.5, P(A)=0.2, P(B)=0.3 → 0.2*0.3=0.06 ≠ 0.5, so not independent.
(2) P(A∩B)=1/6, P(A)=1/4, P(B)=2/3 → (1/4)*(2/3)=2/12=1/6, so independent.
(3) P(B|A)=0.8, P(B)=0.79 → Not equal, so not independent.
(4) P(B|A)=0.7, P(B)=0.7 → Equal, so independent.
(5) P(B|A)=0.2, P(B̄)=0.8 → P(B)=0.2. Since P(B|A)=P(B)=0.2, independent.

P(甲 snow)=0.3, P(乙 snow)=0.6, independent.
(1) P(both snow)=0.3*0.6=0.18.
(2) P(neither snow)= (1-0.3)*(1-0.6)=0.7*0.4=0.28.

A,B independent, P(A)=1/3, P(B)=2/5.
P(A∩B)= (1/3)*(2/5)=2/15.
P(Ā∩B)= P(Ā)P(B)= (2/3)*(2/5)=4/15.
P(A∩B̄)= P(A)P(B̄)= (1/3)*(3/5)=3/15=1/5.

A,B independent, P(A∩B)=2/5, P(B)=2/3.
Find P(A): Since P(A∩B)=P(A)P(B), so P(A)= (2/5) / (2/3) = (2/5)*(3/2)=3/5.
P(Ā∩B)= P(Ā)P(B)= (2/5)*(2/3)=4/15? Wait: P(Ā)=1-3/5=2/5. So P(Ā∩B)= (2/5)*(2/3)=4/15.
P(A∩B̄)= P(A)P(B̄)= (3/5)*(1/3)=3/15=1/5.

A,B independent, P(A∩B)=1/10, P(B)=4/5.
P(A)= (1/10) / (4/5) = (1/10)*(5/4)=5/40=1/8.
P(Ā|B)= P(Ā)=1-1/8=7/8 (since independent).
P(A|B̄)= P(A)=1/8.

Bag: 4 red, 6 white. 甲,乙,丙 each draw one ball with replacement.
(1) P(all get red)= (4/10)^3 = (2/5)^3 = 8/125.
(2) P(甲 red, 乙 red, 丙 white)= (4/10)*(4/10)*(6/10)= (2/5)*(2/5)*(3/5)=12/125.

Self-Test 18:

Toss a fair coin three times. Let T=Heads, F=Tails. Write sample space Ω.
Ω = {TTT, TTH, THT, THH, HTT, HTH, HHT, HHH}.

Worker makes 4 parts. A_i = {part i is qualified}, i=1..4. Express:
(1) B1 = {all 4 qualified} → A1 A2 A3 A4.
(2) B2 = {all 4 defective} → Ā1 Ā2 Ā3 Ā4.
(3) B3 = {exactly 2 qualified} → sum over choices of 2 parts being qualified, others defective. E.g., A1 A2 Ā3 Ā4 ∪ A1 Ā2 A3 Ā4 ∪ ... etc. (C(4,2)=6 such combinations).
(4) B4 = {at least 2 qualified} → union of events with 2,3, or 4 qualified.

P(Ā)=0.4, P(B)=0.2.
(1) P(A)=1-0.4=0.6.
(2) If A∩B=∅, P(A∪B)=0.6+0.2=0.8.
(3) If B ⊆ A, P(A \ B)=0.6-0.2=0.4.
(4) If P(A∩B)=0.3, P(A∪B)=0.6+0.2-0.3=0.5.

Bag: 10 white, 5 red. Pick 5.
(1) P(all white)= C(10,5)/C(15,5).
(2) P(2 white, 3 red)= [C(10,2)*C(5,3)] / C(15,5).
(3) P(at least 3 red)= P(3 red)+P(4 red)+P(5 red) = [C(5,3)C(10,2)+C(5,4)C(10,1)+C(5,5)C(10,0)] / C(15,5).

P(A)=0.6, P(A∩B)=0.1. Find P(B|A)=0.1/0.6=1/6.

P(B)=0.3, P(A|B)=0.2. Find P(A∩B)=0.2*0.3=0.06. P(A∩B̄) = P(A) - 0.06, but P(A) unknown.

8 items: 6 good, 2 defective. Draw 2 without replacement.
(1) P(2nd defective | 1st good) = After first good, 5 good, 2 defective left → 2/7.
(2) P(2nd defective) = Use total probability: P(2nd defective) = P(2nd def|1st good)P(1st good) + P(2nd def|1st def)P(1st def) = (2/7)*(6/8) + (1/7)*(2/8) = (12/56) + (2/56)=14/56=1/4.

Factories: 甲 40%,合格率 90%;乙 30%,合格率 95%;丙 30%,合格率 96%.
(1) P(qualified) = 0.9*0.4 + 0.95*0.3 + 0.96*0.3 = 0.36 + 0.285 + 0.288 = 0.933.
(2) P(甲|qualified) = 0.36/0.933, P(乙|qualified)=0.285/0.933, P(丙|qualified)=0.288/0.933.

A,B independent, P(A∩B)=0.6, P(B)=0.8.
Find P(A)=0.6/0.8=0.75.
P(Ā∩B)= P(Ā)P(B)= (0.25)*0.8=0.2.
P(A∩B̄)= P(A)P(B̄)=0.75*0.2=0.15.
P(A|B)= P(A)=0.75.
P(A|B̄)= P(A)=0.75.


Chapter 19 Random Variables and Their Numerical Characteristics

19.1 Discrete Random Variables and Probability Distribution

I. Definition of Random Variable

Definition 1: For a random experiment with sample space Ω, if for each sample point ω there is a unique real number X(ω) assigned, then X is called a random variable (RV), usually written as X, Y, Z, etc.
A random variable makes outcomes numerical.

Example 1: Toss a fair coin. Ω={Heads, Tails}. Define X=1 for Heads, X=0 for Tails. Then X is a random variable. P(Heads)=P(X=1).

Example 2: Bag: 3 white, 5 red balls. Draw 4 balls. Let X = number of red balls drawn.
X can be 1,2,3,4.
P(X=1) = [C(5,1)*C(3,3)] / C(8,4) = 5/70 = 1/14.
P(X=2) = [C(5,2)*C(3,2)] / C(8,4) = (10*3)/70 = 30/70 = 3/7.

II. Distribution of Discrete Random Variables

Definition 2: A random variable whose possible values are finite or countably infinite (like a list) is called a discrete random variable.

Examples 1 and 2 are discrete.

Definition 3: A random variable whose possible values form a continuous interval is called a continuous random variable.

Example: Lifespan of a lightbulb, X ≥ 0, is continuous.

Definition 4: Let X be a discrete RV with possible values x1, x2, ..., xn, ... . Let p_k = P(X = x_k). The set of probabilities p_k is the probability distribution of X, if:

p_k ≥ 0 for all k.

Sum over all k of p_k = 1.

We can write it as a table:

X: x1 x2 ... xn ...
P: p1 p2 ... pn ...

Example 3: 10 items: 8 good, 2 defective. Pick 3. Let X = number of defectives.
X can be 0,1,2.
P(X=0) = C(8,3)/C(10,3) = 56/120 = 7/15.
P(X=1) = [C(2,1)*C(8,2)] / C(10,3) = (2*28)/120 = 56/120 = 7/15.
P(X=2) = [C(2,2)*C(8,1)] / C(10,3) = (1*8)/120 = 8/120 = 1/15.
Distribution table:
X 0 1 2
P 7/15 7/15 1/15

(2) P(X ≥ 1) = P(X=1)+P(X=2) = 7/15 + 1/15 = 8/15.

Note: If Y = aX + b (a,b constants, a≠0), and X has distribution as above, then Y has distribution:

Y: a x1 + b a x2 + b ... a xn + b ...
P: p1 p2 ... pn ...

Example 4: X has distribution:
X -2 -1 0 1 2
P 1/10 3/10 1/10 3/10 1/5
Let Y = 2X - 1. Then Y's values: 2*(-2)-1=-5, 2*(-1)-1=-3, 2*0-1=-1, 2*1-1=1, 2*2-1=3.
Probabilities same as corresponding X.
So Y distribution:
Y -5 -3 -1 1 3
P 1/10 3/10 1/10 3/10 1/5

Summary:

Random variable X assigns numbers to outcomes.

Discrete RV: countable possible values.

Continuous RV: values in an interval.

Probability distribution for discrete X: list of P(X=x_k) satisfying p_k≥0, sum p_k=1.

Linear transformation Y=aX+b: shift and scale values, keep probabilities.

Exercise 19.1:

Discrete X distribution:
X -3 -2 0 1
P 0.2 a 0.3 0.4
(1) Find a: Since sum=1, 0.2+a+0.3+0.4=1 → a=0.1.
(2) P(X ≥ -2) = P(X=-2)+P(X=0)+P(X=1) = 0.1+0.3+0.4=0.8.

Toss fair coin twice. X = number of heads. Find distribution.
Possible X: 0,1,2.
P(X=0)=P(TT)=1/4.
P(X=1)=P(HT,TH)=2/4=1/2.
P(X=2)=P(HH)=1/4.
Table:
X 0 1 2
P 1/4 1/2 1/4

Roll fair die twice. X = number of times showing 6. Find distribution.
X: 0,1,2.
P(X=0)= (5/6)*(5/6)=25/36.
P(X=1)= 2*(1/6)*(5/6)=10/36.
P(X=2)= (1/6)*(1/6)=1/36.
Check sum=36/36=1.

Bag: 4 white, 3 red. Draw 3. X = number of white balls.
(1) Distribution:
X=0: all red → C(3,3)/C(7,3)=1/35.
X=1: 1 white, 2 red → [C(4,1)*C(3,2)]/C(7,3)= (4*3)/35=12/35.
X=2: 2 white, 1 red → [C(4,2)*C(3,1)]/35 = (6*3)/35=18/35.
X=3: all white → C(4,3)/35=4/35.
Check sum: 1+12+18+4=35 → 35/35=1.
Table:
X 0 1 2 3
P 1/35 12/35 18/35 4/35

(2) P(X≥2) = P(X=2)+P(X=3)=18/35+4/35=22/35.
(3) Y=3X+2. Values: when X=0, Y=2; X=1, Y=5; X=2, Y=8; X=3, Y=11.
Probabilities same.
Y distribution:
Y 2 5 8 11
P 1/35 12/35 18/35 4/35

12 items: 8 good, 4 defective. Pick 5. X = number of good items.
(1) Distribution: X can be 1,2,3,4,5? Actually min good? If pick 5, at least 1 good because only 4 defective. So X=1,2,3,4,5.
P(X=k) = [C(8,k)*C(4,5-k)] / C(12,5) for k=1,...,5.
Compute:
C(12,5)=792.
k=1: C(8,1)*C(4,4)=8*1=8 → 8/792=1/99.
k=2: C(8,2)*C(4,3)=28*4=112 → 112/792=14/99.
k=3: C(8,3)*C(4,2)=56*6=336 → 336/792=42/99=14/33.
k=4: C(8,4)*C(4,1)=70*4=280 → 280/792=35/99.
k=5: C(8,5)*C(4,0)=56*1=56 → 56/792=7/99.
Check sum: 1/99+14/99+42/99+35/99+7/99 = 99/99=1.
Table:
X 1 2 3 4 5
P 1/99 14/99 14/33 35/99 7/99

(2) P(X≤3) = P(X=1)+P(X=2)+P(X=3) = 1/99+14/99+42/99=57/99=19/33.
(3) Y=4X-3. Values: X=1→Y=1; X=2→Y=5; X=3→Y=9; X=4→Y=13; X=5→Y=17.
Probabilities same.
Y distribution:
Y 1 5 9 13 17
P 1/99 14/99 14/33 35/99 7/99

19.2 Numerical Characteristics of Discrete Random Variables

I. Mean (Expected Value)

Definition 1: Let discrete X have distribution:
X: x1 x2 ... xn
P: p1 p2 ... pn
The mean or expected value of X is:
E(X) = x1 p1 + x2 p2 + ... + xn p_n = Σ x_k p_k.

It's a weighted average of values, weights are probabilities.

Example 1: X distribution:
X 1 2 3 4
P 0.1 0.4 0.3 0.2
E(X)=1*0.1 + 2*0.4 + 3*0.3 + 4*0.2 = 0.1+0.8+0.9+0.8 = 2.6.

II. Variance

Definition 2: The variance of X measures spread around mean:
Var(X) = D(X) = E[(X - E(X))^2] = Σ (x_k - E(X))^2 p_k.
Standard deviation σ(X) = sqrt(Var(X)).

Example 2: X distribution:
X -2 1 3
P 0.2 0.5 0.3
First, E(X)= (-2)*0.2 + 1*0.5 + 3*0.3 = -0.4+0.5+0.9=1.0.
Var(X)= (-2-1)^2*0.2 + (1-1)^2*0.5 + (3-1)^2*0.3 = 9*0.2 + 0 + 4*0.3 = 1.8+1.2=3.0.

Summary:
For discrete X with distribution (x_k, p_k):

Mean: E(X)= Σ x_k p_k.

Variance: Var(X)= Σ (x_k - E(X))^2 p_k.

Exercise 19.2:

X distribution:
X 4 6 8 10
P 0.1 0.2 0.3 0.4
E(X)=4*0.1+6*0.2+8*0.3+10*0.4=0.4+1.2+2.4+4.0=8.0.
Var(X)= (4-8)^2*0.1+(6-8)^2*0.2+(8-8)^2*0.3+(10-8)^2*0.4 =16*0.1+4*0.2+0+4*0.4=1.6+0.8+1.6=4.0.

X:
X 1 4 7 10
P 0.3 0.4 0.1 0.2
E(X)=1*0.3+4*0.4+7*0.1+10*0.2=0.3+1.6+0.7+2.0=4.6.
Var(X)= (1-4.6)^2*0.3+(4-4.6)^2*0.4+(7-4.6)^2*0.1+(10-4.6)^2*0.2 =12.96*0.3+0.36*0.4+5.76*0.1+29.16*0.2=3.888+0.144+0.576+5.832=10.44.

X:
X -10 -5 0 5 10
P 0.1 0.1 0.3 0.2 0.3
E(X)= (-10)*0.1 + (-5)*0.1 + 0*0.3 + 5*0.2 + 10*0.3 = -1.0 -0.5 +0 +1.0+3.0=2.5.
Var(X)= (-10-2.5)^2*0.1 + (-5-2.5)^2*0.1 + (0-2.5)^2*0.3 + (5-2.5)^2*0.2 + (10-2.5)^2*0.3 =156.25*0.1+56.25*0.1+6.25*0.3+6.25*0.2+56.25*0.3=15.625+5.625+1.875+1.25+16.875=41.25.

X:
X -6 -2 1 2 3
P 1/8 3/8 1/8 1/4 1/8
E(X)= (-6)*(1/8)+(-2)*(3/8)+1*(1/8)+2*(1/4)+3*(1/8) = -6/8 -6/8 +1/8 +2/4 +3/8 = (-6-6+1+3)/8 + 1/2 = (-8/8) + 1/2 = -1 + 0.5 = -0.5.
Var(X)= (-6+0.5)^2*(1/8)+(-2+0.5)^2*(3/8)+(1+0.5)^2*(1/8)+(2+0.5)^2*(1/4)+(3+0.5)^2*(1/8) =30.25/8 + 2.25*3/8 + 2.25/8 + 6.25/4 + 12.25/8 = (30.25+6.75+2.25+12.25)/8 + 6.25/4 = 51.5/8 + 6.25/4 = 6.4375 + 1.5625 = 8.0.

19.3 Normal Distribution

I. Definition of Normal Distribution

A continuous random variable X follows a normal distribution with parameters μ (mean) and σ^2 (variance), written X ~ N(μ, σ^2), if its probability density function (PDF) is:
f(x) = (1 / (σ√(2π))) * e^{-(x-μ)^2/(2σ^2)}, for -∞ < x < ∞.
Here σ > 0.

The probability that X lies between a and b is the area under f(x) from a to b.

Properties of f(x):

Bell-shaped, symmetric about x = μ.

Maximum at x = μ, height = 1/(σ√(2π)).

μ shifts the curve left/right. σ controls spread: smaller σ → taller, narrower peak; larger σ → shorter, wider spread.

Standard Normal Distribution: If μ=0, σ=1, then Z ~ N(0,1) is standard normal.

Note: The cumulative distribution function (CDF) F(x) = P(X ≤ x) is area from -∞ to x. For normal, we use tables or software.

Summary:

Normal distribution is continuous, symmetric, bell curve.

Parameters: mean μ, variance σ^2.

PDF formula above.

Example 1 If random variable X ~ N(2, 3), then P(X > 2) = ______.

Solution Because X ~ N(2, 3), the graph of its probability density function f(x) is symmetric about the line x = 2. So, P(X > 2) = P(X <= 2). Also, P(X > 2) + P(X <= 2) = 1. Therefore, P(X > 2) = 1/2.

The answer is 1/2.

Example 2 If random variable X ~ N(-1, 2), and P(X > 0) = 0.2, then P(-1 <= X <= 0) = ______.

Solution Because X ~ N(-1, 2), the graph of its probability density function f(x) is symmetric about the line x = -1. So, P(X >= -1) = 0.5. We also know P(X >= -1) = P(-1 <= X <= 0) + P(X > 0), and P(X > 0) = 0.2. Therefore, P(-1 <= X <= 0) = 0.5 - 0.2 = 0.3.

The answer is 0.3.

2. Standard Normal Distribution

The normal distribution with parameters μ = 0 and σ = 1 is called the standard normal distribution. If random variable X follows the standard normal distribution, we write X ~ N(0, 1). In this case, the probability density function of X is written as:

φ(x) = (1 / sqrt(2π)) * e^(-x²/2) (-∞ < x < +∞)

Note 2
(1) The graph of φ(x) is symmetric about the y-axis.
(2) If random variable X ~ N(μ, σ²), then it can be shown that (X - μ)/σ ~ N(0, 1).

Example 3 If random variable X ~ N(-3, 4), which of the following follows the standard normal distribution?
A. (X - 3)/4
B. (X + 3)/4
C. (X - 3)/2
D. (X + 3)/2

Solution From X ~ N(-3, 4), we know μ = -3 and σ = 2. Therefore, (X + 3)/2 ~ N(0, 1).

The answer is D.

Summary

If random variable X ~ N(μ, σ²), then its probability density function is:
f(x) = (1 / (sqrt(2π) * σ)) * e^(-(x-μ)²/(2σ²)) (-∞ < x < +∞, μ and σ are constants, σ > 0)
It satisfies:
(1) The graph of f(x) is symmetric about the line x = μ, and its maximum value is 1/(sqrt(2π) * σ) at x = μ.
(2) The value of μ determines the center of the graph. The value of σ determines how steep the peak is; a smaller σ gives a steeper peak, a larger σ gives a flatter peak.

If random variable X ~ N(0, 1), then its probability density function is:
φ(x) = (1 / sqrt(2π)) * e^(-x²/2) (-∞ < x < +∞)

If random variable X ~ N(μ, σ²), then (X - μ)/σ ~ N(0, 1).

Exercise 19.3

Multiple Choice (Choose the one correct answer).
(1) Let random variables X₁ ~ N(μ₁, σ₁²), X₂ ~ N(μ₂, σ₂²). The graphs of their probability density functions are shown in Figure 19.3-3. Which of the following is correct?
(Figure description: Two bell curves, one centered to the right and steeper, the other centered to the left and wider.)
A. μ₁ > μ₂, σ₁ > σ₂
B. μ₁ > μ₂, σ₁ < σ₂
C. μ₁ < μ₂, σ₁ > σ₂
D. μ₁ < μ₂, σ₁ < σ₂

(2) Let random variable X ~ N(μ, σ²), and its probability density function is f(x) = (1 / sqrt(8π)) * e^(-(x-5)²/8). Then the mean and standard deviation of X are:
A. 5, 8
B. 5, 2
C. 8, 5
D. 8, 2

(3) Let random variable X ~ N(2, 9), and P(X > c + 1) = P(X < c - 1). Then c = ( )
A. 1
B. 2
C. 3
D. 4

(4) Let random variable X ~ N(2, 5), and P(X <= 4) = 0.84. Then P(X <= 0) = ( )
A. 0.16
B. 0.32
C. 0.68
D. 0.84

Fill in the blanks.
(1) If random variable X ~ N(5, 1), and P(X > 6) = 0.1, then P(5 <= X <= 6) = ______.
(2) If random variable X ~ N(-4, 2), and P(-5 <= X <= -4) = 0.2, then P(X < -5) = ______.
(3) If random variable X ~ N(6, 3), and P(X <= 3) = 0.4, then P(X >= 9) = ______.
(4) If random variable X ~ N(0, 1), and P(X >= 1) = 0.2, then P(-1 <= X <= 0) = ______.
(5) If random variable X ~ N(μ, σ²), and P(2 < X < 6) = 0.6, P(X >= 6) = 0.2, then μ = ______.

Self-Test 19

If the probability distribution of the discrete random variable X is:

X	-3	-2	0	2	4
p	1/9	2/9	2/9	a	1/9
Find (1) the value of a;
(2) P(X <= 0).

A bag contains 6 white balls and 2 red balls. Now 4 balls are drawn. Let X be the number of white balls drawn. Find:
(1) The probability distribution of X;
(2) P(X <= 3);
(3) The probability distribution of Y = 5X - 4.

There are 9 products: 6 qualified and 3 defective. Choose 6 products. Let X be the number of defective products chosen. Find:
(1) The probability distribution of X;
(2) P(X >= 1);
(3) The probability distribution of Y = 3X + 6.

The probability distribution of random variable X is:

X	4	5	6	7	8
p	1/4	a	5/12	2a	1/12
Find (1) the value of a;
(2) The mathematical expectation E(X) and variance D(X) of X.

Eating mooncakes during the Mid-Autumn Festival is a Chinese tradition. A plate has 12 mooncakes: 5 bean paste, 5 kernel, and 3 blueberry. They all look the same. Choose 3 at random.
(1) Find the probability of getting one of each type.
(2) Let X be the number of bean paste mooncakes chosen. Find the probability distribution of X.
(3) Find the mathematical expectation E(X) and variance D(X) of X.

Let random variable X ~ N(-8, σ²), and P(X <= -10) = 0.35. Find:
(1) P(X >= -6);
(2) P(-10 <= X <= -8);
(3) P(-8 <= X <= -6).


Chapter 20 Statistics

20.1 Basic Concepts in Statistics

I. Population and Sample

Definition 1 In statistics, the entire group of objects we study is called the population. Each object in the population is called an individual. Populations are usually denoted by capital letters X, Y, Z, etc. Individuals in population X are usually denoted by X₁, X₂, X₃, etc. For example, when investigating the shelf life of a batch of bottled milk, the shelf life of the entire batch is the population, and the shelf life of each bottle is an individual.

II. Simple Random Sampling

Definition 2 From a population X, take n individuals X₁, X₂, ..., Xₙ. The vector (X₁, X₂, ..., Xₙ) formed by these n individuals is called a sample from population X. The number of individuals n is called the sample size. In a specific observation or experiment, the actual data (x₁, x₂, ..., xₙ) obtained is called a sample observation value.

Definition 3 If individuals X₁, X₂, ..., Xₙ randomly selected from population X satisfy:
(1) X₁, X₂, ..., Xₙ are mutually independent.
(2) Each Xᵢ (i=1,2,...,n) has the same distribution as X.
Then the obtained sample (X₁, X₂, ..., Xₙ) is called a simple random sample. The method of obtaining a simple random sample is called simple random sampling.

Note 1 Simple random sampling is the foundation of other sampling methods. It is usually used when the differences between individuals in the population are small and the total number of individuals is not large. A common method is drawing lots.
Example: To test the lifespan of 3 randomly chosen light bulbs from 100, simple random sampling can be used.

III. Stratified Sampling

Definition 4 If for the research problem, the population can be divided into several clearly distinct, non-overlapping parts, each part is called a stratum. The method of random sampling within each stratum according to its proportion in the total population is called stratified random sampling, or simply stratified sampling.

Note 2 Samples obtained through stratified sampling are generally more representative.

Example 1 A factory has three workshops. Workshop A has 500 employees, Workshop B has 200, Workshop C has 300. Now, using stratified sampling, 20 people are to be selected for an interview. How many should be selected from each workshop?

Solution The proportions of employees in Workshops A, B, and C are 1/2, 1/5, and 3/10 of the total factory workforce.

Workshop A: 20 × (1/2) = 10 people

Workshop B: 20 × (1/5) = 4 people

Workshop C: 20 × (3/10) = 6 people

Summary

Understand the definitions: population, individual, sample, simple random sample, sample size, sample observation value.

Master two sampling methods: simple random sampling and stratified random sampling.

Exercise 20.1

A factory produced a batch of parts. To check their diameters, 5 parts were taken and their diameters (in cm) measured: 5.1, 4.9, 4.8, 5.0, 5.1. Write down the population, individual, sample size, and sample observation values.

A workshop produced 100 parts in one day. To check quality, 5 parts need to be selected. Which sampling method is better?

Grade 9 in a school has 400 students, of which 160 are girls. To survey the average height of all grade 9 students, a sample of 25 students is planned. Design a reasonable sampling plan.

A company has 900 employees, 400 are male. To select 54 employees according to the male-female ratio, how many males and how many females should be selected?

A unit has 150 staff members: 45 senior, 90 intermediate, 15 junior. Using stratified sampling, a sample of size 30 is to be taken. Find how many people from each level should be selected.

20.2 Numerical Features of Data

I. Maximum/Minimum, Mean, Median, Mode

Definition 1 The maximum and minimum of a set of data.

Definition 2 For a set of numbers x₁, x₂, ..., xₙ, the number (x₁ + x₂ + ... + xₙ)/n is called the mean, denoted by x̄. So x̄ = (x₁ + x₂ + ... + xₙ)/n.

Example 1 Two people, A and B, shoot independently, each firing 5 shots at a target. Their scores (in points) are:
A: 9.1, 8.9, 8, 10, 9.5
B: 9.3, 8.7, 9.2, 9.5, 9.5
Determine who has the higher average shooting level.

Solution Let x̄ and ȳ be the average scores for A and B.
x̄ = (9.1+8.9+8+10+9.5)/5 = 9.1
ȳ = (9.3+8.7+9.2+9.5+9.5)/5 = 9.24
Since x̄ < ȳ, B has the higher average level.

Definition 3 For an odd number of data points, sorted from smallest to largest: x₁, x₂, ..., x_{2n+1}. The median is x_{n+1}. For an even number of data points, sorted: x₁, x₂, ..., x_{2n}. The median is (xₙ + x_{n+1})/2.

Example 2 Find the median and mean x̄ for the following sets.
(1) 10, 5, 8, 6, 9
(2) 3, 7, 4, 8, 5, 9

Solution (1) Sorted: 5, 6, 8, 9, 10. Median = 8. x̄ = (5+6+8+9+10)/5 = 7.6.
(2) Sorted: 3, 4, 5, 7, 8, 9. Median = (5+7)/2 = 6. x̄ = (3+4+5+7+8+9)/6 = 6.

Definition 4 The number that appears most often in a data set is called the mode.

Example 3 A mall surveyed 20 customers on the number of items they bought, with results sorted:
0, 0, 1, 1, 1, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 4, 4, 4, 5, 5
Find the mode.

Solution The numbers 2 and 3 appear most often, each 5 times. So both 2 and 3 are modes.

II. Variance and Standard Deviation

Definition 5 For a set of numbers x₁, x₂, ..., xₙ with mean x̄, the variance is (1/n) Σ_{i=1}^{n} (xᵢ - x̄)², denoted s². So s² = (1/n) Σ (xᵢ - x̄)². The standard deviation is s = √[(1/n) Σ (xᵢ - x̄)²].

Example 4 Find the variance s² for: 4, 6, 5, 6, 4, 5.
Solution The mean x̄ = (4+6+5+6+4+5)/6 = 5. So,
s² = (1/6)[(4-5)²+(6-5)²+(5-5)²+(6-5)²+(4-5)²+(5-5)²] = 2/3.

Summary

Understand definitions of max/min, median, mode.

For numbers x₁,...,xₙ: Mean x̄ = (x₁+...+xₙ)/n, Variance s² = (1/n) Σ (xᵢ - x̄)², Standard deviation s = √s².

Exercise 20.2

Find the maximum, minimum, median, and mode for each set.
(1) 1, -1, 2, 1, -1, 0, -2
(2) -4, 4, 5, -5, 5, 6, 5
(3) 10, 15, 12, 16, 16, 16, 12, 19
(4) 23, 17, 14, 18, 25, 16, 14, 16

Find the mean and variance for each set.
(1) 4, 6, 8, 5, 4, 3, 2
(2) 15, 16, 17, 18, 19, 20, 21
(3) 10, 20, 30, 50, 60, 70
(4) -20, -12, -14, 10, 30, 48

Self-Test 20

To study the height of all second-grade students in a primary school, 20 students were surveyed. The population for this study is ( ).
A. All second-grade students in the school
B. The 20 students surveyed
C. The heights of all second-grade students in the school
D. The heights of the 20 students surveyed

Given data: 2, 3, 3, 3, 4, 4, 5, 6, 6, 6, 6, 7, 7, 7. The median is ______, the mode is ______, the mean is ______, the variance is ______.

A school has 5000 people (teachers and students). Using stratified sampling, a sample of size 100 is taken. It is known that 80 of those sampled are students. Find the number of teachers in the school.

A supermarket received 5 packs of salt each from Factory A and Factory B (same type). Measured weights (in g):

| Factory A | 500 | 501 | 505 | 501 | 499 |
| Factory B | 499 | 500 | 498 | 499 | 500 |

Which factory should the supermarket buy from?


