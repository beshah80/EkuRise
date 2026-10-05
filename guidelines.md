Module 1 Lab — Session 1: The Data Model Value

M1 — C\# 14 Essentials

1 \(Null Safety\), 2 \(Primitives\), 3

\(Encapsulation — all parts\), 3B \(Interface

Contracts\)

Tier 1 \(LO 1.1, 1.2\) \+ Tier 2 \(LO 1.3, 1.4\)

 

Welcome to the TMS Development Team

Assume you are the newest backend engineer on the Training Management System the platform that tracks students, manages course enrollment, calculates grades, and allocates government training grants for CTBE.

The lead architect has handed you a stack of incident reports from the legacy system. The Ministry of Education’s latest audit found a phantom 0.03 Birr balance distributed across 100,000 student accounts a floating-point bug that survived three years of testing. A midnight batch job crashed with **NullReferenceException** because nobody checked whether a student’s region field was empty. The enrollment confirmation pipeline silently overwrote course codes because the data objects had public setters. And on the day final exam grades were published, the server froze not from CPU or memory pressure, but because every database call blocked the thread pool with .Result.

Your job across these three sessions is to rebuild the core logic engine from scratch, fixing each category of defect the right way. By the time you finish, your code will be typesafe, financially precise, immutable where it should be, queryable with LINQ, and non blocking under load.

 

Before You Begin — Create the Project

Every exercise in this workbook runs inside a single .NET 10 console application. Create it once now. Do not skip this step.

Open your terminal and run these commands one at a time: *\# Check your SDK version — must show 10.x*

dotnet--version

*\# Create the Console Application*

dotnet new console-n TmsCore--framework net10.0

*\# Move into the project directory*

cd TmsCore

*\# Open it in VS Code \(or Visual Studio\)*

code .

Once the project opens, find TmsCore.csproj. It should look like this: <**Project** Sdk="Microsoft.NET.Sdk">

< **PropertyGroup**>

<**OutputType**>Exe</**OutputType**>

<**TargetFramework**>net10.0</**TargetFramework**>

<**Nullable**>enable</**Nullable**>

<**ImplicitUsings**>enable</**ImplicitUsings**>

</**PropertyGroup**>

</**Project**>

 

Three settings matter:

 **TargetFramework net10.0** — you are running the latest Long-Term Support

release. Every code example in this workbook assumes .NET 10.

 **Nullable enable** — the compiler will warn you when you try to use a value that

might be null without checking first. This single setting prevents the most common runtime crash in all of software: NullReferenceException.

 **ImplicitUsings enable** — common namespaces like System,

System.Collections.Generic, and System.Linq are available without writing using statements at the top of every file.

Now open Program.cs. It contains one line: Console.WriteLine\("Hello, World\!"\);

Run it:

dotnet run

**What you should see:**

Hello, World\!

If that worked, your environment is ready. Delete the Hello, World\! line — you will replace it with real TMS code in Exercise 1.

**Troubleshooting:**

Problem Likely cause Fix dotnet --version shows 8.x Wrong SDK installed Download .NET 10 from or 9.x dot.net and install it code . does nothing VS Code not in PATH Open VS Code manually,

then File → Open Folder →

select TmsCore

Nullable line is missing Older template version Add from csproj <Nullable>enable</Nullable>

manually inside

<PropertyGroup>

dotnet run says “project You are in the wrong Run cd TmsCore first — you not found” directory need to be inside the

project folder

 

Exercise 1: The First Safety Net \(LO 1.1: Environment \+ Null Safety\) **The situation:** The legacy TMS stored student region data as a plain string with no null checks. When the batch enrollment confirmation system tried to format mailing addresses at 2 AM, it called .ToUpper\(\) on a region that was never assigned. The app crashed with NullReferenceException. Three hundred students received no confirmation email. Nobody noticed until Monday morning.

The fix is not to add null checks everywhere after the fact. The fix is to make the compiler tell you about the problem before the code ever runs. Step 1 — See What the Compiler Catches

Open Program.cs. Write the following code that reproduces the legacy bug: *// This is how the legacy system declared region — no indication it could be empty* string region = **null**; *//* ⚠ *Compiler warning CS8600* Console.WriteLine\(region.ToUpper\(\)\); *//* ⚠ *Compiler warning CS8602* **What you should see:** Two yellow squiggly underlines in your IDE, and if you run dotnet build , two warnings:

warning CS8600: Converting null literal or possible null value to non-nullable type. warning CS8602: Dereference of a possibly null reference.

The code compiles — warnings are not errors by default — but the compiler is telling you: “This will crash at runtime.” The legacy team ignored these warnings. You will not. Step 2 — Fix It Three Ways

Replace the code in Program.cs with these three null-handling patterns. Read the comments — they explain what each operator does:

*// Declare the variable as nullable with '?'*

*// This tells the compiler: "I know this might be null. I accept responsibility."* string? region = **null**;

*// Null-conditional operator '?.' — skip the call if null*

*// If region is null, ToUpper\(\) never executes. No crash.*

string? upperRegion = region?.ToUpper\(\); Console.WriteLine\($"Region \(conditional\): \{upperRegion\}"\); *// Null-coalescing operator '??' — provide a fallback value*

*// If region is null, use "Unassigned" instead.*

string displayRegion = region ?? "Unassigned"; Console.WriteLine\($"Region \(coalesced\): \{displayRegion\}"\); *// Null-coalescing assignment '??=' — assign only if currently null // Useful for lazy initialization.*

region ??= "Addis Ababa";

Console.WriteLine\($"Region \(assigned\): \{region\}"\); Run it:

dotnet run

**What you should see:**

Region \(conditional\):

Region \(coalesced\): Unassigned

Region \(assigned\): Addis Ababa

The first line is blank \(not a crash\) because ?. safely returned null. The second line shows the fallback. The third line shows the lazy assignment.

Step 3 — Declare Your First TMS Variables

Now write the variables that the rest of this workbook will use. Still in Program.cs: string studentName = "Abeba";

string studentId = "STU-001";

int enrollmentCount = 3;

decimal grantAmount = 1999.99m; *// 'm' suffix marks a decimal literal* DateTime enrolledAt = DateTime.UtcNow; string? campusRegion = **null**;

Console.WriteLine\($"Student: \{studentName\} \(\{studentId\}\)"\); Console.WriteLine\($"Courses: \{enrollmentCount\}"\); Console.WriteLine\($"Grant: \{grantAmount:F2\}"\); Console.WriteLine\($"Enrolled: \{enrolledAt:yyyy-MM-dd\}"\); Console.WriteLine\($"Campus: \{campusRegion ?? "Not assigned"\}"\); Run it:

dotnet run

**What you should see:**

Student: Abeba \(STU-001\)

Courses: 3

Grant: 1999.99

Enrolled: 2026-05-09

Campus: Not assigned

The date will show today’s date. The grant shows exactly two decimal places — no drift. Why do professional teams rely on the CLI \(dotnet run, dotnet build\) instead of only clicking buttons in an IDE? Build servers and CI/CD pipelines run your code without a GUI. If your project only builds when you press F5 in Visual Studio, it cannot be deployed automatically. The CLI is the universal interface.

**Troubleshooting:**

Problem Likely cause Fix No compiler warnings on Nullable not enabled Check TmsCore.csproj for string region = null; <Nullable>enable</Nullable>

1999.99m shows a red Typing 1999,99m \(comma\) Use a dot for the decimal underline separator: 1999.99m dotnet run says “top-level You wrapped code in a class In .NET 10 console apps, statements must write directly in Program.cs precede…” — no class wrapper needed

Output says Hello, World\! Did not delete the template Remove before your code line Console.WriteLine\("Hello,

World\!"\); from the top of Program.cs

 

Exercise 2: The Ministry Audit Failure \(LO 1.2: Primitives\) **The situation:** The Ministry of Education relies on the TMS to allocate and report on training grants. During a quarterly audit, an automated script flagged a discrepancy of 0.03 Birr — a phantom balance distributed across 100,000 student accounts. The lead architect traced it to the invoicing module’s grant calculation. The legacy code used double for money. That single type choice created a financial error that survived three years of testing.

Step 1 — See the Bug

Open Program.cs. Add the legacy calculation: *// Legacy implementation — the bug that caused the audit failure* double grantPerStudent = 1999.99;

double totalAllocation = grantPerStudent \* 100\_000; Console.WriteLine\($"Total allocated \(double\): \{totalAllocation\}"\); Run it:

dotnet run

**What you should see:** The output is close to 199999000 but includes a tiny drift — something like 199999000.00000003.

Before reading on: why did 1999.99 \* 100000 produce an impossible fraction? Write your hypothesis, then compare.

The double type is a base-2 \(binary\) floating-point primitive. It cannot accurately represent base-10 fractional numbers like 0.99. The value 1999.99 is stored internally as an approximation. Multiply that approximation by 100,000 and the error becomes visible.

Step 2 — Fix It

Replace the calculation with the decimal type: *// Fixed implementation — exact financial math*

decimal grantPerStudent = 1999.99m;

decimal totalAllocation = grantPerStudent \* 100\_000m; Console.WriteLine\($"Total allocated \(decimal\): \{totalAllocation\}"\); Console.WriteLine\($"Total allocated \(formatted\): \{totalAllocation:F2\}"\); Run it:

dotnet run

**What you should see:**

Total allocated \(decimal\): 199999000.00

Total allocated \(formatted\): 199999000.00

Exact. No drift. The decimal type uses base-10 arithmetic internally, so 0.99 is stored as exactly 0.99.

**Decision rule \(use this in real projects\):**

 Use **decimal** when humans expect exact base-10 results \(money, grants, fees,

GPAs, percentages in reports\).

 Use **double** when the data is inherently approximate \(GPS coordinates, physics,

sensor readings\).

**Trade-off you should know:** decimal is slower than double \(roughly 10x for raw arithmetic\), but for business systems the correctness is worth it. If you ever see money stored as double in a code review, flag it as a bug. **Troubleshooting:**

Problem Likely cause Fix Compile error on Using a comma: 1999,99m Use a dot: 1999.99m 1999.99m

Output shows no drift Display rounding hides the Print with :R format: with double error $"\{totalAllocation:R\}" to

see full precision

100\_000m shows a red Older C\# version The underscore underline separator requires C\# 7\+.

Confirm TargetFramework is net10.0

Still using double Mixed types in calculation Search your method for somewhere double and replace all

with decimal

 

Exercise 3: Pipeline Data Corruption \(LO 1.3 & 1.4: Encapsulation\) **The situation:** In the TMS, once an Enrollment is processed, it passes through multiple middleware services logging, telemetry, notification. The database revealed that 12 students were mysteriously enrolled in NULL. The investigation found that a poorly written logging service was accidentally mutating the data during processing. Because the DTO used public set properties, the compiler could not prevent the mutation. *// Legacy implementation — what the logging service did to the data* **public class** Enrollment

\{

**public** string StudentId \{ **get**; **set**; \} = string.Empty;

**public** string CourseCode \{ **get**; **set**; \} = string.Empty;

**public** DateTime ProcessedAt \{ **get**; **set**; \}

\}

*// Somewhere in the logging pipeline:*

*// enrollment.CourseCode = null; // ← No compiler error. Data silently corrupted.* Step 1 — Understand Why Records Exist

If you change public **string CourseCode \{ get; set; \}** to **public readonly string CourseCode;**, the compiler prevents reassignment after construction but using a readonly field instead of a property breaks encapsulation and prevents future validation.

The right tool is a **record**. Records are designed for immutable data once created, the values cannot be changed. They also give you value-based equality for free: two records with the same data are considered equal, unlike classes where two objects with identical data are still “different” because they occupy different memory locations. **Decision rule \(use this in real projects\):**

 Use record for data that represents a fact after it happens — enrollment events,

audit entries, grade snapshots.

 Use class for entities with lifecycle and state changes — student profiles, course

configurations.

**Trade-off you should know:** Records reduce accidental mutation and simplify equality checks, but classes are better when identity matters \(the same student object being tracked through a workflow\) and when you need mutable state. Step 2 — Create the Domain Models File

Create a file named Models.cs in your TmsCore project. This file will hold all domain types for the rest of the workbook.

Write the EnrollmentRecord as a C\# record: *// Immutable by design — the logging pipeline cannot corrupt this* **public** record EnrollmentRecord\(string StudentId, string CourseCode, DateTime EnrolledAt\); One line. The primary constructor parameters become init-only properties automatically. No one can write enrollment.CourseCode = null after construction — the compiler prevents it.

Test it in Program.cs:

var enrollment = **new** EnrollmentRecord\("STU-001", "CS-401", DateTime.UtcNow\); Console.WriteLine\(enrollment\);

*// Try to mutate it — uncomment this line and see the compiler error: // enrollment.CourseCode = "HACKED"; // ERROR: init-only property // Non-destructive copy — creates a NEW record with one field changed* var corrected = enrollment with \{ CourseCode = "CS-402" \}; Console.WriteLine\(corrected\);

*// Value equality — two records with the same data are equal*

var duplicate = **new** EnrollmentRecord\("STU-001", "CS-401", enrollment.EnrolledAt\); Console.WriteLine\($"Same data? \{enrollment == duplicate\}"\); *// True* Run it:

dotnet run

**What you should see:**

EnrollmentRecord \{ StudentId = STU-001, CourseCode = CS-401, EnrolledAt = ... \} EnrollmentRecord \{ StudentId = STU-001, CourseCode = CS-402, EnrolledAt = ... \} Same data? True

The logging pipeline can no longer corrupt the data. If it needs a modified version, it must create a new record with with \{ \} — the original is untouched. **Troubleshooting:**

Problem Likely cause Fix EnrollmentRecord not File not saved or wrong namespace Save Models.cs. In a found in Program.cs single-project console

app, all files share the

root namespace

automatically

Compiler does not You used class instead of record Replace public class with complain about public record and use a enrollment.CourseCode primary constructor

= "HACKED"

with expression red Using it on a class instead of a record The with expression only underline works on record types

 

Exercise 3 — Part 2: Course Capacity with the field Keyword **The situation:** The Course entity needs to be mutable — courses can change capacity and update titles throughout a semester. But the legacy code let anyone set Capacity = -5 with no complaint. In production, a negative capacity made the enrollment check if \(course.EnrolledCount >= course.Capacity\) pass immediately, blocking all students from a 30-seat course.

Before C\# 14, enforcing validation on a property required a private backing field and seven lines of boilerplate for one simple check:

*// Legacy Pre-C\# 14 Implementation \(Verbose\)*

**public class** Course

\{

**private** int \_capacity; *// Manual backing field*

**public** int Capacity

\{

get => \_capacity;

set

\{

**if** \(value <= 0\)

**throw new** ArgumentOutOfRangeException\("Capacity must be positive."\);

\_capacity = value;

\}

\}

\}

C\# 14 introduced the field keyword — you write validation directly in the setter and the compiler generates the backing field for you.

Add the Course class to Models.cs:

**public class** Course

\{

**public** required string Code \{ **get**; init; \}

**public** required string Title

\{

**get**;

set => field = \!string.IsNullOrWhiteSpace\(value\)

? value

: **throw new** ArgumentException\("Title cannot be empty or whitespace.", nameof\(value\)\);

\}

*// C\# 14 Auto-property validation using 'field'*

**public** int Capacity

\{

**get**;

set => field = value > 0

? value

: **throw new** ArgumentOutOfRangeException\(nameof\(value\), "System constraint: Capacit

y must be greater than zero."\);

\}

**public** int EnrolledCount \{ **get**; **set**; \}

\}

Test it in Program.cs:

var course = **new** Course \{ Code = "CS-401", Title = "Advanced C\#", Capacity = 30 \}; Console.WriteLine\($"Course: \{course.Title\} \(Capacity: \{course.Capacity\}\)"\); *// Invalid capacity — should throw*

**try**

\{

course.Capacity = -5;

\}

**catch** \(ArgumentOutOfRangeException ex\) \{

Console.WriteLine\($"Caught: \{ex.Message\}"\);

\}

*// Invalid title — should throw*

**try**

\{

course.Title = "";

\}

**catch** \(ArgumentException ex\)

\{

Console.WriteLine\($"Caught: \{ex.Message\}"\);

\}

Run it:

dotnet run

**What you should see:**

Course: Advanced C\# \(Capacity: 30\)

Caught: System constraint: Capacity must be greater than zero. \(Parameter 'value'\) Caught: Title cannot be empty or whitespace. \(Parameter 'value'\) The model now rejects bad data at the point of entry. No downstream service can set capacity to -5.

**Decision rule \(use this in real projects\):**

 Use field in properties when you need validation and still want concise code.

 Keep manual backing fields only when you need custom behavior that auto-

property backing cannot express cleanly.

**Trade-off you should know:** field removes boilerplate and the compiler generates the backing field — you never see it. But be aware: property initializers \(like = "Active"\) bypass the setter and assign directly to the backing field. If you need setter validation to run on initialization, assign in the constructor instead.

**Troubleshooting:**

Problem Likely cause Fix field keyword shows as Project not targeting C\# 14 Confirm red underline <TargetFramework>net10.

0</TargetFramework> in csproj. C\# 14 is the

default for .NET 10

Validation never throws You wrote public int Capacity Replace the auto-

\{ get; set; \} without the custom property with the setter validated version shown

above

required keyword error Older C\# version required needs C\# 11\+.

Confirm .NET 10 target

 

Exercise 3 — Part 3: Student Model The enrollment pipeline in Session 2 needs a Student type with validated properties. Add this class to Models.cs now — if you skip it, Exercises 4 through 7B will not compile. **public class** Student

\{

**public** required string Id \{ **get**; init; \}

**public** required string Name

\{

**get**;

set => field = \!string.IsNullOrWhiteSpace\(value\)

? value

: **throw new** ArgumentException\("Name cannot be empty or whitespace.", nameof\(valu

e\)\);

\}

**public** int Age

\{

**get**;

set => field = value **is** >= 16 and <= 100

? value

: **throw new** ArgumentOutOfRangeException\(nameof\(value\), "Age must be between 16 a

nd 100."\);

\}

**public** decimal GPA

\{

**get**;

set => field = value **is** >= 0.0m and <= 4.0m

? value

: **throw new** ArgumentOutOfRangeException\(nameof\(value\), "GPA must be between 0.0

and 4.0."\);

\}

\}

Test it in Program.cs:

var s = **new** Student \{ Id = "S1", Name = "Abeba", Age = 20, GPA = 3.8m \}; Console.WriteLine\($"Student: \{s.Name\}, GPA: \{s.GPA\}"\); *// These should throw — try each one:*

*// new Student \{ Id = "S2", Name = "", Age = 20, GPA = 3.0m \};*

*// new Student \{ Id = "S3", Name = "Test", Age = 12, GPA = 3.0m \}; // new Student \{ Id = "S4", Name = "Test", Age = 20, GPA = 5.0m \};* **What you should see:** Student: Abeba, GPA: 3.8. Uncommenting any invalid line throws the appropriate exception with a clear message.

 

Exercise 3B: Interface Contract Wiring \(LO 1.4: OOP Contracts\) **The situation:** The analytics team needs to generate grade reports for the end-of-semester review. The problem: the TMS has two completely different assessment types quizzes \(graded by correct answers out of total\) and lab assignments \(graded by a weighted formula of functionality and code quality\). The reporting pipeline needs to process both through the same method without knowing which type it is dealing with. This is the problem interfaces solve. An interface is a contract it says “any type that implements me guarantees it can do these things.” The reporting method accepts IGradable and calls CalculateGrade\(\). It does not care whether the object is a quiz, a lab, or something that does not exist yet.

Step 1 — Define the Contract

Add the IGradable interface to Models.cs: **public interface** IGradable

\{

string Title \{ **get**; \}

decimal CalculateGrade\(\);

\}

Step 2 — Implement It on Two Assessment Types Add both classes to Models.cs:

**public class** Quiz : IGradable

\{

**public** required string Title \{ **get**; init; \}

**public** required int CorrectAnswers \{ **get**; init; \}

**public** required int TotalQuestions \{ **get**; init; \}

**public** decimal CalculateGrade\(\)

\{

**if** \(TotalQuestions == 0\) **return** 0m;

**return** \(decimal\)CorrectAnswers / TotalQuestions \* 100m;

\}

\}

**public class** LabAssignment : IGradable

\{

**public** required string Title \{ **get**; init; \}

**public** required decimal FunctionalityScore \{ **get**; init; \}

**public** required decimal CodeQualityScore \{ **get**; init; \}

**public** decimal CalculateGrade\(\)

\{

*// 70% functionality, 30% code quality*

**return** \(FunctionalityScore \* 0.7m\) \+ \(CodeQualityScore \* 0.3m\);

\}

\}

Step 3 — Write the Polymorphic Report

Add this to Program.cs:

void PrintGradeReport\(IEnumerable<IGradable> assessments\) \{

Console.WriteLine\("--- Grade Report ---"\);

**foreach** \(var item **in** assessments\)

\{

Console.WriteLine\($"\{item.Title\}: \{item.CalculateGrade\(\):F2\}%"\);

\}

\}

*// Test it — one array holds two completely different types*

IGradable\[\] cohortAssessments = \[

**new** Quiz \{ Title = "C\# Basics", CorrectAnswers = 18, TotalQuestions = 20 \},

**new** LabAssignment \{ Title = "Registration API", FunctionalityScore = 90m, CodeQualityScore =

85m \}

\];

PrintGradeReport\(cohortAssessments\);

Run it:

dotnet run

**What you should see:**

--- Grade Report ---

C\# Basics: 90.00%

Registration API: 88.50%

The PrintGradeReport method processed both Quiz and LabAssignment without knowing their concrete types. When the team adds PeerReview : IGradable next semester, the report method works without a single line of change. That is the power of programming against a contract instead of a concrete class.

**Troubleshooting:**

Problem Likely cause Fix Cannot implicitly convert Class does not declare it implements Ensure public class type the interface Quiz : IGradable —

the : IGradable is required

Integer division — C\# integer division truncates Cast to decimal first: CorrectAnswers / \(decimal\)CorrectAnsw

TotalQuestions returns 0 ers / TotalQuestions

Method does not accept Parameter type mismatch Use the array IEnumerable<IGradab

le> — arrays implement

IEnumerable

automatically

 

Session 1 Checkpoint

Before moving to Session 2, confirm all four:

☐ dotnet run produces exact decimal output for the grant calculation —

no .00000003 drift

☐ Setting course.Capacity = -5 throws ArgumentOutOfRangeException with a clear

message

☐ Attempting enrollment.CourseCode = "HACKED" on an EnrollmentRecord produces a

compiler error \(init-only\)

☐ PrintGradeReport processes both Quiz and LabAssignment through a single

IGradable parameter

Module 1 Lab Session 2: Query and Classification Field Value

**Module** M1 C\# 14 Essentials **Exercises** 4 \(Guard Clauses & Pattern Matching\), 5 \(Collections & LINQ\)

 

Prerequisites Check Before You Start

Your Models.cs must contain all types from Session 1: EnrollmentRecord, Course, Student, and IGradable \(with Quiz and LabAssignment\). If any are missing, go back to Session 1 and add them the exercises below will not compile without them.

 

Exercise 4: Defeating the “Pyramid of Doom” \(LO 1.6: Pattern Matching & Guards\)

**The situation:** The enrollment team asked for a simple validation: “Before registering a student, check that the student exists, the course exists, and the course is not full.” The previous developer wrote this:

**if** \(student \!= **null**\) \{

**if** \(course \!= **null**\) \{

**if** \(course.Capacity > 0\) \{

*// Success buried three levels deep*

\}

\}

\}

This nested if structure is called the Pyramid of Doom. At 2 AM when production is down, the engineer reading this code cannot tell at a glance what the preconditions are. Every new condition adds another nesting level. By the time you have five checks, the actual logic is indented so far to the right it scrolls off the screen. Guard clauses solve this you check each precondition at the top and exit immediately if it fails. The happy path stays flat and readable.

**Decision rule \(use this in production code\):**

 Use guard clauses for required preconditions \(null checks, invalid ranges,

impossible states\). Throw immediately.

 Use switch expressions for clear classification logic \(status mapping, score bands,

risk levels\).

**Trade-off you should know:** Guard clauses make the happy path easy to read, but you must decide when to throw versus when to return a domain result. Throw for programmer errors and violated preconditions. Return a domain result \(like an error enum or a result object\) when the case is expected in normal workflow. Step 1 Build the Enrollment Service

Create a new file called EnrollmentService.cs: **public class** EnrollmentService

\{

**public** EnrollmentRecord ProcessRegistration\(Student? student, Course? course\)

\{

*//* **TODO** *1: Add guard clauses fail fast if student is null, course is null,*

*//* *or course capacity is zero or negative.*

*//* *Use ArgumentNullException for nulls, InvalidOperationException for full course.*

*// Stuck? Pattern: if \(param is null\) throw new ArgumentNullException\(nameof\(param\)\);*

*//* **TODO** *2: Use a switch expression on student.GPA to classify academic standing:*

*//* *>= 3.5 → "Honors"*

*//* *>= 2.5 → "Good Standing"*

*//* *< 2.5 → "Academic Warning"*

*//* *Print the result: $"\{student.Name\} is in \{standing\}."*

*// Stuck? Pattern: string result = value switch \{ >= X => "Label", ... \};*

*//* **TODO** *3: Return a new EnrollmentRecord with student.Id, course.Code,*

*//* *and DateTime.UtcNow.*

\}

\}

Step 2 Test It

In Program.cs, add:

var service = **new** EnrollmentService\(\);

*// Test 1: Valid registration*

var validStudent = **new** Student \{ Id = "S1", Name = "Abeba", Age = 20, GPA = 3.8m \}; var validCourse = **new** Course \{ Code = "CS-401", Title = "Advanced C\#", Capacity = 30 \}; var result = service.ProcessRegistration\(validStudent, validCourse\); Console.WriteLine\($"Enrolled: \{result.StudentId\} in \{result.CourseCode\}"\); *// Test 2: Null student should throw*

**try**

\{

service.ProcessRegistration\(**null**, validCourse\);

\}

**catch** \(ArgumentNullException ex\)

\{

Console.WriteLine\($"Guard caught: \{ex.ParamName\}"\);

\}

*// Test 3: Full course should throw*

var fullCourse = **new** Course \{ Code = "CS-402", Title = "Full Course", Capacity = 1 \}; fullCourse.EnrolledCount = 1;

**try**

\{

service.ProcessRegistration\(validStudent, fullCourse\);

\}

**catch** \(InvalidOperationException ex\)

\{

Console.WriteLine\($"Business rule: \{ex.Message\}"\);

\}

Run it:

dotnet run

**What you should see:**

 Enrolled: S1 in CS-401 for a valid registration

 Guard caught: student when passing null

 A standing classification like Abeba is in Honors.

 Business rule: ... when the course is full

**Troubleshooting:**

Problem Likely cause Fix Missing return compiler You did not add the Add return new says “not all code paths return statement after EnrollmentRecord\(student.Id, return a value” course.Code, DateTime.UtcNow\); the guard clauses at

the end

Switch expression Missing a fallback arm Add \_ => "Academic Warning" as warning “not all cases the final case covered”

Wrong exception type Using ArgumentException Full course is a runtime state, not for full course instead of a bad argument use

InvalidOperationException InvalidOperationException

EnrollmentRecord Wrong parameter order Constructor expects \(string constructor error StudentId, string CourseCode,

DateTime EnrolledAt\)

Exercise 5: The Analytics Dashboard \(LO 1.5: Collections & LINQ\) **The situation:** The Head of Faculty needs a leaderboard report by end of day. She wants: all Honors students sorted by GPA, the class average, and a breakdown of students by academic standing. The data is in a list. You have LINQ.

From this exercise forward, the solution code is not fully provided for every step. You will implement logic using the TODO comments as guidance the same way a senior developer leaves code review comments for a junior. If you get stuck, each TODO has a “Stuck?” hint.

Step 1 Create the Student Data

In Program.cs, add:

*// C\# 12\+ Collection Expressions the modern way to initialize lists* List<Student> students = \[

**new** Student \{ Id = "S1", Name = "Abeba", Age = 22, GPA = 3.8m \},

**new** Student \{ Id = "S2", Name = "Kidane", Age = 21, GPA = 2.4m \},

**new** Student \{ Id = "S3", Name = "Dawit", Age = 20, GPA = 3.1m \},

**new** Student \{ Id = "S4", Name = "Sara", Age = 23, GPA = 3.9m \},

**new** Student \{ Id = "S5", Name = "Frehiwot", Age = 19, GPA = 2.0m \},

**new** Student \{ Id = "S6", Name = "Yonas", Age = 24, GPA = 3.5m \},

**new** Student \{ Id = "S7", Name = "Meron", Age = 22, GPA = 1.8m \},

**new** Student \{ Id = "S8", Name = "Tesfaye", Age = 21, GPA = 2.9m \}

\];

Step 2 Build the Honors Leaderboard

var leaderboard = students

*//* **TODO** *1: Extract students where GPA is >= 3.5m*

*//* **TODO** *2: Sort the remaining students by GPA descending*

*//* **TODO** *3: Project the result so we only keep the 'Name' string*

*//* **TODO** *4: Materialize the lazy query into a concrete List*

;

Console.WriteLine\($"Found \{leaderboard.Count\} Honors Students:"\); **foreach** \(var name **in** leaderboard\)

\{

Console.WriteLine\($"- \{name\}"\);

\}

**Decision rule \(use this in real projects\):**

 Use LINQ when you need readable data transformations \(filter, sort, project,

aggregate\).

 Materialize with .ToList\(\) only when you need a concrete snapshot now. **Trade-off you should know:** LINQ is usually clearer and less error-prone than manual loops, but deferred execution can surprise you if the source collection changes before enumeration. Call .ToList\(\) when you need the results locked in. Step 3 Class Average

*//* **TODO** *5: Use LINQ to calculate the average GPA across all students. //* *Format it to 2 decimal places using :F2.* decimal averageGpa = \_\_\_\_

*// Stuck? Pattern: students.Average\(s => s.SomeProperty\)*

Console.WriteLine\($"\\nClass Average GPA: \{averageGpa:F2\}"\); Step 4 Group by Academic Standing

*//* **TODO** *6: Use .GroupBy with a switch expression to classify each student. //* *GPA >= 3.5 → "Honors", >= 2.5 → "Good Standing", //* *>= 2.0 → "Probation", < 2.0 → "Academic Warning"* var standingGroups = \_\_\_\_\_

*// Stuck? Pattern: .GroupBy\(s => s.GPA switch \{ >= X => "Label", ... \}\)*

 

Console.WriteLine\("\\n--- Academic Standing Report ---"\); **foreach** \(var group **in** standingGroups\)

\{

Console.WriteLine\($"\\n\{group.Key\} \(\{group.Count\(\)\}\):"\);

**foreach** \(var s **in** group\)

\{

Console.WriteLine\($" \{s.Name\} GPA: \{s.GPA\}"\);

\}

\}

Step 5 Collection Expressions with Spread

*//* **TODO** *7: Use the spread operator \(..\) to merge two arrays and append a value. // Stuck? Pattern: string\[\] combined = \[..array1, ..array2, "extra"\];* string\[\] backendCourses = \["C\#", "ASP.NET Core"\]; string\[\] frontendCourses = \["TypeScript", "Angular"\]; string\[\] allCourses = \_\_\_ // TODO

Console.WriteLine\($"\\nFull curriculum: \{string.Join\(", ", allCourses\)\}"\); Step 6 Run and Verify

dotnet run

**What you should see:**

 Found 3 Honors Students: with Sara, Abeba, and Yonas \(in GPA descending order:

3.9, 3.8, 3.5\).

 Class Average GPA: 2.93

 Academic standing groups: Honors \(3\), Good Standing \(2\), Probation \(2\),

Academic Warning \(1\).

 Full curriculum: C\#, ASP.NET Core, TypeScript, Angular, Capstone

**Troubleshooting:**

Problem Likely cause Fix Wrong order in .OrderByDescending Chain order: .Where\(\) leaderboard placed after .Select → .OrderByDescending\(\) → .Select\(\)

→ .ToList\(\)

Compile error Pipeline does not end IEnumerable has .Count\(\) \(method\), List on with .ToList\(\) has .Count \(property\). Add .ToList\(\) leaderboard.Cou

nt

DivideByZeroExce Empty student list Guard with if \(students.Count > 0\) before ption on calling .Average\(\) average

Spread syntax .. Older C\# version Requires C\# 12\+. Add red underline <LangVersion>latest</LangVersion> to csproj

if needed

Groups appear GroupBy preserves This is correct behavior groups appear in in unexpected encounter order, not the order of first occurrence order alphabetical

 

Session 2 Checkpoint

Before moving to Session 3, confirm all three:

☐ ProcessRegistration\(null, course\) throws ArgumentNullException with the parameter

name student

☐ The honors leaderboard prints Sara, Abeba, Yonas in that order \(GPA descending:

3.9, 3.8, 3.5\)

☐ The academic standing report shows four groups with correct counts: Honors \(3\),

Good Standing \(2\), Probation \(2\), Academic Warning \(1\)



Module 1 Guided Lab Session 3: Async and Resilience Field Value

**Module** M1 C\# 14 Essentials **Exercises** 6 \(Async/Await \+ Part B\), 6B \(Optional\), 7 \(Custom Exceptions\), 7B

\(Integration Report\)

**Assessment** Tier 4 \(LO 1.7, 1.8\) \+ Tier 5 Integration \(Exercise 7B\) **Tiers**

 

Prerequisites Check Before You Start

Your project must have:

 Models.cs with EnrollmentRecord, Course, Student, IGradable, Quiz, LabAssignment

\(from Session 1\)

 EnrollmentService.cs with ProcessRegistration using guard clauses and a switch

expression \(from Session 2\)

If either file is missing or incomplete, go back and finish the previous session first every exercise below depends on those types.

 

Exercise 6: Connection Dropping Under Load \(LO 1.7: Async/Await\) **The situation:** On the first day of registration, 200 students hit the enrollment endpoint simultaneously. The server froze. Not from CPU or memory pressure from thread starvation. The previous developer called .Result on every database query, blocking a thread pool thread for the entire duration of each 300ms database call. With 200 concurrent requests and only ~20 available threads, every thread was locked waiting for I/O. New requests queued. Timeouts cascaded. The registrar rebooted the server twice before calling engineering.

The fix is async/await. Instead of blocking a thread while waiting for the database, you release it back to the pool. When the database responds, the runtime picks up where you left off on any available thread. Same work, same result, but the thread pool never starves.

Step 1 See Thread Starvation in Numbers

In Program.cs, add:

**using** System.Diagnostics;

*// Simulate 5 database calls, each taking 300ms*

*// THE WRONG WAY: Blocking with Thread.Sleep*

var sw = Stopwatch.StartNew\(\);

**for** \(int i = 0; i < 5; i\+\+\)

\{

Thread.Sleep\(300\); *// Thread is HELD for 300ms cannot serve anyone else*

\}

Console.WriteLine\($"Blocking sequential: \{sw.ElapsedMilliseconds\}ms"\); *// ASYNC BUT STILL SEQUENTIAL: Thread released, but calls are one-at-a-time* sw.Restart\(\);

**for** \(int i = 0; i < 5; i\+\+\)

\{

await Task.Delay\(300\); *// Thread released while waiting but still sequential*

\}

Console.WriteLine\($"Async sequential: \{sw.ElapsedMilliseconds\}ms"\); *// THE RIGHT WAY: Async parallel all 5 start simultaneously*

sw.Restart\(\);

var tasks = Enumerable.Range\(0, 5\).Select\(\_ => Task.Delay\(300\)\); await Task.WhenAll\(tasks\);

Console.WriteLine\($"Async parallel: \{sw.ElapsedMilliseconds\}ms"\); Run it:

dotnet run

**What you should see:**

Blocking sequential: ~1500ms

Async sequential: ~1500ms

Async parallel: ~300ms

Three numbers. The first two are similar about 1500ms. The third is about 300ms. That is a 5x improvement. With 200 concurrent users, the difference between a server that responds and a server that crashes.

Step 2 Build the TMS Student Fetcher

Create a method that simulates loading a student from a database: async Task<Student> FetchStudentAsync\(string id\) \{

Console.WriteLine\($" Fetching \{id\}..."\);

await Task.Delay\(300\); *// Simulate database latency*

**return new** Student

\{

Id = id,

Name = $"Student-\{id\}",

Age = 20,

GPA = id **switch**

\{

"S1" => 3.8m,

"S2" => 2.4m,

"S3" => 3.5m,

"S4" => 1.9m,

"S5" => 3.2m,

\_ => 2.5m

\}

\};

\}

Now add a second method that fetches a course:

async Task<Course> FetchCourseAsync\(string code\) \{

Console.WriteLine\($" Fetching course \{code\}..."\);

await Task.Delay\(200\); *// Simulate database latency*

**return new** Course

\{

Code = code,

Title = $"Course-\{code\}",

Capacity = code **switch**

\{

"CRS-101" => 2,

"CRS-201" => 30,

"CRS-301" => 15,

\_ => 25

\}

\};

\}

Step 3 Load in Parallel

sw.Restart\(\);

*// Start all fetches simultaneously students AND courses*

string\[\] studentIds = \["S1", "S2", "S3", "S4", "S5"\]; string\[\] courseCodes = \["CRS-101", "CRS-201", "CRS-301"\]; var studentTasks = studentIds.Select\(id => FetchStudentAsync\(id\)\); var courseTasks = courseCodes.Select\(code => FetchCourseAsync\(code\)\); *// Both arrays load concurrently*

Student\[\] students = await Task.WhenAll\(studentTasks\); Course\[\] courses = await Task.WhenAll\(courseTasks\); Console.WriteLine\($"\\nLoaded \{students.Length\} students and \{courses.Length\} courses in \{sw.E lapsedMilliseconds\}ms"\);

**foreach** \(var s **in** students\)

\{

Console.WriteLine\($" \{s.Name\} GPA: \{s.GPA\}"\);

\}

**What you should see:** All 5 “Fetching…” messages appear almost simultaneously, and the total time is ~300ms \(the time of the slowest single call\), not ~1500ms. **Decision rule \(use this in production code\):**

 Use await for every I/O operation \(database, HTTP, file\). Never call .Result

or .Wait\(\).

 Use Task.WhenAll when you have multiple independent async operations that can

run concurrently.

 Always return Task or Task<T> from async methods. Never use async void except

for event handlers.

**Trade-off you should know:** Task.WhenAll is excellent for independent operations, but if one fails, you only see the first exception by default. For finer error handling per-task, iterate the task array after WhenAll and check each .Exception property. **Troubleshooting:**

Problem Likely cause Fix All times show Using await inside a for Start all tasks first, then await them ~1500ms loop instead of together

Task.WhenAll

AggregateException One of the tasks threw Wrap in try/catch and inspect from Task.WhenAll ex.InnerExceptions for the individual

failures

“Async method You wrote async but never Either add await to an operation or lacks ‘await’ used await inside remove the async keyword operators” warning

Elapsed time much Running in debug mode Run without debugging, or use higher than with breakpoints dotnet run from terminal expected

 

Exercise 6 Part B: The TMS Enrollment Engine

Now connect the pieces. You will load students in parallel, then attempt to enroll each one in a course tracking successes and failures.

var enrollCourse = **new** Course \{ Code = "CRS-101", Title = "C\# Mastery", Capacity = 2 \}; var enrollService = **new** EnrollmentService\(\); var enrollments = **new** List<EnrollmentRecord>\(\); var failures = **new** List<string>\(\);

sw.Restart\(\);

**foreach** \(var student **in** students\)

\{

**try**

\{

var record = enrollService.ProcessRegistration\(student, enrollCourse\);

enrollCourse.EnrolledCount\+\+;

enrollments.Add\(record\);

Console.WriteLine\($" Enrolled: \{student.Name\}"\);

\}

**catch** \(InvalidOperationException ex\)

\{

failures.Add\($"\{student.Name\}: \{ex.Message\}"\);

Console.WriteLine\($" Rejected: \{student.Name\} \{ex.Message\}"\);

\}

\}

**What you should see:** The first 2 students enroll successfully. Students 3–5 are rejected because the course has reached capacity \(Capacity = 2\).

**Troubleshooting:**

Problem Likely cause Fix All students enroll Capacity is too high or Set Capacity = 2 and increment \(none rejected\) EnrolledCount not EnrolledCount\+\+ after each successful

incrementing enrollment

ProcessRegistration Student array contains Ensure FetchStudentAsync always throws nulls returns a valid Student instance ArgumentNullException

Course capacity Guard clause missing in Check that ProcessRegistration throws check not working EnrollmentService when course.EnrolledCount >=

course.Capacity

 

Exercise 6B: Safe Fire-and-Forget \(Optional Not Assessed\) In the TMS, after a successful enrollment, the system should send a confirmation email. But sending an email takes time and should not block the enrollment response. The temptation is to write \_ = SendEmailAsync\(student\); discarding the Task. This is dangerous: if the email server is down, the exception is silently swallowed. The safe pattern wraps the fire-and-forget logic in its own try/catch inside the method: async Task SendConfirmationAsync\(Student student\) \{

**try**

\{

await Task.Delay\(100\); *// Simulate sending email*

Console.WriteLine\($" Email sent to \{student.Name\}"\);

\}

**catch** \(Exception ex\)

\{

*// Log the failure do NOT re-throw.*

*// This is intentional fire-and-forget.*

Console.WriteLine\($" Email failed for \{student.Name\}: \{ex.Message\}"\);

\}

\}

Read this pattern. You will use it in M4 and M7 for background notifications via SignalR.

 

Exercise 7: The Unhelpful Crash \(LO 1.8: Exceptions & Custom Faults\)

**The situation:** When the TMS loses connection to the regional database in Bahir Dar, the framework throws a generic SqlException with a 200-line stack trace. The registrar sees “An error occurred” on screen. The log file shows a raw exception dump that only a database administrator could parse. Nobody knows whether the problem is the network, the database server, or a malformed query.

Custom exceptions let your application speak the domain translating infrastructure failures into messages that operators and logs can act on. When the enrollment pipeline catches a SqlException, it wraps it in a TmsDatabaseException with context: which student, which course, which operation. The log now says something a human can fix. Step 1 Create Two Custom Exceptions

Add to Models.cs \(or a new Exceptions.cs file\): **public class** TmsDatabaseException : Exception \{

**public** string Operation \{ **get**; \}

**public** TmsDatabaseException\(string operation, string message\)

: **base**\(message\)

\{

Operation = operation;

\}

**public** TmsDatabaseException\(string operation, string message, Exception innerException\)

: **base**\(message, innerException\)

\{

Operation = operation;

\}

\}

**public class** CapacityReachedException : InvalidOperationException \{

**public** string CourseCode \{ **get**; \}

**public** CapacityReachedException\(string courseCode\)

: **base**\($"Course \{courseCode\} has reached maximum capacity."\)

\{

CourseCode = courseCode;

\}

**public** CapacityReachedException\(string courseCode, Exception innerException\)

: **base**\($"Course \{courseCode\} has reached maximum capacity.", innerException\)

\{

CourseCode = courseCode;

\}

\}

Step 2 Use Them in the Enrollment Pipeline

Update EnrollmentService.cs. Replace the capacity check InvalidOperationException with the new CapacityReachedException:

**public class** EnrollmentService

\{

**public** EnrollmentRecord ProcessRegistration\(Student? student, Course? course\)

\{

**if** \(student **is null**\)

**throw new** ArgumentNullException\(nameof\(student\)\);

**if** \(course **is null**\)

**throw new** ArgumentNullException\(nameof\(course\)\);

**if** \(course.EnrolledCount >= course.Capacity\)

**throw new** CapacityReachedException\(course.Code\);

string standing = student.GPA **switch**

\{

>= 3.5m => "Honors",

>= 2.5m => "Good Standing",

\_ => "Academic Warning"

\};

Console.WriteLine\($" \{student.Name\} is in \{standing\}."\);

**return new** EnrollmentRecord\(student.Id, course.Code, DateTime.UtcNow\);

\}

\}

Notice that your catch \(InvalidOperationException ex\) block in Exercise 6 Part B still works because CapacityReachedException inherits from InvalidOperationException. The runtime matches the most specific applicable catch. If you want to access the CourseCode property, you can make the catch more specific:

*// This already works \(catches any InvalidOperationException, including CapacityReachedExcepti on\):*

**catch** \(InvalidOperationException ex\)

*// This is more precise \(lets you access ex.CourseCode\):*

**catch** \(CapacityReachedException ex\)

Either version is correct. The precise version is better when you need the domain context.

Step 3 Catch Domain Exceptions

In Program.cs, test catching the custom exception: **try**

\{

var overflowCourse = **new** Course \{ Code = "CRS-999", Title = "Overflow Test", Capacity = 0 \};

enrollService.ProcessRegistration\(

**new** Student \{ Id = "S99", Name = "Test", Age = 20, GPA = 3.0m \},

overflowCourse

\);

\}

**catch** \(CapacityReachedException ex\)

\{

Console.WriteLine\($"\\nDomain exception caught:"\);

Console.WriteLine\($" Course: \{ex.CourseCode\}"\);

Console.WriteLine\($" Message: \{ex.Message\}"\);

\}

Run it:

dotnet run

**What you should see:**

Domain exception caught:

Course: CRS-999

Message: Course CRS-999 has reached maximum capacity.

The calling code knows exactly which course is full and can display a meaningful message to the registrar not a generic “An error occurred.” **Decision rule \(use this in production code\):**

 Use built-in exceptions \(ArgumentNullException, InvalidOperationException\) for

generic precondition failures.

 Create a custom exception only when domain context \(course code, student ID,

campus name\) materially improves log readability or error handling.

 Always pass the original exception as innerException when wrapping

infrastructure failures losing the root cause makes debugging impossible.

**Troubleshooting:**

Problem Likely cause Fix Custom exception not Exception was Ensure you throw new caught by catch thrown as base CapacityReachedException\(course.Code\), \(CapacityReachedException\) Exception type not new Exception\(...\) innerException is null You used the Use the two-parameter constructor

single-parameter when wrapping: new constructor TmsDatabaseException\(op, msg, ex\)

“Type not found” for File not saved or Save the file containing the exceptions. custom exceptions wrong In a single-project app, all files share

namespace the namespace

 

Exercise 7B: The Enrollment Report \(LO 1.5 \+ 1.7 Integration\) **The situation:** Management wants a single console output that summarizes every enrollment run: how many students loaded, how many enrolled successfully, which ones failed and why, how long the whole thing took, and the class average GPA. This is the integration exercise that ties together everything from Exercises 1–7. This exercise builds on the enrollment loop from Exercise 6 Part B. If you skipped Part B, go back and complete it first you need the enrollments and failures lists. Implementation

Add this to the end of your Program.cs, after the enrollment loop from Exercise 6 Part B: *// Stop the timer*

sw.Stop\(\);

*// Calculate class average GPA from loaded students*

decimal classAverage = students.Length > 0

? students.Average\(s => s.GPA\)

: 0m;

*// Print the final report*

Console.WriteLine\("\\n========== ENROLLMENT SUMMARY =========="\); Console.WriteLine\($"Total students loaded: \{students.Length\}"\); Console.WriteLine\($"Successful enrollments: \{enrollments.Count\}"\); Console.WriteLine\($"Failed enrollments: \{failures.Count\}"\); Console.WriteLine\($"Class average GPA: \{classAverage:F2\}"\); Console.WriteLine\($"Total elapsed time: \{sw.ElapsedMilliseconds\}ms"\); **if** \(failures.Count > 0\)

\{

Console.WriteLine\("\\n--- Failure Details ---"\);

**foreach** \(var failure **in** failures\)

\{

Console.WriteLine\($" \{failure\}"\);

\}

\}

Console.WriteLine\("========================================"\); **What you should see:**

========== ENROLLMENT SUMMARY ==========

Total students loaded: 5

Successful enrollments: 2

Failed enrollments: 3

Class average GPA: 2.96

Total elapsed time: ~350ms

--- Failure Details ---

Student-S3: Course CRS-101 has reached maximum capacity.

Student-S4: Course CRS-101 has reached maximum capacity.

Student-S5: Course CRS-101 has reached maximum capacity.

========================================

The elapsed time should be well under 2 seconds, proving that parallel loading worked. The failure messages now use CapacityReachedException from Exercise 7. **Troubleshooting:**

Problem Likely cause Fix stopwatch / sw is Variable declared Make sure Stopwatch.StartNew\(\) is declared before not defined in a different the parallel load, and sw stays in scope through

scope the report

enrollments or Lists not declared Declare var enrollments = new failures is not before the List<EnrollmentRecord>\(\); and var failures = new defined enrollment loop List<string>\(\); before the foreach Elapsed time is Students loaded Use Task.WhenAll as shown in Exercise 6 Step 3 very high sequentially, not

\(~1500ms\+\) in parallel

Class average Students array is Verify FetchStudentAsync returns students with GPA shows 0.00 empty non-zero GPAs

 

Session 3 Checkpoint

Confirm all three:

☐ Loading 5 students \+ enrolling them completes in under 500ms total \(parallel

load working\)

☐ Students 3–5 are rejected with CapacityReachedException messages that include

the course code

☐ Enrollment summary report prints all five categories: loaded, successful, failed,

average GPA, elapsed time

 

Lab Verification Full Module

Run your complete Program.cs one final time with dotnet run. Your output should demonstrate all nine core patterns:

1. **Null-Safe** string? with ?., ??, ??= operators \(Exercise 1\)

2. **Financially Precise** decimal arithmetic with no drift \(Exercise 2\)

3. **Immutable** EnrollmentRecord as a C\# record \(Exercise 3\)

4. **Cleanly Validated** field keyword in Course.Capacity and Student.GPA \(Exercise 3,

Parts 2-3\)

5. **Contract-Driven** IGradable interface with polymorphic PrintGradeReport \(Exercise

3B\)

6. **Readable Under Pressure** Guard clauses and switch expressions \(Exercise 4\)

7. **Declarative** LINQ with GroupBy, collection spread \(Exercise 5\)

8. **Asynchronous** Task.WhenAll for parallel loading \(Exercise 6\)

9. **Resilient** CapacityReachedException with domain context \(Exercise 7\)

If all nine are working, you have built a production-quality domain engine. Every type and pattern reappears in M4 \(Web API\), M5 \(EF Core\), and beyond.

 

Optional Extension: The Modular Audit Path \(Delegates & Lambdas\)

**The situation:** Every time a registration succeeds, the TMS must perform multiple side-effects: log the event to a file, send an SMS to the student, and update the campus leaderboard. If you hardcode these into the EnrollmentService, the class becomes a mess of dependencies that change for reasons unrelated to enrollment logic. Using a delegate \(or Action<T>\) allows the service to announce that an enrollment happened without knowing what happens next. Other modules subscribe to the announcement and react independently.

Implementation \(Complete Challenge\)

No solution code is provided. Synthesize the logic from what you have learned: *//* **TODO** *1: Define a delegate that accepts a Student object \(or use Action<Student>\)* **public class** EnrollmentService

\{

*//* **TODO** *2: Create a property that holds the delegate 'listener'*

**public** void FinalizeEnrollment\(Student s\)

\{

Console.WriteLine\("Persisting to database..."\);

*//* **TODO** *3: Check if the delegate listener is 'not null'*

*//* *and invoke it with the student object.*

\}

\}

*//* **TODO** *4: In Program.cs, create a lambda function that prints: //* *"SMS SENT: Welcome to the TMS, \[StudentName\]\!" //* **TODO** *5: Attach that lambda to the EnrollmentService and call FinalizeEnrollment.* **What you should see:**

Persisting to database...

SMS SENT: Welcome to the TMS, Abeba\!

**Troubleshooting:**

Problem Likely cause Fix NullReferenceException No listener attached Assign your lambda to the when invoking the before calling service’s delegate property first delegate FinalizeEnrollment Nothing prints after Null check is correct but Assign the lambda: “Persisting to lambda was not assigned service.Listener = s => database…” Console.WriteLine\(...\)

 

Lab Completion & Career Checkpoint

By completing Exercises 1–7B, you have resolved all 9 core C\# 14 architectural anti-patterns assessed in M1. Your logic engine is now:

1. **Null-Safe** \(Nullability enabled\)

2. **Financially Precise** \(Decimal Birr\)

3. **Immutable** \(Records\)

4. **Cleanly Validated** \(field keyword\)

5. **Declarative** \(LINQ \+ GroupBy \+ collection spread\)

6. **Readable under pressure** \(Guard clauses \+ switch expressions\)

7. **Asynchronous** \(Non-blocking Task management \+ parallel loading\)

8. **Resilient** \(Custom Exceptions TmsDatabaseException \+ CapacityReachedException\)

9. **Integrated** \(End-to-end enrollment report with Stopwatch timing\)

**Optional enhancement \(not assessed in M1\):** 10. **Decoupled** \(Delegates & Lambdas\)

 

Appendix: Self-Paced Extension Activities

Total self-paced estimate: 90 minutes \(6 activities x 15 minutes\). Recommended use in full-time format: assign Activities 1-3 on Day 1 evening, 4-6 on Day 2 evening. **Activity 1 \(15 min\):** Create a second console app called TmsCore.Tests using dotnet new console . Add a project reference from TmsCore.Tests to TmsCore using dotnet add reference. Verify both projects build with dotnet build. **Activity 2 \(15 min\):** Build a currency converter that reads an amount in ETB, converts to USD and EUR using decimal rates, and prints the result formatted to 2 decimal places. Verify that switching to double produces a different \(incorrect\) result. **Activity 3 \(15 min\):** Take 5 variables from your Session 1 code. Change each to string?. Fix every compiler warning using ?., ??, or ??=. Count how many potential null crashes you just prevented.

**Activity 4 \(15 min\):** Add a public override string ToString\(\) method to every class you built in Session 1. Each should return a formatted summary \(e.g., "Student: Abeba \(STU-001\), GPA: 3.8"\). Verify that Console.WriteLine\(student\) now produces readable output. **Activity 5 \(15 min\):** Using your student list from Session 2, write three new LINQ queries: \(1\) find all students whose name starts with “A”, \(2\) group students by their enrollment year, \(3\) calculate the total grant allocation for honors students only. **Activity 6 \(15 min\):** Take any synchronous method from your Sessions 1-2 code. Convert it to return Task<T>, add await Task.Delay\(100\) to simulate latency, and call it from Program.cs using await. Verify it compiles and runs without warnings.



Module 2 Lab Session 1: Type Safety and Domain Models

Field Value

**Module** M2 TypeScript 6.0 Essentials **Exercises** 1 \(Strict Mode\), 2 \(Interfaces\), 3 \(Type Guards \+ parseStudent\) **After this** A strict-mode TypeScript project with exported Student, Course, and **session you** EnrollmentRecord models, plus a type guard that safely narrows unknown **can show** API data

 

Welcome to the TMS Frontend Team

In M1 you built the TMS domain engine in C\# Student, Course, EnrollmentRecord, Quiz, LabAssignment. The backend now produces JSON responses for every API endpoint. But JSON carries no type information. A response like \{ id: "STU-001", gpa: 3.8 \} could contain anything: the id might arrive as a number after an API migration, or gpa might come back as the string "three point eight" when someone changes the serialization settings. TypeScript is the type layer between the .NET backend and the Angular frontend you will build in M8. It lets you describe the shape of data at compile time so that mismatches surface in your editor not on a live dashboard where Yared is trying to submit final grades.

The existing frontend code uses plain JavaScript with no type safety. Grades are sorted wrong because of string-vs-number confusion \("85" > "9" evaluates to false string comparison compares character codes\). Enrollment status is tracked with boolean flags that allow 27 impossible states. Timestamps are unreliable because the code uses the legacy Date object.

Your task across these three sessions is to build the TypeScript models and utilities that will power the TMS frontend. You will organise them the same way professional Angular projects do one file per model so they are ready to move into the Angular project in M8.

 

Exercise 1: Enforcing the Rules

**The situation:** The existing TMS frontend was set up with strict: false. Because of this, a grade input from an HTML form was treated as a string, and "85" > "9" evaluated to false a student with 85% appeared below a student with 9% on the grade dashboard. Nobody caught the bug for an entire semester.

In M1, you enabled <Nullable>enable</Nullable> in the C\# project to catch null-safety issues at compile time. This is the TypeScript equivalent strict: true forces the compiler to flag type errors before your code ever runs.

**Decision rule \(use this in real projects\):**

 Always enable strict: true in tsconfig.json. This is non-negotiable for production

TypeScript.

 Enable noUncheckedIndexedAccess separately it catches a class of bugs that even

strict: true misses \(array access returning undefined\).

**Trade-off you should know:** Strict mode produces more compiler errors on day one. That is the point. Each error is a bug you would have shipped silently in plain JavaScript. Fix the code, not the config.

Step 1 Create the Project

mkdir tms-client **&&** cd tms-client

mkdir models

npm init-y

npm install typescript @js-temporal/polyfill @types/node--save The models/ folder is where every domain type lives one file per model. This is the same structure you will use in Angular \(M8\), where each model lives in src/app/models/. Starting with clean file organisation now means you will not have to refactor later. Step 2 Initialize the TypeScript Compiler

npx tsc--init

Step 3 Configure Strict Mode

Open the generated tsconfig.json and set the following flags: \{

"compilerOptions": \{

"target": "es2025",

"module": "nodenext",

"strict": **true**,

"noImplicitAny": **true**,

"strictNullChecks": **true**,

"noUncheckedIndexedAccess": **true**

\}

\}

Step 4 Verify

npx tsc--showConfig

**What you should see:** Your resolved configuration with all strict flags enabled. **Troubleshooting:**

Problem Likely cause Fix error TS2307: module not set to nodenext Set "module": "nodenext" and create Cannot find module or no .ts file exists an index.ts file \(even empty\) error TS18003: No No TypeScript files in the Create an index.ts file so the inputs were found project compiler has something to process tsc: command not TypeScript not installed Run npx tsc instead of tsc to use the found locally local install

 

Step 5 Try Transpiling to the ./dist directory and see what is generated npx tsc--outDir dist

 

further reading https://www.typescriptlang.org/tsconfig/

Exercise 2: TMS Domain Models

**The situation:** The C\# backend defines Student, Course, and EnrollmentRecord you built these in M1. The frontend needs matching TypeScript interfaces so the API responses have a known shape. If the backend returns \{ id: string, grade: number \} and the frontend expects \{ id: number, grade: string \}, the app silently breaks at runtime. In C\#, a class must explicitly declare class Quiz : IGradable this is nominal typing. TypeScript uses structural typing: if an object has the right shape \(the right properties with the right types\), it satisfies the interface automatically. The name does not matter the shape is the model.

**Decision rule \(use this in real projects\):**

 Use readonly on ID fields and any property that should not change after creation.

In C\#, readonly is enforced by the CLR at runtime. In TypeScript, it is compile-time only the JavaScript output has no concept of it. The value is in catching mistakes during development.

 Use ? for optional properties. gpa?: number means the property may be undefined.

With strictNullChecks enabled, the compiler forces you to handle the missing case.

**Trade-off you should know:** TypeScript’s readonly vanishes in the compiled JavaScript. If someone modifies the object using raw JavaScript, the protection is gone. This is expected TypeScript guarantees only apply within the TypeScript codebase.

Step 1 Create the Model Files

Each model gets its own file inside the models/ folder. This is how professional Angular projects organise types one file per domain concept. Every interface is exported so other files can import it.

Create models/student.model.ts:

**import** \{ Temporal \} **from** "@js-temporal/polyfill"; **export interface** Student \{

**readonly** id: string;

name: string;

enrollmentDate: Temporal.Instant;

gpa?: number; *// Optional undefined until the student receives a grade*

\}

Create models/course.model.ts:

**import** \{ Temporal \} **from** "@js-temporal/polyfill"; **export interface** Course \{

**readonly** id: string;

title: string;

capacity: number;

startDate?: Temporal.PlainDate;

\}

Create models/enrollment.model.ts:

**import** \{ Temporal \} **from** "@js-temporal/polyfill"; **export interface** EnrollmentRecord \{

**readonly** studentId: string;

**readonly** courseCode: string;

enrolledAt: Temporal.Instant;

\}

Why export? Without it, the interface only exists inside the file that defines it. When you try to import Student from another file \(which you will do in Exercise 3, and again in M8\), TypeScript says “module has no exported member.” Every reusable type must be exported.

Step 2 Test the Constraints

Create a file called index.ts in the project root \(not inside models/\). This is where you will run test code throughout the module:

**import** \{ Temporal \} **from** "@js-temporal/polyfill"; **import** \{ Student \} **from** "./models/student.model"; **const** student: Student = \{

id: "STU-001",

name: "Hana Tadesse",

enrollmentDate: Temporal.Now.instant\(\),

\};

*// Try these what does the compiler say?*

student.id = "STU-999";

console.log\(student.gpa.toFixed\(2\)\);

console.log\(student.gpa?.toFixed\(2\) ?? "Not yet graded"\); Notice the import \{ Student \} from "./models/student.model" this is how you will import models everywhere: in test files, in services, and in Angular components \(M8\). **What you should see:** The readonly assignment error and the null-safety error both appear at compile time. The safe access line compiles and prints Not yet graded. **Troubleshooting:**

Problem Likely cause Fix No error on strict: true not Check your config readonly student.id = set in enforcement requires strict mode "STU-999" tsconfig.json

Temporal is not Missing import Add import \{ Temporal \} from "@js-defined temporal/polyfill"; at the top of

your file

Autocomplete IDE not gpa is number not working on recognizing the | undefined use?. to safely access student.gpa optional it

property

 

Exercise 3: Safe API Parsing \(Type Guards and Unknown\) **The situation:** The TMS API returns JSON. JSON has no type information response.json\(\) gives you unknown. The legacy code cast everything to any, which allowed invalid data to silently corrupt the grade dashboard.

*// Legacy dangerous*

**function** processStudent\(data: any\) \{

console.log\(\`GPA: $\{data.gpa.toFixed\(2\)\}\`\); *// Crashes if gpa is missing or not a number*

\}

In M1, you used guard clauses to validate preconditions \(if \(student is null\) throw new ArgumentNullException\(...\)\). TypeScript’s type guards serve the same purpose they prove to the compiler that a value has the shape you expect before you use it. **Decision rule \(use this in real projects\):**

 Use unknown for any data from outside your application \(API responses, CSV

imports, localStorage, URL parameters\). Never use any.

 Write a type guard \(value is Student\) when you need the compiler to narrow the

type after the check. Use parseStudent \(throw on failure\) when invalid data should stop execution.

**Trade-off you should know:** Type guards return boolean the caller decides what to do with invalid data. Parse functions throw they guarantee a valid result or nothing. Use guards when partial success is acceptable \(filtering an array of mixed data\). Use parse functions when invalid data is a programming error that should fail loudly. Step 1 Understand the Problem

If data is the number 42, what happens when the code reads data.gpa? With any, the compiler says nothing. At runtime, \(42\).gpa returns undefined, and .toFixed\(2\) on undefined crashes.

Step 2 Write a Type Guard

The type guard and parse function belong in the same file as the model they validate. Open models/student.model.ts and add below the interface: **export function** isStudent\(value: unknown\): value **is** Student \{

**return** \(

**typeof** value === "object" &&

value \!== **null** &&

"id" **in** value &&

"name" **in** value &&

**typeof** \(value **as** Record<string, unknown>\).id === "string" &&

**typeof** \(value **as** Record<string, unknown>\).name === "string"

\);

\}

Now in index.ts, import and use it:

**import** \{ Student, isStudent \} **from** "./models/student.model"; **function** processStudent\(raw: unknown\) \{

**if** \(isStudent\(raw\)\) \{

**const** gpaDisplay = raw.gpa?.toFixed\(2\) ?? "Not yet graded";

console.log\(\`Student $\{raw.name\} GPA: $\{gpaDisplay\}\`\);

\} **else** \{

console.error\("Invalid student data received"\);

\}

\}

Step 3 Test It

processStudent\(\{ id: "STU-001", name: "Hana", gpa: 3.7 \}\); *// Prints: Student Hana GPA: 3.70*

processStudent\(42\);

*// Prints: Invalid student data received*

**What you should see:** The first call prints the student data with a formatted GPA. The second call prints the error message no crash.

**Troubleshooting:**

Problem Likely cause Fix isStudent always returns typeof value === "object" "id" in 42 throws a runtime error false comes after the in checks check typeof first Autocomplete does not Return type is boolean The return type must be a type work inside the if block instead of value is Student predicate: value is Student raw.name shows a type Guard function is not Check the function signature the error inside the guard returning value is Student predicate is what triggers block narrowing

 

Exercise 3 Part B: Throwing on Invalid Data

The isStudent guard returns false for bad data the caller decides what to do. Sometimes you need the function to either succeed with a valid Student or blow up with a clear reason so the caller does not have to check. This is the parseStudent pattern the TypeScript equivalent of M1’s CapacityReachedException that tells you exactly what went wrong.

Add parseStudent to models/student.model.ts, below isStudent: **export function** parseStudent\(raw: unknown\): Student \{

**if** \(**typeof** raw \!== "object" || raw === **null**\) \{

**throw new** TypeError\(

\`Expected an object, received $\{raw === **null** ? "null" : **typeof** raw\}\`,

\);

\}

**const** obj = raw **as** Record<string, unknown>;

**if** \(**typeof** obj.id \!== "string"\) \{

**throw new** TypeError\(

\`Expected id to be a string, received $\{ **typeof** obj.id\}\`,

\);

\}

**if** \(**typeof** obj.name \!== "string"\) \{

**throw new** TypeError\(

\`Expected name to be a string, received $\{ **typeof** obj.name\}\`,

\);

\}

**return** \{

id: obj.id,

name: obj.name,

enrollmentDate: Temporal.Now.instant\(\),

\};

\}

Test it in index.ts:

**import** \{ parseStudent \} **from** "./models/student.model"; console.log\(parseStudent\(\{ id: "STU-001", name: "Hana" \}\)\); *// Prints a valid Student object*

parseStudent\(\{ id: 42, name: "Test" \}\);

*// Throws: TypeError: Expected id to be a string, received number* **What you should see:** The first call returns a Student object. The second call throws a TypeError because id is a number, not a string. The error message names the field and the actual type this makes debugging real API mismatches straightforward. **Troubleshooting:**

Problem Likely cause Fix Function returns You wrote return Replace return with throw for the error cases undefined instead of throw

instead of new TypeError\(...\)

throwing

Error message is Missing field name Include the field name and typeof in the not descriptive and actual typeof message string

value

Confusion about Different use cases Use isStudent when the caller handles both parseStudent vs paths. Use parseStudent when invalid data isStudent should stop execution immediately Session 1 What You Can Now Show

Before moving to Session 2, confirm all five:

☐ npx tsc --showConfig shows strict: true with all flags enabled

☐ Your models/ folder contains three separate files: student.model.ts, course.model.ts,

enrollment.model.ts

☐ index.ts successfully imports Student from ./models/student.model the import

compiles without errors

☐ isStudent\(\{ id: "STU-001", name: "Hana" \}\) returns true and narrows to Student inside

an if block

☐ parseStudent\(\{ id: 42, name: "Test" \}\) throws a TypeError with a descriptive message

naming the field and actual type

 

further reading Typescript cheetsheets



Module 2 Lab Session 2: Unions, Generics, and Temporal

Field Value

**Module** M2 TypeScript 6.0 Essentials **Exercises** 4 \(AssessmentItem Union\), 5 \(Enrollment \+ Course Lifecycle\), 6

\(ApiResponse Generic\), 7 \(Temporal Timestamps\)

**After this** Discriminated unions with exhaustive never checks, a generic **session you** ApiResponse<T>, and UTC/timezone timestamps with Temporal.Instant **can show** replacing every use of legacy Date

 

Prerequisites Check Before You Start

Your tms-client project must contain the models/ folder with student.model.ts, course.model.ts , and enrollment.model.ts from Session 1, each with exported interfaces. If any are missing, go back and complete Session 1 the exercises below import those types.

 

Exercise 4: Assessment Types \(Discriminated Unions\) **The situation:** The TMS has two types of graded items: quizzes and lab assignments. In M1, you modelled these with C\# polymorphism an IGradable interface plus Quiz and LabAssignment classes. Each class had different properties but a shared CalculateGrade\(\) method. TypeScript solves the same problem differently with a discriminated union. The idea is familiar. The modelling tool is different:

 **C\#:** One interface \(IGradable\) \+ two classes \(Quiz, LabAssignment\) \+ virtual dispatch

at runtime.

 **TypeScript:** One union type \(AssessmentItem\) \+ a discriminant field \(kind\) \+

compiler-enforced narrowing.

**Decision rule \(use this in real projects\):**

 Use discriminated unions when the data comes from JSON \(which has no class

hierarchy\) and you need exhaustive pattern matching.

 C\# inheritance works well for shared runtime behaviour. TypeScript unions work

well for data classification.

**Trade-off you should know:** Neither approach is universally better. C\# inheritance lets you share implementation across subclasses. TypeScript unions make the compiler verify that you handle every variant.

Step 1 Define the Union

Create a new file models/assessment.model.ts: **export interface** Quiz \{

**readonly** id: string;

kind: "quiz";

title: string;

correctAnswers: number;

totalQuestions: number;

\}

**export interface** LabAssignment \{

**readonly** id: string;

kind: "lab";

title: string;

functionalityScore: number;

codeQualityScore: number;

\}

**export type** AssessmentItem = Quiz | LabAssignment; Step 2 Write the Grade Calculator

Add this function to the same file, below the type:

**export function** calculateGrade\(item: AssessmentItem\): number \{

**switch** \(item.kind\) \{

**case** "quiz":

**return** Math.round\(\(item.correctAnswers / item.totalQuestions\) \* 100\);

**case** "lab":

**return** Math.round\(

item.functionalityScore \* 0.7 \+ item.codeQualityScore \* 0.3,

\);

\}

\}

Step 3 Test It

In index.ts, import and test:

**import** \{ AssessmentItem, calculateGrade \} **from** "./models/assessment.model"; **const** quiz: AssessmentItem = \{

id: "QUIZ-001",

kind: "quiz",

title: "SQL Basics",

correctAnswers: 8,

totalQuestions: 10,

\};

**const** lab: AssessmentItem = \{

id: "LAB-001",

kind: "lab",

title: "REST API Project",

functionalityScore: 85,

codeQualityScore: 90,

\};

console.log\(\`Quiz grade: $\{calculateGrade\(quiz\)\}%\`\); *// 80* console.log\(\`Lab grade: $\{calculateGrade\(lab\)\}%\`\); *// 87 // Verify readonly try this line and check the compiler error:*

quiz.id = "QUIZ-999";

*// ERROR: Cannot assign to 'id' because it is a read-only property* **What you should see:** Quiz prints 80%, lab prints 87%. The quiz.id = "QUIZ-999" line produces a compiler error. Inside the "quiz" case, try accessing item.functionalityScore the compiler reports an error because that property only exists on the "lab" variant. **Troubleshooting:**

Problem Likely cause Fix Property 'correctAnswers' TypeScript narrows This is correct behaviour access does not exist outside the the type inside each variant-specific properties only "quiz" case case block inside the matching case No error on quiz.id = "QUIZ- Missing readonly on Add readonly to id in both Quiz and 999" the id property LabAssignment interfaces Grade calculation is NaN totalQuestions is Add a guard: if \(item.totalQuestions

zero === 0\) return 0;

 

Exercise 5: Enrollment Lifecycle \(State Machine Union\) **The situation:** The TMS tracks enrollment through 5 states: PENDING, APPROVED, ACTIVE, COMPLETED, DROPPED. The previous developer used 5 boolean flags creating 32 possible combinations, of which 27 are impossible.

*// Legacy 27 impossible states allowed*

**interface** EnrollmentBad \{

isPending: boolean;

isApproved: boolean;

isActive: boolean;

isCompleted: boolean;

isDropped: boolean;

\}

Can isPending: true and isDropped: true both be true at the same time? What would the UI show? A discriminated union makes these impossible states unrepresentable. **Decision rule \(use this in real projects\):**

 Use a discriminated union whenever an entity moves through discrete states

with different data per state.

 Each state variant should carry only the data that is meaningful for that state

PENDING needs the student and course IDs for the approval queue, COMPLETED needs the final grade for certificates, DROPPED needs the reason for reporting.

**Trade-off you should know:** Discriminated unions force you to handle every state explicitly. This is more code up front than boolean flags, but it eliminates an entire class of “impossible state” bugs. In Angular \(M8–M9\), you will use the same pattern for component state management.

Step 1 Model the Enrollment Lifecycle

Add this to models/enrollment.model.ts \(below the existing EnrollmentRecord interface\): **export type** EnrollmentStatus =

| \{

status: "PENDING";

requestedAt: Temporal.Instant;

studentId: string;

courseId: string;

\}

| \{ status: "APPROVED"; approvedBy: string; approvedAt: Temporal.Instant \} | \{ status: "ACTIVE"; startDate: Temporal.PlainDate; currentGrade?: number \} | \{ status: "COMPLETED"; finalGrade: number; completedAt: Temporal.Instant \} | \{ status: "DROPPED"; reason: string; droppedAt: Temporal.Instant \};

Step 2 Write the Exhaustive Handler

Add this to the same file:

**export function** describeEnrollment\(enrollment: EnrollmentStatus\): string \{

**switch** \(enrollment.status\) \{

**case** "PENDING":

**return** \`Awaiting approval since $\{enrollment.requestedAt\}\`;

**case** "APPROVED":

**return** \`Approved by $\{enrollment.approvedBy\}\`;

**case** "ACTIVE":

**return** enrollment.currentGrade \!== **undefined**

? \`In progress grade so far: $\{enrollment.currentGrade\}\`

: \`In progress not yet graded\`;

**case** "COMPLETED":

**return** \`Finished with $\{enrollment.finalGrade\}\`;

**case** "DROPPED":

**return** \`Dropped: $\{enrollment.reason\}\`;

**default**: \{

**const** \_check: never = enrollment;

**throw new** Error\(\`Unhandled status: $\{JSON.stringify\(\_check\)\}\`\);

\}

\}

\}

Step 3 Test and Break It

**const** pending: EnrollmentStatus = \{

status: "PENDING",

requestedAt: Temporal.Now.instant\(\),

studentId: "STU-001",

courseId: "CRS-101",

\};

console.log\(describeEnrollment\(pending\)\); *// Awaiting approval since 2026-05-08T...*

**Now break it:** Comment out the DROPPED case and save. The compiler should immediately flag the default line because \{ status: "DROPPED"; ... \} cannot be assigned to never.

**What you should see:** The code compiles cleanly with all 5 cases. Removing any case produces a compile error on the default line naming the missing variant. **Troubleshooting:**

Problem Likely cause Fix The never check does not You did not actually comment The error only appears trigger an error out a case when a union variant

is unhandled

enrollment.approvedBy TypeScript only allows access to This is correct move shows a type error outside state-specific fields inside the the access inside the the APPROVED case matching branch APPROVED case

 

Exercise 5 Part B: Course Lifecycle

You just modelled how an enrollment moves through states. Courses have their own lifecycle a course starts as a draft, gets published for review, goes active when classes begin, and eventually gets archived or cancelled. The same discriminated union pattern applies.

Add the CourseStatus type to models/course.model.ts: **export type** CourseStatus =

| \{ status: "DRAFT"; createdBy: string; createdAt: Temporal.Instant \}

| \{ status: "PUBLISHED"; publishedAt: Temporal.Instant; syllabus: string \}

| \{

status: "ACTIVE";

enrolledCount: number;

startDate: Temporal.PlainDate;

\}

| \{

status: "ARCHIVED";

archivedAt: Temporal.Instant;

finalEnrollmentCount: number;

\}

| \{ status: "CANCELLED"; reason: string; cancelledAt: Temporal.Instant \};

**Now write** **describeCourse** **yourself.** Follow the same exhaustive switch pattern you used for describeEnrollment. Include the never check in the default case. The function signature is:

**export function** describeCourse\(status: CourseStatus\): string \{

*// Your switch goes here. Handle all 5 states.*

*// Each case should return a descriptive string using the state-specific fields.*

*// Include the default/never check.*

\}

Test it in index.ts with an ACTIVE course: **import** \{ CourseStatus, describeCourse \} **from** "./models/course.model"; **const** webDev: CourseStatus = \{

status: "ACTIVE",

enrolledCount: 28,

startDate: Temporal.PlainDate.from\("2026-09-01"\),

\};

console.log\(describeCourse\(webDev\)\);

*// Should print something like: Active with 28 students since 2026-09-01* **Now break it:** Comment out one of your cases. The compiler should flag the default line same pattern you just used in describeEnrollment. **If you are stuck:** Look at your describeEnrollment function the structure is identical. Each case returns a string using the fields available in that specific variant. Exercise 6: Reusable API Response \(Generics\) **The situation:** Every TMS page fetches data from the API. Each fetch has the same lifecycle: loading, then either success with data or error with a message. Writing a separate response type for students, courses, and enrollments violates DRY. In M1, you used C\# generics like List<Student> and Task<Student>. TypeScript generics work the same way you write the wrapper once and fill in the data type when you use it. Look at these two types. What is the same? What is different? *// Before generics repetitive*

**type** StudentResponse =

| \{ status: "loading" \}

| \{ status: "success"; data: Student; fetchedAt: Temporal.Instant \}

| \{ status: "error"; message: string; statusCode: number \};

**type** CourseListResponse =

| \{ status: "loading" \}

| \{ status: "success"; data: Course\[\]; fetchedAt: Temporal.Instant \}

| \{ status: "error"; message: string; statusCode: number \};

The structure is identical. Only the data type changes. A generic eliminates the duplication.

**Decision rule \(use this in real projects\):**

 Use generics when you have a reusable wrapper that works with multiple data

types \(API responses, paginated lists, cache entries\).

 Do NOT use generics when a function only ever works with one type.

sortStudentsByGpa\(students: Student\[\]\) is clearer than sort<T>\(items: T\[\]\) when T is always Student.

**Trade-off you should know:** C\# generics survive to runtime the CLR knows the type and you can use typeof\(T\). TypeScript generics are erased at compile time. The JavaScript output has no concept of T. If you need runtime type checking, combine generics with a type guard the generic handles compile-time safety, the guard handles runtime validation.

Step 1 Define the Generic

Create models/api-response.model.ts:

**import** \{ Temporal \} **from** "@js-temporal/polyfill"; **export type** ApiResponse<T> =

| \{ status: "loading" \}

| \{ status: "success"; data: T; fetchedAt: Temporal.Instant \}

| \{ status: "error"; message: string; statusCode: number \};

Step 2 Write the Renderer

You have now written three exhaustive switch blocks \(calculateGrade, describeEnrollment, describeCourse\). This is the same pattern only the types are generic. Write the function body yourself:

**export function** renderResponse<T>\(

response: ApiResponse<T>,

formatter: \(data: T\) **=>** string,

\): string \{

*//* **TODO***: Handle all three states with a switch on response.status.*

*// "loading" → return "Loading..."*

*// "success" → call the formatter with response.data*

*// "error" → return a string with the statusCode and message*

\}

**If you are stuck:** The switch body follows the exact same shape as describeEnrollment one case per union variant, return a string from each. Step 3 Test with Different Data Types

In index.ts, import and test your renderResponse with two different data types: **import** \{ ApiResponse, renderResponse \} **from** "./models/api-response.model"; **import** \{ Student \} **from** "./models/student.model"; **import** \{ Course \} **from** "./models/course.model"; **const** studentRes: ApiResponse<Student> = \{

status: "success",

data: \{

id: "STU-001",

name: "Dawit Bekele",

enrollmentDate: Temporal.Now.instant\(\),

gpa: 3.4,

\},

fetchedAt: Temporal.Now.instant\(\),

\};

console.log\(

renderResponse\(studentRes, \(s\) **=>** \`$\{s.name\} GPA: $\{s.gpa ?? "N/A"\}\`\),

\);

*// Now test with a different data type*

**const** courseListRes: ApiResponse<Course\[\]> = \{

status: "success",

data: \[

\{

id: "CRS-101",

title: "Web Development Fundamentals",

capacity: 30,

startDate: Temporal.PlainDate.from\("2026-09-01"\),

\},

\],

fetchedAt: Temporal.Now.instant\(\),

\};

console.log\(

renderResponse\(courseListRes, \(courses\) **=>**

courses.map\(\(c\) **=>** c.title\).join\(", "\),

\),

\);

**What you should see:** The first call prints Dawit Bekele GPA: 3.4. The second call prints Web Development Fundamentals. The same renderResponse function handles both ApiResponse<Student> and ApiResponse<Course\[\]> that is the power of the generic. **Troubleshooting:**

Problem Likely cause Fix Type 'Student' is not Missing type Specify the type: ApiResponse<Student>, not assignable to type 'T' parameter on the just ApiResponse

variable

response.data is You are outside data only exists on the success variant inaccessible the "success" access it inside the case "success" block

branch

formatter parameter Generic T not Ensure the formatter’s parameter type type mismatch matching the matches what data holds

data type

 

Exercise 7: Temporal Timestamps \(Dates, Timezones, Durations\) **The situation:** The TMS needs precise timestamps. When was an enrollment approved? When does the assignment deadline expire? How many days until the course starts? The legacy code used JavaScript’s Date object, where timezone math requires manual division by 1000 \* 60 \* 60 and the results vary across browsers. In M1, you used DateTime.UtcNow for timestamps. The Temporal API is the ES2026 standard that replaces JavaScript’s broken Date with immutable, timezone-aware types. Temporal.Instant maps directly to C\#’s DateTime.UtcNow. Temporal.PlainDate maps to DateOnly.

**Decision rule \(use this in real projects\):**

 Use Temporal.Instant for recording when something happened \(enrollment

approval, grade submission\). This is a single point on the UTC timeline.

 Use Temporal.PlainDate for dates without time \(course start date, student

birthday\). No timezone ambiguity.

 Use Temporal.ZonedDateTime only for display converting a UTC instant to a user’s

local wall-clock time.

**Trade-off you should know:** TypeScript 6.0 gives you Temporal types at compile time. Whether Temporal works at runtime depends on your environment Node.js needs the @js-temporal/polyfill package, and browser support is rolling out \(Chrome 144\+, Firefox 139\+\). For the TMS, we include the polyfill and use Temporal as the standard. The Date object is not used in any new code.

Implementation

Add this to index.ts \(the Temporal import should already be at the top from Session 1\): *// 1. Record the exact moment an enrollment is approved \(UTC\)* **const** approvedAt = Temporal.Now.instant\(\); console.log\(\`Approved at \(UTC\): $\{approvedAt\}\`\); *// 2. Display in local timezone*

**const** addisTime = approvedAt.toZonedDateTimeISO\("Africa/Addis\_Ababa"\); **const** londonTime = approvedAt.toZonedDateTimeISO\("Europe/London"\); console.log\(\`Addis: $\{addisTime.toPlainTime\(\)\}\`\); console.log\(\`London: $\{londonTime.toPlainTime\(\)\}\`\); *// Same moment, different wall-clock time*

*// 3. Course start date \(date only, no time\)*

**const** courseStart = Temporal.PlainDate.from\("2026-09-01"\); **const** today = Temporal.Now.plainDateISO\(\); **const** daysUntilStart = today.until\(courseStart\).total\(\{ unit: "days" \}\); console.log\(\`$\{Math.floor\(daysUntilStart\)\} days until course starts\`\); *// 4. Assignment deadline duration*

**const** deadline = Temporal.PlainDate.from\("2026-12-15"\); **const** remaining = today.until\(deadline\);

console.log\(

\`$\{remaining.total\(\{ unit: "days" \}\)\} days until assignment is due\`,

\);

**What you should see:** All four sections print correct values. The Addis time is 3 hours ahead of London time. The “days until” calculations produce reasonable numbers based on today’s date.

**Troubleshooting:**

Problem Likely cause Fix ReferenceError: Temporal is Missing import Add import \{ Temporal \} from "@js-not defined temporal/polyfill"; at the top TypeError: approvedAt is a string, Make sure approvedAt comes approvedAt.toZonedDateTim not a Temporal.Instant from Temporal.Now.instant\(\), not a eISO is not a function string Negative “days until” The target date is in Update the date to a future value

the past

Import error for @js- Package not installed Run npm install @js-temporal/polyfill temporal/polyfill

 

Module 2 What You Can Now Show

Confirm all seven:

☐ models/assessment.model.ts exports Quiz, LabAssignment, AssessmentItem, and

calculateGrade

☐ calculateGrade correctly returns 80 for a quiz with 8/10 and 87 for a lab with 85

functionality \+ 90 quality

☐ Commenting out any case in describeEnrollment or describeCourse produces a

compile error on the never default

☐ You wrote describeCourse yourself it was not copied from the handout

☐ renderResponse works with both ApiResponse<Student> and ApiResponse<Course\[\]>

using the same function

☐ All Temporal timestamps use Temporal.Instant \(not new Date\(\)\) zero instances of

legacy Date in any model file

☐ Temporal.PlainDate.until\(\) correctly calculates days between two dates

 

Module 2 What You Built

Review your tms-client project structure: tms-client/

models/

student.model.ts Student \+ isStudent \+ parseStudent

course.model.ts Course \+ CourseStatus \+ describeCourse

enrollment.model.ts EnrollmentRecord \+ EnrollmentStatus \+ describeEnrollment

assessment.model.ts Quiz, LabAssignment, AssessmentItem \+ calculateGrade

api-response.model.ts ApiResponse<T> \+ renderResponse<T>

index.ts test code

tsconfig.json strict mode enabled

Every interface and function is exported and importable. In M8 you will move these models into the Angular project’s src/app/models/ folder. The patterns discriminated unions for state, generics for API responses, type guards for runtime validation are the same patterns Angular components and services use.



Module 3 Lab: Version Control and Collaborative Tooling

Field Value

**Module** M3 Git and Tooling

**Exercises** Setup \(GitHub \+ push\), 1 \(Modern Syntax\), 2 \(Detached HEAD \+ reflog\), 3

\(Atomic Commits\), 4 \(Merge Conflict\), 5 \(Squash Workflow\)

**After this** A GitHub repository with your real TMS code, safe branching with **session** switch/restore, commit recovery with reflog, atomic conventional commits, **you can** a resolved merge conflict, and a squashed feature branch **show**

 

Welcome to the TMS DevOps Environment In M1 and M2, you built the TMS domain engine in C\# and the typed data layer in TypeScript. But enterprise software is never built alone. Your tms-client project currently lives only on your machine if your hard drive fails, everything is gone. If Yared in Addis Ababa and Sarah in London need to work on the same codebase, there is no way to synchronise.

This lab puts your real code under version control. By the end, your tms-client project lives on GitHub, and you can branch, commit, recover lost work, resolve conflicts, and clean up history the exact workflow every professional development team uses daily.

 

Setup: Put Your TMS Code on GitHub

This is not a practice exercise this is the real setup you will use for the rest of the programme. Every module from here forward works from this repository. Step 1 Create a GitHub Repository

1. Go to github.com/new

2. Repository name: tms-client

3. Description: Training Management System TypeScript data layer

4. Visibility: **Public** \(so your facilitator and teammates can see your work\)

5. Do **not** add a README, .gitignore, or licence you will create these locally

Step 2 Initialise Git in Your M2 Project

Open a terminal in your tms-client folder \(the project you built in M2\): git init

Step 3 Create a .gitignore

Before your first commit, tell Git which files to ignore. Node.js projects generate a node\_modules/ folder containing thousands of dependency files these should never be committed.

Create a file called .gitignore in the project root: node\_modules/

dist/

\*.js

\*.js.map

\*.d.ts

\!src/\*\*/\*.d.ts

Why exclude .js files? Your project is TypeScript the .ts files are the source code. The .js files are compiled output that can be regenerated with npx tsc. Committing compiled output pollutes the repository and causes merge conflicts on files nobody wrote by hand. Step 4 Make Your First Commit

git add .

git status

**What you should see:** Your model files \(models/student.model.ts, models/course.model.ts, etc.\), index.ts, tsconfig.json, package.json, and .gitignore are staged. node\_modules/ is **not** listed the .gitignore is working.

git commit-m "feat: add TMS TypeScript data layer from M2" Step 5 Connect to GitHub and Push

Replace YOUR-USERNAME with your actual GitHub username: git remote add origin https://github.com/YOUR-USERNAME/tms-client.git git branch-M main

git push-u origin main

Step 6 Verify on GitHub

Open your repository in a browser. You should see:

 models/ folder with 5 .model.ts files

 index.ts

 tsconfig.json

 package.json

 .gitignore

 No node\_modules/ folder

**Troubleshooting:**

Problem Likely cause Fix node\_modules/ .gitignore was added after Run git rm -r --cached node\_modules then appears on the first git add commit again GitHub

error: remote You already added a Run git remote set-url origin origin already remote https://github.com/YOUR-exists USERNAME/tms-client.git Authentication GitHub requires a Personal Generate a PAT at failed Access Token \(PAT\) for github.com/settings/tokens and use it

HTTPS as your password

 

Exercise 1: Modern Git Syntax \(switch and restore\) **The situation:** The legacy developers who set up the TMS project learned Git in 2018. They use git checkout for everything switching branches, restoring deleted files, creating new branches. This overloaded command is dangerous and causes accidental data overrides.

Modern Git \(2.23\+\) strictly separates these operations: git switch for branches, git restore for files.

**Think about this:** If you modify models/student.model.ts and then type git

checkout main , Git might either switch your branch to main, or overwrite

student.model.ts with the version from main. Which one will it do? Because

checkout is ambiguous, many developers have accidentally wiped out hours of

work.

The problem is that **the** checkout **command family is ambiguous to humans** because very similar looking commands perform very different actions: git checkout main \# switch branch git checkout -- student.model.ts \# discard changes git checkout main -- student.model.ts \# restore file from main A single missing--or an incorrect argument can change the meaning dramatically. That's what Git's newer commands were designed to avoid.

Step 1 Create a Feature Branch \(the safe way\)

*\# Modern: clear intent "I am switching to a new branch"*

git switch-c feature/course-capacity The old way was git checkout -b feature/course-capacity. Both create and switch. The difference: switch will refuse to run if you have uncommitted changes that would be overwritten. checkout might silently discard them. Step 2 Restore a File \(the safe way\)

Intentionally break one of your model files open models/course.model.ts and delete a few lines. Save the file.

*\# Modern: clear intent "I am restoring a file, not switching branches"* git restore models/course.model.ts

Open the file again your changes are gone, the file is back to its last committed state. With git checkout models/course.model.ts, the same thing happens, but the command looks identical to git checkout some-branch which does something completely different. Step 3 Switch Back

git switch main

 

Exercise 2: The Timeline Panic \(Detached HEAD and reflog\) **Imagine:** A junior developer on your team was trying to view an old version of the architecture. They ran git checkout a1b2c3d to look at an earlier commit. Git placed them in a “Detached HEAD” state. Ignorant of the warning, they spent 3 hours writing the Certificate generation logic, committed it, and then switched back to main. Their commit vanished completely from the visible history. The project manager is panicking because 3 hours of work are seemingly gone.

In Git, commits are never “deleted” automatically, even if they are not attached

to a branch. They float in the background as orphans until the Git Garbage

Collector runs \(default: 30 days\). The tool that finds these invisible commits is git

reflog a hidden journal of every HEAD movement.

Step 1 Simulate the Disaster

*\# Create a normal commit so we have some history*

git commit--allow-empty -m "feat: adding basic student logic" *\# Detach HEAD \(go back one commit without a branch\)*

git switch--detach HEAD~1

*\# Now commit work while detached this is the "lost" work*

git commit--allow-empty -m "feat: certificate generation logic"

*\# Switch back to main the commit vanishes from git log*

git switch main

Check your history:

git log--oneline

The “certificate generation logic” commit is **gone** from the visible history. Step 2 Recover with reflog

*\# View the hidden journal of every HEAD movement*

git reflog

Find the line that says commit: feat: certificate generation logic. Copy the commit hash \(e.g., e7d4f9a\).

*\# Rescue it by creating a new branch pointing at the orphaned commit* git branch rescue-certificate YOUR-COMMIT-HASH *\# Verify it exists*

git switch rescue-certificate

git log--oneline -3

You just recovered work that appeared permanently lost. This is the most common “emergency” question in Git interviews: *“I accidentally lost a commit. How do I get it* *back?”*

*\# Clean up switch back to main*

git switch main

 

Exercise 3: Atomic Commit Literacy

**The situation:** Your teammate submits a Pull Request \(PR\) to the TMS repository. It contains one massive commit: Fix typos, add the Enrollment Capacity System, and update student routing.

The lead architect rejects the PR immediately. Why?

**Monolithic commits** are hard to review and dangerous to revert. If the

Enrollment System crashes production, reverting the commit also reverts the

typo fixes and the routing updates collateral damage.

**Atomic commits** follow a simple rule: one logical change = one commit. Each

commit can be reviewed, reverted, or cherry-picked independently.

You will now create a clean feature branch with atomic commits using the Conventional

Commits format.

From this point, you write the commands yourself. The exact syntax is no longer provided.

*\# INSTRUCTIONS: Create a clean atomic history*

*\#* **TODO** *1: Switch to your 'main' branch*

*\#* **TODO** *2: Create a new branch named 'feature/enrollment-capacity' \#* **TODO** *3: Create an empty file named 'EnrollmentValidator.ts' inside models/ \#* **TODO** *4: Stage the file*

*\#* **TODO** *5: Commit using Conventional Commits format: \#* *feat\(enrollment\): add max capacity validator* **Stuck?** You used git switch -c in Exercise 1. You used git add and git commit -m in the Setup. The commands are the same only the branch name, file name, and commit message change.

 

Exercise 4: The Merge Conflict \(Collaborative Integration\) **The situation:** You and another developer both modified the same line in the TMS assessment logic. When you try to merge their branch into yours, Git stops and demands you resolve the conflict manually.

This is the single most common friction point in team development. Every professional developer must be able to read conflict markers, make a decision, and complete the merge.

Step 1 Set Up the Conflict

*\# Start from main*

git switch main

*\# Create a baseline file*

echo "export const PASS\_THRESHOLD = 60;" > assessment-config.ts git add assessment-config.ts

git commit-m "chore: setup baseline assessment rules" *\# Simulate a colleague's change on a separate branch*

git switch-c feature/strict-assessments

echo "export const PASS\_THRESHOLD = 75;" > assessment-config.ts git commit-am "fix\(assessment\): increase threshold for honors certification" *\# Your parallel change on main*

git switch main

echo "export const PASS\_THRESHOLD = 50;" > assessment-config.ts git commit-am "fix\(assessment\): lower threshold for inclusive grading"

*\# The collision*

git merge feature/strict-assessments

Git prints: Automatic merge failed; fix conflicts and then commit the result. Step 2 Resolve the Conflict

Open assessment-config.ts in your editor. You will see conflict markers: <<<<<<< HEAD

export const PASS\_THRESHOLD = 50;

=======

export const PASS\_THRESHOLD = 75;

>>>>>>> feature/strict-assessments

Make an engineering decision:

 Keep 50 \(your change\)

 Keep 75 \(their change\)

 Choose a different value entirely \(e.g., 65 as a compromise\)

Delete all conflict markers \(<<<<<<<, =======, >>>>>>>\) so only valid TypeScript remains. *\#* **TODO***: Stage the resolved file*

*\#* **TODO***: Complete the merge with git commit \(use the default merge message\)* Step 3 Verify

git log--oneline --graph -5

You should see a merge commit with two parent branches joining together.

 

Exercise 5: The History Cleanup \(Safe Squash Workflow\) **The situation:** Your local commit history on a feature branch is messy:

 chore: started certificate logic

 fix: forgot a semi-colon in PDF generator

 oops: typo in student name

 feat: finish PDF certificate logic

If you push this to the shared repository, the architect will reject it for “polluting the history.” You need to **squash** these four messy commits into a single, professional commit.

**Rebase safety rule:** Never rebase a branch that has already been pushed to a

shared repository. Rebase only local branches before the first push.

Step 1 Simulate the Mess

git commit--allow-empty -m "chore: started certificate logic" git commit--allow-empty -m "fix: forgot semicolon in PDF generator" git commit--allow-empty -m "oops: typo in student name" git commit--allow-empty -m "feat: finish PDF certificate logic" Step 2 Squash with reset –soft

*\# Create a clean branch for the squash*

git switch-c cleanup/certificate-generator *\# Reset the last 4 commits but keep all changes staged*

git reset--soft HEAD~4

*\# Recommit as one clean commit*

git commit-m "feat\(certificate\): implement PDF certificate generation" Why reset --soft instead of interactive rebase? Interactive rebase opens a terminal text editor \(Vim or Nano\) which can trap you if you have not configured it. The reset --soft technique achieves the same result without that risk.

Step 3 Verify

git log--oneline -5

You should see one clean feat\(certificate\) commit where four messy commits used to be. *\# Switch back to main*

git switch main

 

Module 3 What You Can Now Show

☐ Your tms-client repository is live on GitHub with the M2 code, and node\_modules/

is excluded by .gitignore

☐ git reflog shows the rescued “certificate generation logic” commit, and it is

attached to the rescue-certificate branch

☐ You have at least one commit using the feat\(...\) Conventional Commits format

that you wrote yourself

☐ git log --graph --oneline shows a merge commit node where you resolved the

assessment threshold conflict

☐ The cleanup/certificate-generator branch has a single squashed commit replacing

four messy ones

You are ready to collaborate on the TMS repository. Continue to Module 4.

Module 4 Guided Lab Session 1: Request Flow & Visibility

Field Value

**Module** M4 ASP.NET Core 10 Fundamentals **Exercises** 1 \(Middleware Ordering\), 1B \(Custom Request Logging Middleware\)

**After this** Anonymous calls get **401** on a protected TMS route; every response **session you can** carries **X-Correlation-Id**; logs tie request/response to that id **show**

 

Welcome to the Backend Foundation Sprint You keep building the **same** Training Management System \(TMS\) API you will extend it. A previous build’s Program.cs **runs**, but behaviour on a sensitive route is wrong: you will discover what is wrong by **observing HTTP status and tracing the pipeline**, then fix it. Later you add tracing so failures are tied to a single request. In M1 you modelled Student, Course, Enrollment, Assessment, and related ideas. In M4 you host those concerns on the web server. The least goal by this lab is to make a sensitive read behave like a protected API in HTTP, then add **request logging \+ correlation ID** so support and debugging are grounded in evidence.

This session focuses on request pipeline behavior and middleware ordering. You are not implementing a production authentication system in Session 1. The goal is to observe how ASP.NET Core behaves when a protected endpoint participates in the authentication and authorization pipeline.

Later sessions will introduce more complete API functionality and authentication scenarios.

**By the end of Exercise 1:** GET /api/assessments/results rejects anonymous callers with **401** \(no assessment JSON for unauthenticated requests\). You earn that outcome by fixing how the pipeline is wired **without** relying on comments inside the starter file to name the mistake for you.

**By the end of Exercise 1B:** you wrap the pipeline in custom logging, emit **X-Correlation-****Id**, and register UseExceptionHandler early the same posture you will reconnect when Session 3 adds ProblemDetails and Scalar.

Before you begin: the TmsApi project Module 4 uses **one** ASP.NET Core Web API project named **TmsApi**. Create it **once** at the start of Session 1 and keep working in that same folder through Session 3 so exercises stack on each other. If you closed your machine and are **continuing**, open the existing TmsApi directory you already created do **not** run dotnet new again for a second API. Open your terminal and run these commands **one time** when you begin: *\# Check your SDK version: must show 10.x*

dotnet--version

*\# Create the Web API project \(controller-based template; matches later exercises\)* dotnet new webapi-n TmsApi--no-openapi --use-controllers *\# Move into the project directory*

cd TmsApi

*\# Open it in VS Code \(or Visual Studio\)*

code .

 

*\# Add REST Client extension in VS Code \(if you are using Visual Studio code\)*



 

dotnet run

You should see output like:

info: Microsoft.Hosting.Lifetime\[14\]

Now listening on: http://localhost:5xxx

Press Ctrl\+C to stop the server.

 

**Troubleshooting**

Problem Likely cause Fix

dotnet --version shows 8.x or Wrong SDK Install .NET 10 from dot.net 9.x

Port already in use Another process on dotnet run --urls

5000 "http://localhost:5050"

Template produces minimal Default changed in Always pass--use-controllers as API not controllers some SDK builds shown above dotnet: command not found SDK not on PATH Reopen terminal after install; or

fully qualify the path

**Testing HTTP**

After running the api and installing the extension, we can test the endpoints from the TmsApi.http file. Open it and click on the send request button, you will see the response on the right side from headers to body all included.



 

Every URL example below uses http://localhost:5000; replace it with the port your terminal prints.

Exercise 1: The Blind Server \(Middleware Ordering\) **Context:** Assessment outcomes must stay confidential. The route GET /api/assessments/results returns a **placeholder** JSON body for now \(you will replace it with real persistence and DTOs in later modules\). Below is **starter** Program.cs from a prior check-in. Run it, hit the route anonymously, then read the file **top to bottom** and decide what should happen for a protected read versus what actually happens. Predict before you change code

Trace one incoming GET in order: which app.Use... / Map... lines run, and in what

sequence? Where does the response get committed? Write your prediction in one sentence, then run the app and compare.

*// Starter pipeline \(do not assume this order is correct\)*

var builder = WebApplication.CreateBuilder\(args\); var app = builder.Build\(\);

app.UseRouting\(\);

app.MapGet\("/api/assessments/results", \(\) => Results.Ok\(**new** \{

courseCode = "CS-101",

studentId = "S-001",

letterGrade = "A"

\}\)\);

app.UseAuthentication\(\);

app.UseAuthorization\(\);

app.Run\(\);

 

**Reminder: Services and Middleware**

Many ASP.NET Core features require two separate steps:

1. Register the required services during application startup.

2. Add the corresponding middleware to the request pipeline.

 

When troubleshooting, verify both steps. Middleware often depends on

services that were registered earlier in **Program.cs.**

Your task: secure the pipeline

Exercise Assumption

For this exercise, focus on protecting the endpoint and placing authentication and authorization correctly in the request pipeline.

You are not required to implement a production authentication mechanism. The objective is to observe the difference between:

Unprotected endpoint

→ 200 OK

Protected endpoint

→ Request challenged before endpoint execution

The route should require authorization and participate in the authentication/authorization pipeline.

Rewrite Program.cs so GET /api/assessments/results is **not** anonymous: unauthenticated callers get **401**, not the JSON body. You keep the same placeholder response for callers who **are** allowed through.

Work from your template’s usual Program.cs shape; **do not** paste a full solution from elsewhere unless your facilitator green-lights it. Use this scaffold and fill the TODOs from your understanding of the pipeline:

var builder = WebApplication.CreateBuilder\(args\); *// Services: add authentication / authorization services*

var app = builder.Build\(\);

*//* **TODO** *1: Register routing in the pipeline where it belongs for your app. //* **TODO** *2: Register authentication and authorization in the pipeline where your template and fa cilitator expect them for a protected minimal API route.*

*//* **TODO** *3: Map GET /api/assessments/results with the same response body as the starter, but r equire authorization for that route.*

app.Run\(\);

Run / verify / common failure

 **Run:** dotnet run

 **Check:** Browser GET http://localhost:5000/api/assessments/results with **no** login or

bearer token.

 **Expected: 401 Unauthorized** not **200** with JSON, not **404**.

**Why 401?**

A protected endpoint should not execute for anonymous callers.

When authentication and authorization participate correctly in the request pipeline,

ASP.NET Core prevents the endpoint from running and returns 401 Unauthorized

instead.

If you still receive the JSON response body, trace the request path through the

pipeline again and verify that the endpoint is actually protected.

 **Common failure** **404****:** Terminal middleware or branch ordering re-read the

middleware / request-pipeline section in **Module 4 ASP.NET Core Fundamentals** \(your Essentials reading\).

 **Common failure still 200 with body on anonymous GET:** Middleware that

should run for this route is not in the active pipeline path trace order again or ask for a hint in the room.

**Note:**

In custom middleware, RequestDelegate next represents the remainder of the request pipeline.

Calling: **await next\(context\);** passes control to the next middleware or endpoint. When downstream processing completes, execution returns to the current middleware so that additional work \(such as logging\) can be performed.

 

Middleware Reference Card

The following APIs are commonly used inside custom middleware.

// Read the request path

var path = context.Request.Path;

// Read the HTTP method

var method = context.Request.Method;

// Add a response header

context.Response.Headers\["X-Correlation-Id"\] = correlationId; // Read the response status code

var statusCode = context.Response.StatusCode;

// Measure elapsed time

var stopwatch = Stopwatch.StartNew\(\);

// work happens here

stopwatch.Stop\(\);

var elapsedMs = stopwatch.ElapsedMilliseconds;

// Write a structured log entry

\_logger.LogInformation\(

"Request \{Method\} \{Path\}",

context.Request.Method,

context.Request.Path\);

// Pass control to the next middleware

**await \_next\(context\);**

 

Typical middleware flow

// Before next

// - Read request information

// - Add response headers

// - Start timing

**await \_next\(context\);**

// After next

// - Read response information

// - Stop timing

// - Write completion logs

 

Exercise 1B: Custom Request Logging Middleware **Context:** Order is fixed, but you still cannot tie log lines to a single failing request. Implement middleware that logs start/finish and stamps **X-Correlation-Id**. Your Task \(implement, do not transcribe a full solution\)

1. Add RequestLoggingMiddleware.cs with a RequestDelegate next and

ILogger<RequestLoggingMiddleware>.

2. In InvokeAsync:

o Generate a short correlation id \(for example from

Guid.NewGuid\(\).ToString\("N"\)\[..8\]\).

o Set context.Response.Headers\["X-Correlation-Id"\] **before** await next\(context\). o Use Stopwatch to measure elapsed time. o Log one line on entry \(method, path, correlation id\) and one on exit

\(status code, elapsed ms, same id\).

3. In Program.cs, register **in this order**

 app.UseMiddleware<RequestLoggingMiddleware>\(\); first \(outer wrapper\).

 Then UseExceptionHandler, UseHttpsRedirection, UseRouting, UseAuthentication,

UseAuthorization.

 Map GET /api/assessments/results last, still with .RequireAuthorization\(\).

If you are unsure where UseExceptionHandler fits, use **Module 4 ASP.NET Core** **Fundamentals** \(middleware and error-handling sections\) you are wiring the slot now so later exercises can plug in ProblemDetails without reordering everything. Run / verify / common failure

 **Run:** dotnet run

 **Check:** Same anonymous GET as Exercise 1 \(/api/assessments/results\).

 **Expected:** Response still **401**; response headers include **X-Correlation-Id**; **two** log

lines per request sharing the same correlation id.

 **Common failure no logs:** Middleware registered after the endpoint.

 **Common failure no header:** Header set after await next\(context\) \(too late\).

**Why the Header Must Be Added Early?**

Response headers should normally be added before:

**await next\(context\);**

A downstream middleware or endpoint may begin sending the response. Once the

response has started, additional headers may no longer be applied.

Setting X-Correlation-Id before calling next ensures the identifier is available on

successful responses as well as error responses.

 

Why this session ends here

You have only done two things and that is enough for Session 1:

1. **Exercise 1:** A **TMS assessment** route is **not** reachable anonymously \(401 without

auth\); the pipeline matches what you learned in the Essentials reading on request flow.

2. **Exercise 1B:** Every request is **observable**: correlation id in the response and

paired log lines. You also **registered** UseExceptionHandler in the stack you will extend when error bodies become ProblemDetails.

Looking Ahead

Looking Ahead

This session validates the unauthorized path only:

Anonymous caller → 401 Unauthorized Later modules will introduce authentication mechanisms that allow you to verify both outcomes:

Anonymous caller → 401 Unauthorized Authenticated caller → 200 OK

For this now, the objective is to understand middleware ordering, request flow, and request observability.

 

Session 1 checkpoint

Before Session 2, confirm:

☐ Anonymous GET /api/assessments/results returns **401**, not 200 with JSON.

☐ Every response includes **X-Correlation-Id** \(DevTools Network headers, or Scalar

response headers after Session 3\).

☐ Console shows **two** structured lines per request with the **same** correlation id.

☐ Optional: when /scalar/v1 exists, you can repeat the GET in **Try it** and see the

same status and header without a curl wall.

If any check fails, finish Session 1 before moving on.



Lab Support Code: Training Authentication Handler To keep Session 1 focused on middleware ordering rather than production authentication, use the following temporary authentication handler.

Create a file named TrainingAuthHandler.cs. using System.Security.Claims;

using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication; using Microsoft.Extensions.Options;

 

public class TrainingAuthHandler

: AuthenticationHandler<AuthenticationSchemeOptions>

\{

public TrainingAuthHandler\(

IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger,

UrlEncoder encoder\)

: base\(options, logger, encoder\)

\{

\}

 

protected override Task<AuthenticateResult> HandleAuthenticat

eAsync\(\)

\{

if \(\!Request.Headers.ContainsKey\("X-Training-User"\)\) \{

return Task.FromResult\(

AuthenticateResult.Fail\("Missing training user he

ader."\)\);

\}

 

var claims = new\[\]

\{

new Claim\(ClaimTypes.Name, Request.Headers\["X-Trainin

g-User"\]\!\)

\};

 

var identity = new ClaimsIdentity\(claims, Scheme.Name\); var principal = new ClaimsPrincipal\(identity\); var ticket = new AuthenticationTicket\(principal, Scheme.N

ame\);

 

return Task.FromResult\(

AuthenticateResult.Success\(ticket\)\);

\}

\}

This handler treats requests containing an X-Training-User header as authenticated. Requests without that header are treated as unauthenticated. The handler exists only for training purposes and will be replaced by real authentication approaches in later modules.

 

**Register the Training Authentication Scheme**

Register authentication and authorization services during application startup. builder.Services

.AddAuthentication\("Training"\)

.AddScheme<AuthenticationSchemeOptions,

TrainingAuthHandler>\("Training", null\);

 

builder.Services.AddAuthorization\(\);

 

Configure the middleware pipeline:

app.UseAuthentication\(\);

app.UseAuthorization\(\);

Protect the endpoint:

app.MapGet\(...\)

.RequireAuthorization\(\);



Module 4 Lab Session 2: Services Done Right Field Value

**Module** M4 ASP.NET Core 10 Fundamentals **Exercises** 2 \(Captive Dependencies\), 3 \(Options Pattern\), 4 \(Structured

Logging\)

**Assessment Tiers** Tier 2 DI \(LO 4.3, 4.4\) \+ Tier 3 Config \(LO 4.5\) \+ Tier 4 Logging \(LO

4.6\)

 

Prerequisites Check Before You Start

Your TmsApi project must contain:

 Program.cs with the canonical pipeline order from Session 1 UseRouting,

UseAuthentication , UseAuthorization, all before any MapGet

 RequestLoggingMiddleware.cs registered as the **first** middleware in the pipeline

 A working GET /api/assessments/results endpoint that returns 401 Unauthorized for

anonymous calls

 An X-Correlation-Id header on every response

If any of these are missing or incomplete, go back to Session 1. The three exercises in this session depend on a working pipeline and a Correlation ID. Without them you will spend time chasing symptoms instead of learning the lesson.

 

What This Session Is Really About

Session 2 covers three topics that look unrelated until you see them break in production:

1. **DI lifetimes done wrong** \(Exercise 2\) a singleton holds a scoped service, the

database connection pool exhausts at 3 AM

2. **Configuration done wrong** \(Exercise 3\) a required setting is missing, the app

starts fine, then the first user to trigger that code path crashes the request

3. **Logging done wrong** \(Exercise 4\) the logs work, but they are unsearchable strings,

so when something goes wrong you cannot find the one log line that matters

These are the **silent-failure** topics of ASP.NET Core. None of them produce a compile error. None of them fail in development with one user. They fail at 2 AM under real load with a real customer waiting. Your job in Session 2 is to learn the discipline that prevents each of them DI lifetime correctness, validated configuration, and structured logging as one cohesive arc, because in real systems they fail together.

TMS Service Foundation

Before you continue, you need the enrollment service that the next exercises depend on. This is the same TMS domain from M1 now exposed as a service the web layer can inject. Create a new file called EnrollmentService.cs: *// --- The contract ---*

**public interface** IEnrollmentService

\{

Task<EnrollmentRecord> EnrollAsync\(string studentId, string courseCode\);

Task<EnrollmentRecord?> GetByIdAsync\(string id\);

Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync\(\);

Task<bool> DeleteAsync\(string id\);

\}

*// --- The in-memory implementation ---*

**public class** EnrollmentService : IEnrollmentService \{

**private readonly** Dictionary<string, EnrollmentRecord> \_store = **new**\(\);

**private readonly** ILogger<EnrollmentService> \_logger;

**public** EnrollmentService\(ILogger<EnrollmentService> logger\)

\{

\_logger = logger;

\}

**public** Task<EnrollmentRecord> EnrollAsync\(string studentId, string courseCode\)

\{

var id = Guid.NewGuid\(\).ToString\("N"\)\[..8\];

var record = **new** EnrollmentRecord\(id, studentId, courseCode, DateTime.UtcNow\);

\_store\[id\] = record;

\_logger.LogInformation\(

"Enrolled \{StudentId\} in \{CourseCode\} record \{EnrollmentId\}", studentId, courseCode, id\);

**return** Task.FromResult\(record\);

\}

**public** Task<EnrollmentRecord?> GetByIdAsync\(string id\)

\{

\_store.TryGetValue\(id, **out** var record\);

**return** Task.FromResult\(record\);

\}

**public** Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync\(\)

\{

IReadOnlyList<EnrollmentRecord> all = \_store.Values.ToList\(\);

**return** Task.FromResult\(all\);

\}

**public** Task<bool> DeleteAsync\(string id\)

\{

var removed = \_store.Remove\(id\);

**return** Task.FromResult\(removed\);

\}

\}

*// --- The data shape ---*

**public** record EnrollmentRecord\(

string Id,

string StudentId,

string CourseCode,

DateTime EnrolledAt\);

Do **not** register it in Program.cs yet Exercise 2 will walk you through registration and lifetime decisions.

**Troubleshooting**

 EnrollmentRecord is a record type \(immutable by default\). If you need to review

records, revisit M1 Exercise 3.

 The Task.FromResult calls simulate async I/O. In M5 these become real database

calls with EF Core.

 

Exercise 2: The Memory Leak \(Captive Dependencies\) **Context:** The TMS has a background worker that recalculates scholarships every hour. The server crashed with an OutOfMemoryException. The investigation traced the crash to a **Scoped** service \(IEnrollmentService\) being held by a **Singleton** \(EnrollmentWorker\) a **captive dependency**.

Quick reference the three lifetimes

 **Transient:** New instance every time it is resolved.

 **Scoped:** One instance per HTTP request; disposed when the request ends.

 **Singleton:** One instance for the entire application lifetime.

If a Scoped service \(for example something that should track **this request’s** enrollments\) is captured inside a Singleton, it can live across requests. Under load, connections or in-memory state leak or go **stale** another request may see the wrong student’s data. **Prerequisite \(read once\):** Many HTTP requests can be **in flight at the same time**. Each request should get its **own** scoped services. A singleton lives forever it must **not** keep a reference to a scoped instance created for an old request. First, make the failure visible

**Step A Buggy registration \(temporary\):** Implement EnrollmentWorker so its constructor takes **IEnrollmentService** **directly** \(not IServiceScopeFactory yet\). Register: builder.Services.AddSingleton<EnrollmentWorker>\(\); builder.Services.AddScoped<IEnrollmentService, EnrollmentService>\(\); Add **host validation** so the container catches illegal lifetime wiring early: builder.Host.UseDefaultServiceProvider\(options => \{

options.ValidateScopes = **true**;

options.ValidateOnBuild = **true**;

\}\);

**Run:** dotnet run

**Expected \(the “good” failure\):** The app throws at startup or on first resolve with an error similar to:

Cannot consume scoped service 'IEnrollmentService' from singleton 'EnrollmentWorker'. That message IS the captive dependency detector working. Read it carefully it names the two lifetimes that clash.

**Step B Confirm the failure:** If the app refused to start in Step A, that IS the expected failure. Read the error message carefully it names both lifetimes. Skip ahead to “Your Task” below.

If the app somehow started \(unlikely with ValidateOnBuild = true\), add this smoke-test route to Program.cs and run concurrent requests to expose the bug: app.MapGet\("/api/enrollments/worker-smoke", \(EnrollmentWorker worker\) => \{

worker.ProcessBatch\(\);

**return** Results.Ok\("processed"\);

\}\);

From **PowerShell** \(replace the base URL with yours from dotnet run\): $base = "http://localhost:5000" *\# or https://localhost:7xxx match your terminal* 1..15 | ForEach-Object-Parallel \{

Invoke-WebRequest-Uri "$using:base/api/enrollments/worker-smoke"-UseBasicParsing | Out

-Null

\} -ThrottleLimit 15

**Expected:** Exceptions or inconsistent data under parallel calls the scoped service is being shared across requests.

**Troubleshooting:** If nothing fails, confirm ValidateScopes = true is set. If the app refuses to start after the singleton registration, that IS the expected failure proceed to the fix below.

Your Task: Fix the Captive Dependency

Inject **IServiceScopeFactory** into the singleton and create a **short-lived scope** each time the worker runs. Resolve IEnrollmentService **from that scope** only. *// These registrations are given do NOT change them:*

builder.Services.AddSingleton<EnrollmentWorker>\(\); builder.Services.AddScoped<IEnrollmentService, EnrollmentService>\(\); *// Inside EnrollmentWorker.cs...*

**public class** EnrollmentWorker\(IServiceScopeFactory scopeFactory\) \{

**public** void ProcessBatch\(\)

\{

*//* **TODO** *2: Create a short-lived scope using the injected factory.*

*// Stuck? using var scope = factory.CreateScope\(\);*

*//* **TODO** *3: Resolve the scoped service from the new scope's provider.*

*// Stuck? var svc = scope.ServiceProvider.GetRequiredService<IEnrollmentService>\(\);*

*//* **TODO** *4: Use the service, then let the 'using' block dispose the scope*

*//* *and its scoped services automatically.*

\}

\}

Run / Call / Expected / Common failure

 **Run:** dotnet run after implementing IServiceScopeFactory.

 **Call:** Same as Step A or B startup should succeed; if you have worker-smoke, run

the PowerShell parallel block again.

 **Expected after fix:** No Cannot consume scoped service… at startup; under parallel

calls, behavior is stable \(no cross-request stale state for scoped work done **inside** ProcessBatch\).

 **Common failure:** Forgetting using on the scope scoped services never dispose.

 **Common failure:** Resolving IEnrollmentService from the **root** app.Services inside

the singleton still wrong. Always CreateScope\(\) first.

Exercise 3: The Silent Crash \(Options Pattern\) **Context:** The TMS relies on an external payment gateway for tuition processing \(measured in Ethiopian Birr\). The appsettings.json is missing the GatewayUrl key. Because the previous developer used IConfiguration\["GatewayUrl"\] inline during checkout, the application runs perfectly until a student tries to pay, throwing a massive server error.

\[\!CAUTION\] **Think about this before coding:** If an application is missing a

critical configuration setting, is it better for it to crash entirely upon startup, or

crash later when a user tries to use the specific feature?

Your Task: Validate Configuration at Startup

Build a strongly-typed options class and wire it so the app refuses to start with invalid configuration.

*//* **TODO** *1: Create a class called PaymentOptions with two properties: //* *- GatewayUrl \(string, required use \[Required\] attribute\) //* *- MaxDepositBirr \(decimal, range 100-100000 use \[Range\] attribute\) // Stuck? public class PaymentOptions \{ \[Required\] public required string GatewayUrl \{ get; init; \}*

*... \}*

 

*//* **TODO** *2: In Program.cs, bind PaymentOptions to the "Payments" section of appsettings.json //* *and enable startup validation.*

*// Stuck? builder.Services.AddOptions<PaymentOptions>\(\)*

*//* *.BindConfiguration\("Payments"\)*

*//* *.ValidateDataAnnotations\(\)*

*//* *.ValidateOnStart\(\);*

 

*//* **TODO** *3: Test it delete the "Payments" section from appsettings.json and run the app. //* *What error do you see? Does the app start or crash immediately?* Run / Call / Expected / Common failure

 **Run:** dotnet run **after** removing or commenting out the entire "Payments" section

in appsettings.json \(or removing GatewayUrl only, if your validators require it\).

 **Call:** \(none failure happens at startup\)

 **Expected:** Process **does not stay running**; console shows something like:

Microsoft.Extensions.Options.OptionsValidationException: DataAnnotation validation failed for 'PaymentOptions': 'The GatewayUrl field is required.'

 **Common failure app starts anyway:** ValidateOnStart\(\) is missing, or options are

not bound to "Payments", or the section name in JSON does not match BindConfiguration\("Payments"\).

 

Exercise 4: The Unsearchable Logs \(Structured Logging\) **Context:** A student complained that their grade was not saved. The IT team searched Application Insights for logs, but could not filter by the student’s ID because the logs were written as raw concatenated strings.

The difference between searchable and unsearchable logs comes down to one habit: *// BAD one concatenated blob, not queryable*

logger.LogInformation\("Enrolling student " \+ studentId \+ " in course " \+ course\); *// GOOD StudentId and Course become queryable properties in any log aggregator* logger.LogInformation\("Enrolling student \{StudentId\} in course \{Course\}", studentId, course\); Your Task: Audit and Fix EnrollmentService Logging Open EnrollmentService.cs \(the file you created at the top of this session\). The EnrollAsync method already uses structured logging. Now apply the same discipline across the service and add proper **log levels**.

1. Add a LogWarning to EnrollAsync when the same student enrolls in the same

course twice \(duplicate check before inserting\):

**public** Task<EnrollmentRecord> EnrollAsync\(string studentId, string courseCode\) \{

*// Check for duplicate enrollment*

var existing = \_store.Values

.FirstOrDefault\(e => e.StudentId == studentId && e.CourseCode == courseCode\);

**if** \(existing **is** not **null**\)

\{

\_logger.LogWarning\(

"Duplicate enrollment attempt \{StudentId\} already in \{CourseCode\} \(record \{EnrollmentI

d\}\)",

studentId, courseCode, existing.Id\);

**return** Task.FromResult\(existing\);

\}

var id = Guid.NewGuid\(\).ToString\("N"\)\[..8\];

var record = **new** EnrollmentRecord\(id, studentId, courseCode, DateTime.UtcNow\);

\_store\[id\] = record;

\_logger.LogInformation\(

"Enrolled \{StudentId\} in \{CourseCode\} record \{EnrollmentId\}",

studentId, courseCode, id\);

**return** Task.FromResult\(record\);

\}

2. Add a LogWarning to GetByIdAsync when the requested record does not exist:

**public** Task<EnrollmentRecord?> GetByIdAsync\(string id\) \{

\_store.TryGetValue\(id, **out** var record\);

**if** \(record **is null**\)

\{

\_logger.LogWarning\("Enrollment \{EnrollmentId\} not found", id\);

\}

**return** Task.FromResult\(record\);

\}

3. Add a LogInformation to DeleteAsync:

**public** Task<bool> DeleteAsync\(string id\)

\{

var removed = \_store.Remove\(id\);

**if** \(removed\)

\_logger.LogInformation\("Deleted enrollment \{EnrollmentId\}", id\);

**else**

\_logger.LogWarning\("Delete failed enrollment \{EnrollmentId\} not found", id\);

**return** Task.FromResult\(removed\);

\}

**Log level decision rules** \(use these throughout the TMS\):

 **Information** a business event completed successfully \(enrollment created,

deleted\)

 **Warning** something unexpected but recoverable happened \(duplicate attempt,

record not found\)

 **Error** an operation failed and needs attention \(exceptions, data corruption\)

Run / Call / Expected / Common failure

 **Run:** dotnet run, then call POST /api/enrollments twice with the same student and

course \(you will wire this endpoint in Session 3 / Exercise 5\). For now you can verify by calling the methods directly from a temporary test endpoint, or wait until Session 3.

 **Call:** Watch the **console** output.

 **Expected:** First call logs \[Information\] Enrolled S-001 in CS-101. Second call logs

\[Warning\] Duplicate enrollment attempt. A GET for a nonexistent ID logs \[Warning\] Enrollment xyz not found.

 **Common failure:** Still seeing concatenated strings you used "Enrolling " \+

studentId instead of "Enrolling \{StudentId\}", studentId.

 **Common failure:** All logs show \[Information\] you forgot to use LogWarning for the

edge cases.

 

Why this session ends here

Session 2 covered three failures that all look the same in production logs: “the app worked yesterday and now it doesn’t.” Every senior engineer has fought each one, often more than once.

 A singleton holding a scoped service is a **lifetime mismatch**. Fix:

IServiceScopeFactory and a using scope. Detect early with ValidateScopes = true and ValidateOnBuild = true.

 A missing required configuration value is a **deployment hazard**. Fix: strongly-

typed options with \[Required\] / \[Range\] and ValidateOnStart\(\). Detect at process start, never at user-facing failure time.

 A concatenated log string is an **observability hazard**. Fix: named template

placeholders \{StudentId\}, \{CourseCode\}, with appropriate log levels. Detect by searching for a specific student in the aggregator and seeing zero hits when there should be many.

In Session 3 you build the API surface that makes all of this visible from the outside controllers, the error envelope clients consume, and the environment toggle that decides what diagnostic surface is exposed in Dev versus Production. The capstone refactor at the end of Session 3 will probe whether you can change all three pillars \(DI, config, logging\) under time pressure that is what makes today’s work matter.

 

Session 2 Checkpoint

Before moving to Session 3, confirm all four:

☐ Registering EnrollmentWorker as singleton with a constructor taking

IEnrollmentService directly causes startup to fail with a “Cannot consume scoped service” error \(proves ValidateScopes is on\)

☐ After the IServiceScopeFactory fix, the app starts and 15 concurrent calls to worker-

smoke complete with no captive-dependency exceptions

☐ Deleting Payments:GatewayUrl from appsettings.json causes the app to crash at

startup with a clear OptionsValidationException message not a 500 on first request

☐ A second enrollment of the same student into the same course logs \[Warning\]

Duplicate enrollment attempt with \{StudentId\} and \{CourseCode\} as queryable structured properties not a concatenated string

Module 4 Lab Session 3: API Surface & Production Readiness

Field Value

**Module** M4 ASP.NET Core 10 Fundamentals **Exercises** 5 \(Controllers \+ CRUD\), 6 \(ProblemDetails\), 7 \(Dev/Prod Toggle\);

optional instructor-led integration exercise after Lab Verification

**After this** CRUD on /api/enrollments, ProblemDetails errors, Dev vs Prod surface **session you can** \(Scalar vs hidden\); items 1–11 on the Lab Verification list green **show**

 

Prerequisites Check Before You Start

Your TmsApi project must contain:

 The pipeline and Correlation ID middleware from Session 1 pipeline order correct,

X-Correlation-Id on every response

 EnrollmentService.cs from Session 2 with structured logging and proper log levels

\(Information / Warning\) on all four methods

 EnrollmentWorker injecting IServiceScopeFactory, not IEnrollmentService directly

 PaymentOptions registered with BindConfiguration\("Payments"\) and ValidateOnStart\(\)

so missing config crashes at startup

 builder.Services.AddControllers\(\); in the **builder** section of Program.cs

 app.MapControllers\(\); in the **middleware** section \(after UseAuthentication /

UseAuthorization, before app.Run\(\)\)

If any of these are missing, go back. Session 3 wires the public-facing API on top of all of them; the closing Session 3 exercise your instructor runs will assume this foundation is solid.

 

What This Session Is Really About

So far the API runs but has no public surface. You have a pipeline, services, configuration, and logging but a frontend cannot do anything with that yet. Session 3 builds three things in that order:

1. **The surface itself** \(Exercise 5\) controllers that expose /api/enrollments with the

four HTTP verbs every TMS client needs, returning the right status codes and the Location header REST clients expect

2. **The error envelope** \(Exercise 6\) a single, structured JSON shape \(RFC 9457

ProblemDetails\) that every error response uses, so the Angular frontend in M8 can write one error handler instead of one per failure mode

**The environment posture** \(Exercise 7\) Scalar API explorer in Development, hidden in Production; stack traces hidden in Production, replaced by ProblemDetails; the same code, two different surfaces depending on where it runs.

 

Exercise 5: The Enrollment API \(Controllers with Real CRUD\) **Context:** Now that the infrastructure is stable, you must expose HTTP endpoints for the Angular frontend. The TMS uses MVC Controllers each resource gets its own controller class with a constructor-injected service.

Before you code

1. Confirm builder.Services.AddControllers\(\); is in the **builder** section of Program.cs

\(added in Session 2 prerequisites\).

2. Confirm app.MapControllers\(\); is in the **middleware** section \(after auth, before

app.Run\(\)\).

3. Create a Controllers folder in your project.

Part A: GET Endpoints

Create a new file called Controllers/EnrollmentsController.cs: **using** Microsoft.AspNetCore.Mvc;

\[ApiController\]

\[Route\("api/enrollments"\)\]

**public class** EnrollmentsController\(IEnrollmentService enrollmentService\) : ControllerBase \{

*// GET /api/enrollments returns all enrollment records*

\[HttpGet\]

**public** async Task<IActionResult> GetAll\(\)

\{

var enrollments = await enrollmentService.GetAllAsync\(\);

**return** Ok\(enrollments\);

\}

*// GET /api/enrollments/\{id\} returns one or 404*

\[HttpGet\("\{id\}"\)\]

**public** async Task<IActionResult> GetById\(string id\)

\{

var record = await enrollmentService.GetByIdAsync\(id\);

**return** record **is** not **null** ? Ok\(record\) : NotFound\(\);

\}

\}

Test it: dotnet run, then call GET /api/enrollments. You should see 200 OK with an empty array \(no enrollments yet\).

Part B: POST with 201 \+ Location

Add a POST action to the same controller:

*// POST /api/enrollments creates and returns 201 with Location header*

\[HttpPost\]

**public** async Task<IActionResult> Create\(\[FromBody\] CreateEnrollmentRequest request\)

\{

var record = await enrollmentService.EnrollAsync\(request.StudentId, request.CourseCode\);

**return** CreatedAtAction\(nameof\(GetById\), **new** \{ id = record.Id \}, record\);

\}

Add the request model at the bottom of the same file \(or a separate Models folder\): **public** record CreateEnrollmentRequest\(string StudentId, string CourseCode\); CreatedAtAction does three things: sets the status to **201**, sets the Location header to the URL of the new resource \(/api/enrollments/\{id\}\), and returns the created record in the body. This is the correct HTTP semantic for resource creation not a bare 200 OK. Part C: DELETE with 204 / 404

Add a DELETE action:

*// DELETE /api/enrollments/\{id\} returns 204 or 404*

\[HttpDelete\("\{id\}"\)\]

**public** async Task<IActionResult> Delete\(string id\)

\{

var deleted = await enrollmentService.DeleteAsync\(id\);

**return** deleted ? NoContent\(\) : NotFound\(\);

\}

NoContent\(\) returns **204** the standard response for a successful delete with no body. Run / Call / Expected / Common failure

 **Run:** dotnet run

 **Call:** Use curl or your browser/Scalar to test all four endpoints:

*\# 1. GET all \(empty list\)*

curl http://localhost:5000/api/enrollments

*\# Expected: 200 with \[\]*

*\# 2. POST a new enrollment*

curl-X POST http://localhost:5000/api/enrollments \\

-H "Content-Type: application/json" \\

-d '\{"studentId":"S-001","courseCode":"CS-101"\}'

*\# Expected: 201 Created with Location header and body*

*\# 3. GET the created enrollment \(use the id from step 2\)*

curl http://localhost:5000/api/enrollments/\{id\}

*\# Expected: 200 with the record*

*\# 4. DELETE the enrollment*

curl-X DELETE http://localhost:5000/api/enrollments/\{id\} *\# Expected: 204 No Content*

*\# 5. GET the deleted enrollment*

curl http://localhost:5000/api/enrollments/\{id\}

*\# Expected: 404 Not Found*

 **Common failure 404 for everything:** Missing AddControllers\(\) or MapControllers\(\)

in Program.cs, or wrong \[Route\] prefix.

 **Common failure POST returns 200 instead of 201:** You used Ok\(\) instead of

CreatedAtAction\(\). The Location header is required for REST compliance.

 **Common failure POST returns 500 with “Value cannot be null”:** The \[FromBody\]

attribute is missing, or the Content-Type: application/json header is missing from the request.

**Troubleshooting**

 If CreatedAtAction throws “No route matches the supplied values,” the

nameof\(GetById\) argument does not match your GET action’s name. They must be identical.

 If curl is not available on Windows, use PowerShell: Invoke-WebRequest -Uri

"http://localhost:5000/api/enrollments" -Method GET

 

Exercise 6: The Consistent Fault \(Standardized Error Handling\) **Context:** When the API crashes, it currently returns a raw HTML error page. The TMS frontend needs structured JSON \(**RFC 9457 ProblemDetails**\) so it can show a helpful alert whether the failure was saving an **Enrollment**, publishing a **Course** outline, recording an **Assessment** attempt, or issuing a **Certificate**. UseStatusCodePages is optional but recommended alongside ProblemDetails it turns **empty-body** status codes \(such as bare **404** responses\) into the same consistent JSON shape.

Implementation: ProblemDetails

Implement the standard **ProblemDetails** exception handler in Program.cs. *//* **TODO** *1: In the Builder section, add the ProblemDetails service // builder.Services.AddProblemDetails\(\);*

*//* **TODO** *2: In the Middleware section, use the Exception Handler // app.UseExceptionHandler\(\);*

*//* **TODO** *3 \(optional alignment with Essentials\): app.UseStatusCodePages\(\); //* **TODO** *4: Map a test route '/api/error' that intentionally throws.* You need an exception to throw. Add this class to your project \(a new file or at the bottom of EnrollmentService.cs\):

**public class** TmsDatabaseException\(string message\) : Exception\(message\); Then wire the test route in Program.cs: app.MapGet\("/api/error", \(\) =>

\{

**throw new** TmsDatabaseException\("Simulated database failure for ProblemDetails testing"\);

\}\);

Run / Call / Expected / Common failure

 **Run:** dotnet run

 **Call:** GET http://localhost:5000/api/error \(or whatever path throws inside your test

route\).

 **Expected:** Response is **JSON** with at least type, title, status, and detail fields \(RFC

9457 ProblemDetails shape\), not a stack trace as HTML.

 **Common failure raw HTML:** UseExceptionHandler\(\) is missing or registered **after**

endpoint execution so exceptions never reach it. Place it **before** routing/mapping per your Essentials sample.

 

Exercise 7: The Environment Toggle \(Dev vs Prod\) **Context:** In Development, you want rich diagnostics and an interactive API explorer. In production, you must **hide** explorers and avoid leaking stack traces. Before you write any code

1. Install Scalar \(required for MapScalarApiReference\):

dotnet add package Scalar.AspNetCore

2. Add the OpenAPI service and the Scalar using to Program.cs:

**using** Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder\(args\);

*// ... your existing service registrations ...*

builder.Services.AddOpenApi\(\); *// Required before MapOpenApi\(\) will work* Without AddOpenApi\(\), calling MapOpenApi\(\) later will throw at startup. Your Task: Environment-Aware Configuration

*//* **TODO** *1: Check if the app is running in Development mode. // Stuck? if \(app.Environment.IsDevelopment\(\)\) \{ ... \}*

*//* **TODO** *2: In Development only expose the OpenAPI document and an interactive API explorer. //* *Use the built-in MapOpenApi\(\) and MapScalarApiReference\(\). // Stuck? app.MapOpenApi\(\); app.MapScalarApiReference\(\);*

*//* **TODO** *3: In Production use the exception handler middleware so stack traces //* *are never shown to external users.*

*// Stuck? app.UseExceptionHandler\(\);*

*//* **TODO** *4: Run in both environments and verify: //* *- In Development: can you browse /scalar/v1 and see your endpoints? //* *- In Production: does a thrown exception return ProblemDetails JSON, not a stack trace?*

Run / Call / Expected / Common failure

**Development**

 **Run:** dotnet run \(default is Development in launchSettings.json\).

 **Call:** Browser http://localhost:5000/scalar/v1 \(path may vary slightly by template;

use the link shown if different\).

 **Expected:** Scalar UI loads and lists your OpenAPI document.

**Production \(Windows PowerShell\)**

$env:ASPNETCORE\_ENVIRONMENT = "Production" dotnet run

 **Call:** Same Scalar URL as above, and hit an endpoint that throws \(or /api/error if

you kept it\).

 **Expected:** /scalar/v1 returns **404** or is otherwise **not** exposed; errors return

**ProblemDetails JSON**, not HTML stack traces.

Reset terminal when done:

Remove-Item Env:ASPNETCORE\_ENVIRONMENT

 **Common failure Scalar does not compile:** Package not added \(dotnet add package

Scalar.AspNetCore\) or missing using Scalar.AspNetCore;.

Session Checkpoint

 

Before completing the module, confirm all five:

☐ POST /api/enrollments with \{"studentId":"S-001","courseCode":"CS-101"\} returns 201

Created with a Location header pointing to /api/enrollments/\{id\} verify the header with curl -i

☐ DELETE /api/enrollments/\{id\} returns 204 No Content for an existing record and 404

Not Found for a non-existent one

☐ GET /api/error returns RFC 9457 ProblemDetails JSON \(type, title, status, detail\) not

an HTML stack trace

☐ In Development, /scalar/v1 loads the API explorer; in Production, it returns 404

☐ In Production, hitting /api/error returns the same ProblemDetails JSON envelope

not a stack trace, not raw HTML

If any item fails, finish it before your instructor runs the closing Session 3 exercise \(if scheduled\).

 

Lab Verification Full Module

This list is split on purpose:

 **Items 1–11:** Self-check your TMS API through Session 3. Fix failures before any

instructor-led integration exercise.

 **Item 12:** After that exercise \(if your programme runs it\), confirm the new

behaviour at the HTTP boundary and that items 1–11 still pass.

Items 1–11

Run your API using dotnet run and verify each row: \# Check Session Tier 1 GET /api/assessments/results without credentials returns **401** 1 T1

**Unauthorized**

2 Every response includes an X-Correlation-Id header \(check with curl -v\) 1 T1 3 Console logs show structured lines with \[CorrelationId\], method, 1 T1

path, status code, elapsed time

4 With ValidateScopes = true, injecting IEnrollmentService directly into a 2 T2

singleton throws at startup

5 After the IServiceScopeFactory fix, the app starts and 15 concurrent 2 T2

calls do not cross-contaminate data \# Check Session Tier 6 Deleting Payments:GatewayUrl from appsettings.json crashes the app 2 T3

at startup \(not at first request\)

7 Duplicate enrollment logs \[Warning\]; success logs \[Information\]; not- 2 T4

found logs \[Warning\]

8 POST /api/enrollments returns **201 Created** with a Location header 3 T5

\(not bare 200\)

9 DELETE /api/enrollments/\{id\} returns **204**; a second DELETE on the 3 T5

same ID returns **404**

10 /api/error returns **ProblemDetails JSON** with type, title, status, detail 3 T5

no HTML

11 In Development, /scalar/v1 loads the API explorer; in Production, it 3 T5

returns **404**

If all eleven pass, you are ready for M4 wrap-up and \(if applicable\) your instructor’s closing exercise.

After the closing exercise 1 item

\# Check Sessio Tier

n

12 Instructor-assigned integration work is observable at the HTTP 3 T6

boundary; items **1–11** still pass \(no regressions\)

If item 12 passes alongside items 1-11, your backend infrastructure is production-ready.



Module 5 Lab Session 1: Make the Database Talk Field Value

**Module** M5: Entity Framework Core 10 and PostgreSQL **Session** 1 of 3

**Exercises** 1 \(DbContext configuration and migrations\), 2 \(LINQ queries

and engine experiments\)

 

Welcome to the Persistence Layer Sprint In the previous module, you built the TMS Web API on top of an in-memory storage dictionary inside EnrollmentService. Yared, the lead instructor, opens this session with a simple demonstration: he enrolls a new student in a course, restarts the API process, and requests the student list. The enrollment is gone. Volatile system memory is perfectly fine for understanding routing, dependency injection, and middleware. However, an enterprise system requires data that survives restarts, system failures, and upgrades. The database is the ultimate contract that the institution trusts, and Entity Framework \(EF\) Core 10 is the bridge that allows your C\# codebase to communicate with this persistent store. In this lab session, you will connect your existing TmsApi codebase to a local PostgreSQL instance. You will define your database tables using C\# classes, write and inspect your first database migration, and run experiments to demystify how LINQ composition works under the hood. By the end of this session, your console will output the exact SQL commands generated by your queries, proving that filtering, sorting, and grouping happen where they belong: in the database, not in memory.



Before You Begin: Set Up PostgreSQL and Tooling Before writing any C\# code, ensure your local environment has PostgreSQL installed and the .NET entity framework command-line tools ready. 1. Verify PostgreSQL Installation

If you do not have PostgreSQL installed, download the installer from

postgresql.org/download and complete the installation. Be sure to note the password you assign to the default postgres database user. Ensure the PostgreSQL command-line utility is accessible by opening a terminal and running:

psql--version

If your system returns an error, make sure the PostgreSQL binary path \(e.g., C:\\Program Files\\PostgreSQL\\18\\bin on Windows\) is added to your environment PATH variable.

2. Confirm your .NET SDK

Verify that you are using .NET 10:

dotnet--version

This must return a 10.x.y version. If it returns an older version, install the latest .NET 10 SDK before proceeding.

3. Add EF Core Packages to TmsApi

Open your terminal and navigate to the folder containing your existing TmsApi project from Module 4. Run the following commands to add the required Entity Framework Core packages:

dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL dotnet add package Microsoft.EntityFrameworkCore.Design dotnet add package Microsoft.EntityFrameworkCore.Tools



Before continuing, understand what these packages do: Package What it is Why we need it **Npgsql.EntityFramework** The PostgreSQL Database EF Core is database-**Core.PostgreSQL** Provider for EF Core. agnostic. This package

translates your LINQ

queries into PostgreSQL-

compliant SQL

commands and handles

database connections.

**Microsoft.EntityFramew** Design-time logic for EF Provides compiler **orkCore.Design** Core. services that allow EF

Core to inspect your

project code structure to

generate migrations. It

does not ship with your

compiled production

binary.

**Microsoft.EntityFramew** Package-manager Integrates migration **orkCore.Tools** console tooling hooks. tools into the .NET

development

environment and is

required for design-time

CLI capabilities.

4. Verify the Entity Framework CLI Tools

Ensure the dotnet-ef CLI tool is installed globally on your machine. **What is the** **dotnet-ef** **tool?** It is a command-line tool that acts as your migration operator. While the EF packages are referenced inside your C\# code, the dotnet-ef tool is a command-line utility that compiles your project, detects model changes, and creates or runs migration scripts. Verify if the tool is installed by running:

dotnet ef--version

If the tool is missing, install it globally by running:

dotnet tool install--global dotnet-ef When installed, running dotnet ef --version should print a 10.x.y version matching your SDK.

Exercise 1: Configure TmsDbContext and Apply the First Migration

Before you can query data using C\#, you must model your database tables as C\# classes \(Entities\) and group them into a context \(DbContext\) that manages connections, tracks state, and translates code to database calls.

**A note on the models you sketched in M1.** Back in C\# Essentials you

modelled the full TMS domain — Student, Course, Enrollment,

Assessment, and Certificate — inside the TmsCore console project, as

immutable shapes \(record types, init-only properties, string identifiers

like Code\). That was the right design for a console app teaching value

semantics. Persistence changes the rules, so we evolve those shapes

here rather than copy them verbatim: -**Mutable properties.** EF Core

tracks each loaded row and writes back changes. It needs settable

properties, so the entities below drop init/record immutability. -

**Stable surrogate keys.** Relational tables want a stable, generated

primary key rather than a business value like a course code that might

change. We use int Id \(PostgreSQL auto-increments it\) because it is

compact, fast to index, and easy to read in logs and psql — the right

default for this course. GUIDs \(Guid → PostgreSQL uuid\) are a perfectly

valid alternative when you need keys generated on the client, merged

across databases, or non-guessable in URLs; the cost is 16 bytes per key

and, for *random* GUIDs, poorer index locality — reach for time-ordered

Guid.CreateVersion7\(\) \(UUIDv7\) if you go that way. Either way, the key

is a surrogate, so foreign keys \(StudentId/CourseId\) get a clean target

instead of string codes. -**Navigation properties.** Relationships

\(Enrollment → Student/Course, and the ICollection<Enrollment>

collections\) are what let EF translate your LINQ joins and GroupBy into

SQL in Exercise 2.

So treat the classes below as the **persistence-ready evolution** of your

M1 domain — same TMS concepts, reshaped for the database. Recreate

them in TmsApi as shown \(do not paste the old TmsCore versions, which

will not migrate or track correctly\).

Step 1: Define Your Database Entities

Create a folder named Entities in your TmsApi project. Add all five C\# classes for the TMS domain below. You will wire the first three \(Student, Course, Enrollment\) into the database in this exercise; Assessment and Certificate are defined here too, but you will connect them yourself in the Extended Exercise so you practise the migration loop on your own.

1. **Entities/Student.cs**

**namespace** TmsApi.Entities; **public class** Student

\{

**public** int Id \{ **get**; **set**; \} *// sur*

*rogate primary key — internal, used by foreign keys*

**public** required string RegistrationNumber \{ **get**; **set**; \} *// na*

*tural key — human-readable \(uniqueness configured in Session 2\)*

**public** required string Name \{ **get**; **set**; \} **public** decimal GPA \{ **get**; **set**; \} **public** bool IsActive \{ **get**; **set**; \} = **true**; *// Navigation property for many-to-many relationship* **public** ICollection<Enrollment> Enrollments \{ **get**; **set**; \} = **ne**

**w** List<Enrollment>\(\);

\}

2. **Entities/Course.cs**

**namespace** TmsApi.Entities; **public class** Course

\{

**public** int Id \{ **get**; **set**; \} *// surrogate primar*

*y key — internal, used by foreign keys*

**public** required string Code \{ **get**; **set**; \} *// natural key — hu*

*man-readable \(uniqueness configured in Session 2\)*

**public** required string Title \{ **get**; **set**; \} **public** int Capacity \{ **get**; **set**; \} *// Navigation property for many-to-many relationship* **public** ICollection<Enrollment> Enrollments \{ **get**; **set**; \} = **ne**

**w** List<Enrollment>\(\);

\}

3. **Entities/Enrollment.cs**

**using** System;

**namespace** TmsApi.Entities;

**public class** Enrollment

\{

**public** int Id \{ **get**; **set**; \} **public** int StudentId \{ **get**; **set**; \} **public** int CourseId \{ **get**; **set**; \} **public** decimal? Grade \{ **get**; **set**; \} *// Nullable, as student m*

*ay be currently enrolled*

**public** DateTime EnrolledAt \{ **get**; **set**; \} = DateTime.UtcNow; *// Navigation properties back to entities*

**public** Student Student \{ **get**; **set**; \} = **null**\!; **public** Course Course \{ **get**; **set**; \} = **null**\!;

\}

4. **Entities/Assessment.cs** — a quiz or practical task that belongs to one

Course.

**namespace** TmsApi.Entities; **public class** Assessment

\{

**public** int Id \{ **get**; **set**; \} **public** required string Title \{ **get**; **set**; \} **public** decimal MaxScore \{ **get**; **set**; \} **public** decimal Weight \{ **get**; **set**; \} *// share of the final gra*

*de, e.g. 0.30m for 30%*

*// Foreign key \+ navigation to the owning course* **public** int CourseId \{ **get**; **set**; \} **public** Course Course \{ **get**; **set**; \} = **null**\!;

\}

5. **Entities/Certificate.cs** — issued to one Student for completing one

Course.

**using** System;

**namespace** TmsApi.Entities; **public class** Certificate

\{

**public** int Id \{ **get**; **set**; \} *// surrogate*

*primary key*

**public** required string SerialNumber \{ **get**; **set**; \} *// natural*

*key — human-readable \(uniqueness configured in Session 2\)*

**public** DateTime IssuedAt \{ **get**; **set**; \} = DateTime.UtcNow; *// Foreign keys \+ navigation to the student and course* **public** int StudentId \{ **get**; **set**; \} **public** int CourseId \{ **get**; **set**; \} **public** Student Student \{ **get**; **set**; \} = **null**\!; **public** Course Course \{ **get**; **set**; \} = **null**\!;

\}

**Two keys per entity, on purpose.** Each of these tables has an int Id

*surrogate* key and a *natural* key \(Course.Code,

Student.RegistrationNumber, Certificate.SerialNumber\). Note the

natural-key property does **not** repeat its class name it is Code, not

CourseCode following the .NET guideline against stuttering members.

\(The <Entity>Id names you see, like Enrollment.CourseId, are different:

those are foreign keys pointing at *another* entity, which is exactly EF

Core’s key-naming convention.\) They do different jobs, and good

schemas keep both: - The **surrogate** **Id** is what relationships use. It

never changes and means nothing to a human, so foreign keys

\(Enrollment.CourseId, Certificate.StudentId, …\) point at it and stay

stable even if a course is renamed or recoded. - The **natural key** is what

people read and type “CS-101”, a student’s registration number, a

printed certificate serial. It *should* be unique so the database refuses

duplicates, but we do *not* use it as the primary key, because business

identifiers have a habit of changing or being reused, and you do not

want every foreign key to break when they do.

For now these classes stay deliberately plain no database attributes. You

will declare the **unique constraint** in Session 2 with the Fluent API

\(builder.HasIndex\(c => c.Code\).IsUnique\(\)\), which keeps the entity

free of persistence concerns. So in this session’s first migration the

natural-key columns are created as NOT NULL \(because they are

required\) but not yet unique; that index arrives with your Session 2

configuration.

Step 2: Implement TmsDbContext

Create a folder named Data in your TmsApi project. Add a class file named Data/TmsDbContext.cs :

**using** Microsoft.EntityFrameworkCore; **using** TmsApi.Entities;

**namespace** TmsApi.Data;

**public class** TmsDbContext\(DbContextOptions<TmsDbContext> options\) : DbC ontext\(options\)

\{

**public** DbSet<Student> Students => Set<Student>\(\);

**public** DbSet<Course> Courses => Set<Course>\(\);

**public** DbSet<Enrollment> Enrollments => Set<Enrollment>\(\); \}

Step 3: Register the DbContext in Program.cs Open your Program.cs file. Locate the section where services are registered, and add the DbContext registration using Npgsql:

**using** Microsoft.EntityFrameworkCore; **using** TmsApi.Data;

*// Register TmsDbContext scoped for incoming HTTP requests* builder.Services.AddDbContext<TmsDbContext>\(options =>

options.UseNpgsql\(builder.Configuration.GetConnectionString\("TmsDat abase"\)\)\);

Step 4: Configure Connection String

Open appsettings.Development.json and add the ConnectionStrings block at the root level, replacing your\_postgres\_password with your actual PostgreSQL password:

\{

"Logging": \{

"LogLevel": \{

"Default": "Information", "Microsoft.AspNetCore": "Warning"

\}

\},

"ConnectionStrings": \{

"TmsDatabase": "Host=localhost;Database=TmsDb;Username=postgres;Pas sword=your\_postgres\_password"

\}

\}

Step 5: Generate the First Migration

From your project directory where your csproj file sits, run: dotnet ef migrations add InitialCreate

This command analyzes your entity properties and builds a C\# migration script detailing the SQL layout changes.

Step 6: Inspect the Generated Migration File

Before pushing changes to the database, open the newly created migration file located inside the Migrations/ directory. Find the class and review the two main methods: -**Up\(MigrationBuilder migrationBuilder\)**: This method executes when applying the migration. Locate the CreateTable calls. Identify the primary keys and the foreign keys configured for Enrollments. Note that the natural-key columns \(Courses.Code, Students.RegistrationNumber\) are created NOT NULL but **not** unique yet you add that unique index in Session 2 with the Fluent API. -**Down\(MigrationBuilder migrationBuilder\)**: This method executes if you roll back the migration. Verify that it drops the tables in reverse dependency order \(Enrollments first, then Students and Courses\).

**Why don’t** **Assessment** **and** **Certificate** **show up when you migrate**

**in a moment?** A C\# class only becomes a database table once it is part

of the EF Core *model* that is, registered as a DbSet \(or reachable through

a navigation property from an entity that is\). In Step 2 you register only

Student , Course, and Enrollment, and notice that Student/Course do

**not** carry collections pointing at Assessment or Certificate. So EF Core

never discovers those two classes, and your first migration creates three

tables, not five. Wiring them in is your Extended Exercise. Step 7: Apply the Migration

Apply the migration changes to your PostgreSQL instance: dotnet ef database update

Step 8: Verify the Schema in PostgreSQL

Open your command terminal and log into your PostgreSQL instance to verify that the database and its tables were successfully provisioned: psql-U postgres-d TmsDb-c "\\dt" You should see output indicating that the tables exist:

List of relations

Schema | Name | Type | Owner

--------\+-------------\+-------\+----------

public | Courses | table | postgres public | Enrollments | table | postgres

public | Students | table | postgres

\(3 rows\)

 

Exercise 2: The LINQ Engine Logging, Deferred Execution, and Translation Limits

LINQ queries look like standard C\# code, but when run against a DbSet, they act as instructions to compile SQL. In this exercise, you will configure logging, seed data, and run experiments to see exactly when and how C\# translates to database commands.

Step 1: Enable Console SQL Logging

To see the exact SQL running behind the scenes, update your AddDbContext registration in Program.cs to print logs directly to the command window during development:

builder.Services.AddDbContext<TmsDbContext>\(options =>

options.UseNpgsql\(builder.Configuration.GetConnectionString\("TmsDat abase"\)\)

.LogTo\(Console.WriteLine, LogLevel.Information\) *// Log SQL t*

*o output window*

.EnableSensitiveDataLogging\(\)\); *// Show parameters in query*

*logs \(dev only\)*

Step 2: Write an Auto-Seeder

To ensure your database contains records to query, add this temporary seeding block inside Program.cs just before app.Run\(\). It verifies if database tables are empty at application startup and populates them:

*// Seed test data at startup*

**using** \(var scope = app.Services.CreateScope\(\)\) \{

var context = scope.ServiceProvider.GetRequiredService<TmsDbContex t>\(\);

context.Database.Migrate\(\); *// Applies any pending migrations; keep s migration history intact*

**if** \(\!context.Students.Any\(\)\)

\{

var students = **new** List<Student> \{

**new**\(\) \{ RegistrationNumber = "TMS-2026-0001", Name = "Alice Smith", GPA = 3.8m, IsActive = **true** \},

**new**\(\) \{ RegistrationNumber = "TMS-2026-0002", Name = "Bob J

ones", GPA = 2.9m, IsActive = **true** \},

**new**\(\) \{ RegistrationNumber = "TMS-2026-0003", Name = "Charl

ie Brown", GPA = 3.4m, IsActive = **false** \},

**new**\(\) \{ RegistrationNumber = "TMS-2026-0004", Name = "Diana

Prince", GPA = 3.9m, IsActive = **true** \},

**new**\(\) \{ RegistrationNumber = "TMS-2026-0005", Name = "Evan

Wright", GPA = 2.5m, IsActive = **true** \}

\};

context.Students.AddRange\(students\); var courses = **new** List<Course> \{

**new**\(\) \{ Code = "CS-101", Title = "Introduction to Computer

Science", Capacity = 30 \},

**new**\(\) \{ Code = "CS-201", Title = "Data Structures and Algor

ithms", Capacity = 25 \},

**new**\(\) \{ Code = "MAT-101", Title = "Calculus I", Capacity =

40 \}

\};

context.Courses.AddRange\(courses\); context.SaveChanges\(\);

var enrollments = **new** List<Enrollment> \{

**new**\(\) \{ StudentId = students\[0\].Id, CourseId = courses\[0\].I

d, Grade = 4.0m \},

**new**\(\) \{ StudentId = students\[0\].Id, CourseId = courses\[1\].I

d, Grade = 3.6m \},

**new**\(\) \{ StudentId = students\[1\].Id, CourseId = courses\[0\].I

d, Grade = 2.8m \},

**new**\(\) \{ StudentId = students\[3\].Id, CourseId = courses\[1\].I

d, Grade = 3.9m \}

\};

context.Enrollments.AddRange\(enrollments\); context.SaveChanges\(\);

\}

\}

**Why** **Migrate\(\)** **and not** **EnsureCreated\(\)****?** You already created the

schema with dotnet ef database update. Database.Migrate\(\) respects

that migration history and applies anything still pending.

EnsureCreated\(\) is a different, incompatible path: it builds the schema

with no migration record, so the next dotnet ef database update you

run in Session 2 would fail with a “table already exists” error. Stick with

migrations end to end.

Step 3: Run the Deferred Execution Experiment Create a temporary API controller called Controllers/TestController.cs inside your project. This controller will host experiments to illustrate query compilation timing.

**using** Microsoft.AspNetCore.Mvc; **using** System;

**using** System.Linq;

**using** TmsApi.Data;

**namespace** TmsApi.Controllers;

\[ApiController\]

\[Route\("api/test"\)\]

**public class** TestController\(TmsDbContext context\) : ControllerBase \{

\[HttpGet\("deferred"\)\]

**public** IActionResult TestDeferred\(\)

\{

Console.WriteLine\("\\n>>> STEP 1: Building the query object \(no

database contact\)..."\);

var query = context.Students.Where\(s => s.GPA >= 3.0m\); Console.WriteLine\(">>> STEP 2: Appending a sorting clause..."\); var orderedQuery = query.OrderBy\(s => s.Name\); Console.WriteLine\(">>> STEP 3: Materializing query into a C\# Li

st..."\);

var results = orderedQuery.ToList\(\); *// Execution is triggered*

*here*

Console.WriteLine\(">>> STEP 4: Materialization finished. List p

opulated.\\n"\);

**return** Ok\(results\);

\}

\}

Start your application \(dotnet run\) and invoke this endpoint using a browser or a terminal call. Use the port your app actually printed at startup \(look for the Now listening on: http://localhost:... line it is set by launchSettings.json and is not always 5000\):

curl http://localhost:5000/api/test/deferred

Look closely at your application console window. Notice the output order. **Observer Challenge**: Verify that the database queries and connection logs occur exactly *between* Step 3 and Step 4. This demonstrates that LINQ queries acting on IQueryable are merely instruction sets \(recipes\) and do not run until you request the final collection \(materialization\).

Step 4: Run the SQL Translation Failure Experiment Add a helper C\# method and a new testing endpoint to your TestController class to see what happens when C\# functions cannot be translated to SQL:

*// Non-translatable helper method*

**private static** bool IsHonorRoll\(decimal gpa\)

\{

**return** gpa >= 3.5m;

\}

\[HttpGet\("translation-fail"\)\]

**public** IActionResult TestTranslationFail\(\)

\{

Console.WriteLine\("\\n>>> STEP 1: Running non-translatable quer

y..."\);

**try**

\{

var students = context.Students

.Where\(s => IsHonorRoll\(s.GPA\)\) *// EF Core does not kno*

*w how to map this method to SQL*

.ToList\(\);

**return** Ok\(students\);

\}

**catch** \(Exception ex\)

\{

Console.WriteLine\($">>> EXCEPTION CAUGHT: \{ex.Message\}\\n"\); **return** BadRequest\(**new** \{ Message = ex.Message \}\);

\}

\}

Invoke this endpoint in your terminal:

curl http://localhost:5000/api/test/translation-fail The application will catch an InvalidOperationException stating: The LINQ expression '...' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation... **Why did this fail?** EF Core parses your LINQ code into an Abstract Syntax Tree \(AST\) to generate SQL. Because your custom method IsHonorRoll is compiled C\# IL, Npgsql cannot translate it into a PostgreSQL function.

**How to Resolve This**: You have two primary resolution patterns depending on performance requirements: 1. **Server-Side Evaluation \(Preferred\)**: Replace the custom helper method with inline logic that EF Core can parse directly: csharp var students = context.Students.Where\(s => s.GPA >= 3.5m\).ToList\(\); *Verify in log*: This sends a clean, filtered query: SELECT ... FROM "Students" WHERE "GPA" >= 3.5. 2. **Client-Side Evaluation**: Pull the dataset into memory using .AsEnumerable\(\) before applying C\# logic: csharp var students = context.Students .AsEnumerable\(\) // Pulls all rows into application RAM .Where\(s => IsHonorRoll\(s.GPA\)\) .ToList\(\); *Warning* : While this works, look at the generated SQL in your console. It sends SELECT ... FROM "Students", pulling the entire table across the network. If your database grows to millions of records, this will degrade application performance. Step 5: Solve the Registrar’s Business Queries Now that you understand materialization and translation, write endpoints to solve the four registrar requests. Ensure that the processing is fully translated and executed by the database.

Add these queries to a new controller or a reporting service. Write your queries using the syntax patterns below and check the console logs to confirm that filters, sorting, and aggregations are present in the SQL output:

1. **How many active students have GPA >= 3.0?**

o **C\#**:

var count = await context.Students

.Where\(s => s.IsActive && s.GPA >= 3.0m\) .CountAsync\(\);

o **SQL Check**: Ensure you see SELECT COUNT\(\*\)::int4 FROM

"Students" AS s WHERE s."IsActive" AND s."GPA" >= 3.0.

2. **Which courses have the most enrollments, sorted descending?**

o **C\#**:

var list = await context.Courses

.Select\(c => **new** \{

c.Title,

EnrollmentCount = c.Enrollments.Count

\}\)

.OrderByDescending\(x => x.EnrollmentCount\) .ToListAsync\(\);

o **SQL Check**: Look for ORDER BY and count calculations happening in

SQL, not C\#.

3. **What is the average GPA per course?**

o **C\#**:

var list = await context.Enrollments

.GroupBy\(e => e.Course.Title\) .Select\(g => **new** \{

Course = g.Key, AverageGPA = g.Average\(e => e.Student.GPA\)

\}\)

.ToListAsync\(\);

o **SQL Check**: Look for GROUP BY and the AVG aggregation function in

the log.

4. **Which students have zero enrollments?** \(Show both patterns\)

o **Approach A \(Using Subquery\)**:

var list = await context.Students

.Where\(s => \!s.Enrollments.Any\(\)\) .Select\(s => s.Name\) .ToListAsync\(\);

o **Approach B \(Using EF Core 10 LeftJoin\)**:

var list = await context.Students

.LeftJoin\(context.Enrollments,

s => s.Id,

e => e.StudentId, \(s, e\) => **new** \{ s, e \}\)

.Where\(x => x.e == **null**\) .Select\(x => x.s.Name\) .ToListAsync\(\);

o **SQL Check**: Observe the output queries. Approach A uses NOT

EXISTS \(SELECT 1 FROM "Enrollments" ...\), while Approach B outputs a LEFT JOIN ... WHERE ... IS NULL. Extended Exercise \(Stretch\): Wire Assessment and Certificate into the Database

You already wrote Assessment and Certificate in Step 1, but they are not tables yet. Nothing in TmsDbContext references them, so EF Core never put them in the model. The registrar still cannot record a mark or issue a certificate. Your job is to wire them in and migrate them, applying the exact loop from Exercise 1 with **no** **commands handed to you beyond the names**. This proves a point worth internalising: *defining a class is not the same as creating* *a table.* Membership in the EF Core model is what counts. Your task

1. **Register both entities** as DbSet properties on TmsDbContext, following the

existing Set<Student>\(\) / Set<Course>\(\) / Set<Enrollment>\(\) pattern. \(Optional, and a good check of your understanding: add ICollection<Assessment> to Course, and ICollection<Certificate> to Student and Course, so the relationships read from both sides. Notice this does **not** change the schema. The foreign keys already live on Assessment/Certificate.\)

2. **Generate a new migration** *not* a second InitialCreate. Give it a

descriptive name:

dotnet ef migrations add AddAssessmentsAndCertificates

3. **Inspect it before applying.** Open the new migration and confirm:

o Only the two new tables appear in Up\(\); your existing three tables

are untouched.

o The foreign keys point where you expect \(Assessments.CourseId →

Courses ; Certificates.StudentId → Students and Certificates.CourseId → Courses\), and Down\(\) drops the new tables before anything they depend on.

4. **Apply and verify:**

dotnet ef database update

psql-U postgres-d TmsDb-c "\\dt" You should now see five tables instead of three.

Hints, not solutions

 A DbSet<T> is the single line that pulls a class into the model. If your

migration comes back **empty**, you forgot to add the DbSet \(or saved nothing\). If it tries to recreate existing tables, you accidentally ran InitialCreate again instead of a new migration name.

 The foreign keys are already on the entities from Step 1 \(CourseId,

StudentId\), so EF will infer the relationships by convention. you should not need any extra configuration here.

 If the migration shows changes you did not intend, run dotnet ef

migrations remove, fix the cause, and regenerate before touching the database.

Done when

 psql ... "\\dt" lists Assessments and Certificates alongside the original

three tables.

 You can point to each foreign key in your migration’s Up\(\) method and

say which table it links to.

 

Why This Session Ends Here

In this session, you established the persistence layer foundation: 1. **A working** **database context mapping real Postgres tables**. Your TMS database now persists records across API restarts. 2. **Comprehensive SQL Logging**. You can inspect the queries generated by your C\# code and identify sub-optimal execution paths before shipping. 3. **LINQ Translation discipline**. You understand that IQueryable expressions execute lazily at the database level, and how to avoid client-side evaluation performance traps.

In the next session, you will configure schema specifics, set up paging for large record lists, and use configurations to define relationship constraints.



Session 1 Checkpoint

Before proceeding to Session 2, confirm that: - \[ \] dotnet ef database update executes successfully and you can view the tables in PostgreSQL. - \[ \] You can explain what both Up\(\) and Down\(\) methods do inside your initial migration file. -\[ \] The SQL log verifies that filters, aggregates, sorting, and joins run on the database server. - \[ \] You can explain why calling .ToList\(\) before .Where\(\) poses a performance concern in database-backed systems. - \[ \] *\(Stretch\)* If you took the extended exercise, \\dt shows Assessments and Certificates, and you applied them through a named migration. not EnsureCreated\(\).



Module 5 Guided Lab Session 2: Design the Schema Field Value

**Module** M5: Entity Framework Core 10 and PostgreSQL **Exercises** 3 \(GroupBy \+ pagination\), 4 \(IEntityTypeConfiguration\), 5 \(Relationships\)

 

Prerequisites Check Before You Start

Your TMS solution must contain:

 A working TmsDbContext registered with UseNpgsql and a valid TmsDatabase

connection string

 An applied InitialCreate migration dotnet ef migrations list shows it as applied

 SQL logging enabled in development every query in Session 1 produced SQL in

the console

 Working entities for Student, Course, and Enrollment with appropriate primary

keys and value-typed properties

If any of these are missing, go back to Session 1. The exercises in this session evolve the schema you scaffolded yesterday without a working DbContext and visible SQL, the design choices you make today are invisible.

 

What This Session Is Really About

In Session 1 you proved the database can talk back. Today you design what it says. The three exercises share one theme: **let the database do the work, and keep your C\#** **code short and inspectable.**

1. **Paginate and group in SQL** \(Exercise 3\) never load a whole table just to take the

first 20 rows

2. **Move every entity’s mapping into its own class** \(Exercise 4\) OnModelCreating is a

router, not a 200-line config dump

3. **Model the relationship graph deliberately** \(Exercise 5\) OnDelete behaviour,

navigation properties, and FK direction are design choices, not defaults

These are the decisions that make the difference between an EF Core codebase that scales with the team and one that becomes the file nobody wants to open. Every senior interview asks at least one of: “How do you paginate?”, “Where do your entity configurations live?”, and “What happens when I delete a course that has enrollments?” Today you build defensible answers to all three.

Exercise 3: GroupBy, aggregates, and pagination **Context:** The dashboard must show paged roster data and summary tiles. The database should do the math.

Tasks

1. Implement a paged list of students: **page size 20**, **page number** as a parameter,

stable sort by **name** \(always OrderBy before Skip/Take\).

2. Implement a summary query: top **5** courses by enrollment count with course

title and count \(same idea as quiz-style reporting\).

*//* **TODO** *1: Pagination OrderBy, Skip\(\(page - 1\) \* pageSize\), Take\(pageSize\), ToListAsync with Ca ncellationToken.*

*//* **TODO** *2: Top 5 courses by enrollment GroupBy, order by count, Take\(5\).* **Checkpoint:** SQL log shows LIMIT / OFFSET and GROUP BY where appropriate. **Troubleshooting:**

 **Client evaluation warning \(“could not be translated”\):** Your GroupBy projection

uses a method EF cannot translate to SQL. Simplify the lambda use only property access and aggregates \(Count\(\), Average\(\)\) inside Select.

 **Pagination returns wrong rows:** You must OrderBy before Skip/Take. Without a

stable sort, PostgreSQL may return rows in any order between pages.

 **Empty results:** Confirm you seeded enough enrollment rows in Exercise 2. Run

SELECT COUNT\(\*\) FROM "Enrollments" in pgAdmin to verify.

 

Exercise 4: IEntityTypeConfiguration for each entity **Context:** OnModelCreating should stay short. Each entity gets its own configuration class. Steps

1. Create StudentConfiguration, CourseConfiguration, and EnrollmentConfiguration

implementing IEntityTypeConfiguration<T>.

2. Move key lengths, required fields, and relationship setup from attributes \(if any\)

into Fluent API as your team prefers.

3. Ensure TmsDbContext uses:

modelBuilder.ApplyConfigurationsFromAssembly\(**typeof**\(TmsDbContext\).Assembly\);

4. Add a migration that reflects your refinements \(for example dotnet ef migrations

add RefineTmsModel\) and **inspect** before database update.

*//* **TODO***: In EnrollmentConfiguration, configure FKs and required properties. // Example shape \(adjust names to your model\):*

*// builder.HasOne\(e => e.Student\).WithMany\(s => s.Enrollments\).HasForeignKey\(e => e.StudentI d\);*

*// builder.HasOne\(e => e.Course\).WithMany\(c => c.Enrollments\).HasForeignKey\(e => e.CourseId\);* **Checkpoint:** Three configuration classes, no giant OnModelCreating method. **Troubleshooting:**

 **“The entity type requires a primary key”:** Your configuration is missing

builder.HasKey\(e => e.Id\) or EF cannot infer the key by convention. Name the property Id or EnrollmentId, or configure it explicitly.

 **Migration shows unexpected table drops:** You renamed a DbSet property or an

entity class. EF interprets this as “drop old table, create new one.” Rename in a separate migration with migrationBuilder.RenameTable\(\) instead.

 **ApplyConfigurationsFromAssembly** **finds nothing:** The configuration classes must

be public and in the same assembly as TmsDbContext. Check the assembly reference.

 

Exercise 5: Model the TMS graph

**Context:** A student enrolls in many courses; a course has many students; **enrollment** stores grade and dates.

Tasks

1. Ensure **one-to-many** from Course to Enrollment and from Student to Enrollment.

2. Decide **delete behavior** \(for example Restrict on course delete when enrollments

exist\) and document why in a one-sentence comment.

3. Add or adjust navigation properties so Include can load related data in Session 3.

*//* **TODO***: Choose OnDelete behavior and express it in Fluent API. // builder.HasMany\(...\).WithOne\(...\).OnDelete\(DeleteBehavior.Restrict\);* **Checkpoint:** Model builds; migration would create correct FK constraints \(you will generate it in Session 3, Exercise 6\).

**Troubleshooting:**

 **Cascade cycle detected:** PostgreSQL rejects multiple cascade paths to the same

table. Use DeleteBehavior.Restrict on at least one side and handle deletion in your application code.

 **Navigation property is always null:** You declared the property but forgot Include\(\)

when querying. Without eager loading, navigation properties remain null by default \(EF Core does not use lazy loading unless explicitly configured\).

Why this session ends here

You have made three decisions that scale from a 10-row demo to a 10-million-row production database:

 **Pagination and aggregation run in SQL.** A request for page 50 does not load

pages 1 to 49 first. A request for “top five courses” does not enumerate every enrollment in C\# memory. Both stay constant-cost as your data grows.

 **Configuration is per-entity and code-discoverable.** When the registrar asks

“what is the maximum length of a course title?”, the answer is one file: CourseConfiguration.cs. A new team member can read your model in five minutes instead of fifty.

 **Relationships are deliberate, not accidental.** OnDelete is set explicitly on the side

that needs to fail loudly. Navigation properties exist where Include will need them. Your migrations from now on add foreign keys with the constraints you expect, not whatever EF guesses.

In Session 3 you generate the migration that proves these design choices work, fix the most common production performance bug \(N\+1\), add audit and concurrency safety, and write the one-page Repository/UoW design summary that closes the module.

 

Session 2 Checkpoint

Before moving to Session 3, confirm all four:

☐ dotnet ef migrations add RefineTmsModel \(or a similarly named migration\)

generates a file you have read; the SQL log for database update shows the column changes you expected from your IEntityTypeConfiguration classes

☐ Three configuration classes exist \(StudentConfiguration, CourseConfiguration,

EnrollmentConfiguration\); OnModelCreating contains only modelBuilder.ApplyConfigurationsFromAssembly\(...\) and is otherwise empty or near-empty

☐ Your paginated student endpoint logs SQL with LIMIT 20 OFFSET …; the top-5

courses endpoint logs SQL with GROUP BY and ORDER BY ... DESC LIMIT 5

☐ You can point at the line in your Fluent API where

OnDelete\(DeleteBehavior.Restrict\) \(or your chosen behaviour\) is set, and explain in one sentence why a course with enrollments should not cascade-delete its students’ records

If any of the four fail, stay in Session 2. Session 3’s migration discipline, performance work, and design discussion all assume these design foundations hold.

Module 5 Guided Lab Session 3: Operate the Schema Field Value

**Module** M5: Entity Framework Core 10 and PostgreSQL **Session** 3 of 3

**Exercises** 6 \(Migration discipline\), 7 \(N\+1 fix\), 8 \(Shadow audit \+ concurrency\), 9

\(Bulk archive \+ soft delete\)

 

Prerequisites Check Before You Start

Your TMS solution must contain:

 A working TmsDbContext against PostgreSQL with at least one applied migration

\(InitialCreate\)

 At least three IEntityTypeConfiguration classes StudentConfiguration,

CourseConfiguration, EnrollmentConfiguration

 Modelled relationships from Session 2 Student ↔ Enrollment and Course ↔

Enrollment with explicit OnDelete behaviour

 Pagination and GroupBy queries that produce SQL LIMIT/OFFSET and GROUP BY in

the console

If any of these are missing, go back to Session 2. The migration, performance, and design work in this session presumes the schema design from Session 2 holds. What This Session Is Really About

The schema is designed. Today you operate it like a production team. The four exercises share one theme: **what changes when real users hit your data layer** **at the same time and the rows pile up?**

1. **Migration discipline** \(Exercise 6\) every schema change is code, reviewed before

it is applied, with an explainable Down\(\)

2. **The N\+1 trap** \(Exercise 7\) the most common production performance bug in EF

Core, deliberately produced before being fixed

3. **Audit and concurrency** \(Exercise 8\) a shadow column for LastUpdated that does

not pollute your DTOs, and a row version that prevents two staff members from silently overwriting each other’s edits

4. **Bulk and soft delete** \(Exercise 9\) ExecuteUpdateAsync for set-based archiving and

HasQueryFilter for soft-delete semantics that work everywhere IQueryable does Exercise 6: Two migrations, inspected

Schema changes are code. You review them like any other pull request. Tasks

1. Create a first migration if Exercise 1 did not already cover the full graph;

otherwise add a **second** migration for a deliberate change \(for example add Year or IsArchived on Enrollment, or a RowVersion on Student for Exercise 8\).

2. Record in your notes: what Up\(\) does in plain language.

3. Practice rollback to the previous migration on a **throwaway** database only:

dotnet ef database update PreviousMigrationName--project ...--startup-project ... **Checkpoint:** At least two migration files in the project; you can explain Down\(\) for the latest one.

**Troubleshooting:**

 **Model snapshot conflicts:** One person adds a migration at a time; merge

carefully.

 **Production:** Never run unchecked migrations; this lab stays on local or shared

dev databases.

 

Exercise 7: Many round-trips vs shaped query The N\+1 **pattern** means one query loads parents and then **extra queries** load related data per parent. With **lazy-loading proxies** enabled, touching student.Enrollments inside a loop can do that automatically. With **default EF Core**, you still get N\+1 if you **write** a query inside the loopcso the database still pays the same price. Part A Intentional N\+1 \(for learning\)

var students = await db.Students.AsNoTracking\(\).ToListAsync\(cancellationToken\); **foreach** \(var s **in** students\)

\{

*//* **TODO***: Query enrollment count for this student inside the loop \(use StudentId\).*

*// This should produce 1 \+ N SQL statements. Count them in the log.*

var count = await db.Enrollments

.AsNoTracking\(\)

.CountAsync\(e => e.StudentId == s.Id, cancellationToken\);

Console.WriteLine\($"\{s.Name\}: \{count\} enrollments"\);

\}

Part B Fix with shaping

Rewrite the query so the database does the counting in a single round trip. Use a **projection** that asks SQL to join students and count their enrollments: *// Fix: Single query with projection*

var report = await db.Students

.AsNoTracking\(\)

.Select\(s => **new**

\{

s.Name,

EnrollmentCount = s.Enrollments.Count

\}\)

.ToListAsync\(cancellationToken\);

**foreach** \(var r **in** report\)

Console.WriteLine\($"\{r.Name\}: \{r.EnrollmentCount\} enrollments"\);

**Why this works:** EF translates s.Enrollments.Count into a SQL subquery \(SELECT COUNT\(\*\) FROM "Enrollments" WHERE ...\), so the entire result comes back in **one** SQL statement instead of 1 \+ N.

Alternative approach using Include \(loads full enrollment objects heavier but sometimes useful\):

var students = await db.Students

.AsNoTracking\(\)

.Include\(s => s.Enrollments\)

.ToListAsync\(cancellationToken\);

**foreach** \(var s **in** students\)

Console.WriteLine\($"\{s.Name\}: \{s.Enrollments.Count\} enrollments"\);

**Checkpoint:** SQL log shows **one** query \(or one query \+ one subquery\), not 1 \+ N separate queries. Write one sentence in your notes explaining what changed. **Troubleshooting:**

 **Still seeing N\+1 queries:** You are still calling CountAsync inside the loop. The

projection must happen inside Select so EF can translate it to SQL.

 **Include** **does not load enrollments:** The navigation property Enrollments must be

declared as public ICollection<Enrollment> Enrollments \{ get; set; \} on the Student entity and configured in Fluent API.

 **“Client evaluation” warning on** **.Count****:** You may have a method call EF cannot

translate. Use the property .Count on the navigation collection \(not Count\(\) with a predicate EF cannot handle\).

Exercise 8: Shadow property audit stamp and concurrency token Scholarship adjustments need a **LastUpdated** audit column without cluttering every DTO. Concurrent edits should not silently overwrite each other. Shadow property \(audit\)

In StudentConfiguration \(or OnModelCreating\): builder.Property<DateTime>\("LastUpdated"\); *//* **TODO***: Set default SQL or update logic e.g. set shadow property before SaveChanges in your s ervice layer:*

*// db.Entry\(student\).Property\("LastUpdated"\).CurrentValue = DateTime.UtcNow;* Run a migration adding the column:

dotnet ef migrations add AddStudentLastUpdated--project path\\to\\infra-project.csproj--startu p-project path\\to\\api-project.csproj

dotnet ef database update--project path\\to\\infra-project.csproj--startup-project path\\to\\api-pr oject.csproj

**Verify the column exists** in pgAdmin 4:

1. Open pgAdmin → expand your server → Databases → TmsDb → Schemas →

public → Tables → Students → Columns.

2. You should see a LastUpdated column with type timestamp without time zone.

3. Alternative: run this SQL in pgAdmin’s Query Tool:

**SELECT** column\_name, data\_type

**FROM** information\_schema.**columns**

**WHERE** table\_name = 'Students' **AND** column\_name = 'LastUpdated'; Expected: one row showing LastUpdated | timestamp without time zone. Concurrency

Add a uint Version on Student and configure it in the Fluent API: builder.Property\(s => s.Version\).IsRowVersion\(\);. Migrate.

To test concurrency:

1. Open two browser tabs \(or two terminal windows\) pointing at the same student

record.

2. In Tab A, load the student. In Tab B, load the same student.

3. In Tab A, update the name and save.

4. In Tab B, update the GPA and save.

**Expected:** Tab B’s save throws DbUpdateConcurrencyException because the row version changed when Tab A saved.

**Troubleshooting:**

 **No** **DbUpdateConcurrencyException****:** The Version property is not configured as a

concurrency token. Confirm IsRowVersion\(\) is in your Fluent API config and that the migration added the column.

 **Column type mismatch:** PostgreSQL uses xmin as a system column for row

versioning. The Npgsql provider maps IsRowVersion\(\) to xmin automatically you

do not need to create a separate column. Check the Npgsql concurrency docs if your provider version differs.

 **Shadow property** **LastUpdated** **not appearing:** Migration was not applied. Run

dotnet ef database update and check pgAdmin again.

 

Exercise 9: Bulk archive and soft-delete filter Old enrollments must be archived in bulk. Soft-deleted students should disappear from normal queries but remain recoverable for admins.

Tasks

1. Add HasQueryFilter on Student for \!IsDeleted \(if not already\). Show normal query

vs IgnoreQueryFilters\(\) for an “admin restore” scenario.

2. Use ExecuteUpdateAsync to set IsArchived = true for enrollments older than a

cutoff **without** loading all rows.

*//* **TODO***: ExecuteUpdateAsync with SetProperty match your property names \(Year, EnrolledAt, e tc.\)*

*// await db.Enrollments*

*// .Where\(/\* your predicate \*/\)*

*// .ExecuteUpdateAsync\(s => s.SetProperty\(e => e.IsArchived, true\), cancellationToken\);* **What you should see:**

 SQL log shows a **single** UPDATE "Enrollments" SET "IsArchived" = true WHERE ...

statement, **not** one UPDATE per row.

 Verify in pgAdmin: SELECT COUNT\(\*\) FROM "Enrollments" WHERE "IsArchived" = true;

should match the number of enrollments older than your cutoff.

**Checkpoint:** SQL log shows a single UPDATE \(or provider-equivalent\), not thousands of individual updates.

**Troubleshooting:**

 **ExecuteUpdateAsync** **not found:** This requires EF Core 7\+. Confirm your

Microsoft.EntityFrameworkCore package version is 10.x \(it should be if you followed M4 setup\).

 **IsArchived** **column does not exist:** Add an IsArchived property to your Enrollment

entity, run a migration, and apply it before using ExecuteUpdateAsync.

 **Query filter hides archived rows:** If you added HasQueryFilter\(e => \!e.IsArchived\),

normal queries will exclude archived enrollments. Use IgnoreQueryFilters\(\) in an admin endpoint to see them.

 **IsDeleted** **soft-delete filter not working:** Confirm the filter is applied in

StudentConfiguration and that you are NOT calling IgnoreQueryFilters\(\) in your normal queries.

 

Optional extension: Compiled models

Large models pay startup cost building the model. EF Core can precompile: dotnet ef dbcontext optimize--project ...--startup-project ...

Then wire UseModel\(...\) per EF Core compiled models. Treat this as **stretch** reading not required for module completion.

 

Why this session ends here

The schema you designed in Session 2 now behaves like a production database:

 **Every schema change is reviewable.** Up\(\) and Down\(\) are first-class artefacts. A

teammate who joins next month can read your migrations and understand the history of the schema without you in the room.

 **The N\+1 trap is something you produce on demand and fix on demand.** You can

spot it in code review, in a SQL log, or in a slow-endpoint complaint from the registrar and you know exactly which shaping technique fixes it.

 **Audit and concurrency are policy, not accident.** The shadow LastUpdated column

tracks every change without cluttering your domain types. The row version stops two graders from silently overwriting each other’s grades.

 **Bulk and soft-delete are set-based.** Archiving 50,000 enrollments is one UPDATE

statement, not 50,000. Soft-deleted students disappear from the registrar’s normal queries automatically; admins can still restore them with one filter override.

This is the persistence skillset M6 builds RESTful APIs on top of, M7 layers caching and rate limiting onto, and M11 tests end-to-end. Make sure the verification list below is fully green before you leave M5.

Session 3 Checkpoint

Before completing the module, confirm all five:

☐ At least **two migration files** exist in the project; you can read each Up\(\) and

Down\(\) aloud and explain what it does

☐ The N\+1 demonstration in Exercise 7 Part A produces **1 \+ N** SQL statements in

the log; the fix in Part B produces **1** statement \(or 1 \+ 1 subquery\) both are visible in the same console output

☐ The LastUpdated shadow column exists on Students \(verified in pgAdmin\); the

row-version concurrency test produces a DbUpdateConcurrencyException for the second save

☐ ExecuteUpdateAsync for bulk archive logs a **single** SQL UPDATE statement; the

soft-delete HasQueryFilter hides IsDeleted students from normal queries and IgnoreQueryFilters\(\) brings them back

If any item fails, finish it before moving on.

Lab Verification Full Module

Run your TMS against PostgreSQL with SQL logging on and verify each item: \# Check Session 1 dotnet ef database update succeeds; TmsDb exists in PostgreSQL with 1

Students, Courses, Enrollments

2 All four registrar queries produce shaped SQL \(WHERE, GROUP BY, 1

aggregates\) not SELECT \* then C\# filter

3 Pagination produces SQL with LIMIT 20 OFFSET … for any page number 2 4 Top-5 courses query produces SQL with GROUP BY and ORDER BY ... 2

DESC LIMIT 5

5 Three IEntityTypeConfiguration classes exist; OnModelCreating only 2

contains ApplyConfigurationsFromAssembly

6 OnDelete behaviour is set explicitly in Fluent API; the 2

Course→Enrollment relationship has the chosen behaviour

7 At least two migration files exist; you can read Up\(\) and Down\(\) for 3

the latest one

8 Exercise 7 Part A produces 1 \+ N SQL statements; Part B produces 1 3

\(or 1 \+ 1 subquery\) for the same data

9 ExecuteUpdateAsync produces one bulk UPDATE; HasQueryFilter hides 3

soft-deleted rows from normal queries

Module 6 Lab Session 1: The Predictable Contract

**Module** M6 RESTful Web APIs \(TMS contract\) **Session** 1 of 3

**Exercises** 1 \(First REST Controller\), 2 \(DTOs and Validation\), 3 \(Status Codes

and Business Rule Errors\)

 

Welcome to the API Contract Sprint

In M5 you wired the TMS to a real PostgreSQL database. EF Core knows how to materialise Course, Student, and Enrollment rows; SQL logging shows you the exact SQL behind every LINQ call. The plumbing works. Now imagine handing what you have to an Angular team: there is no CoursesController, the only routes that exist are M4’s EnrollmentsController proof-of-life, and even if there were a controller, it would be returning EF entities with circular Enrollments navigation properties that crash the JSON serialiser.

This session you build the first slice of the API contract that the Angular team in M8 and any future integration partner will actually consume. Three exercises, one controller \(CoursesController\), one supporting controller \(EnrollmentsController\), and one mental model: **for a single resource, the full** **contract should exist before scale concerns enter.** By the end of Session 1 your TMS API answers, for Course and Enrollment:

 Resource-named routes \(/api/courses, /api/courses/\{id\}/enrollments\)

instead of verb-in-URL routes.

 DTO firewall between EF entities and the wire no IsDeleted, no navigation

property leak.

 Data Annotations validation that returns 400 Bad Request with

ValidationProblemDetails automatically.

 A 409 Conflict with ProblemDetails when an admin tries to create a

duplicate course code or enrol into a full course the two business-rule failures this module cares about most.

Pagination, HATEOAS, and Scalar metadata are deliberately not in this session. The full contract for **one** resource has to land first.

Before You Begin Align the M5 Codebase to M6’s Course Shape

M5’s Course entity already has Id, Code, Title, and Capacity \(with Code configured as a unique index in Module 5 Session 2\). For Module 6, we need to perform one column rename to align with standard TMS naming conventions: rename Capacity to MaxCapacity. We will also confirm the unique index configuration for Code is in place.

1. Update the entity

In your M5 entities, edit Course.cs: **public class** Course

\{

**public** int Id \{ **get**; **set**; \}

**public** required string Code \{ **get**; **set**; \}

**public** required string Title \{ **get**; **set**; \}

**public** int MaxCapacity \{ **get**; **set**; \}

**public** ICollection<Enrollment> Enrollments \{ **get**; **set**; \} = \[\]; \}

Confirm that CourseConfiguration has the rules for both properties configured: **public** void Configure\(EntityTypeBuilder<Course> builder\) \{

builder.HasKey\(c => c.Id\);

builder.Property\(c => c.Code\).IsRequired\(\).HasMaxLength\(10\);

builder.Property\(c => c.Title\).IsRequired\(\).HasMaxLength\(200\);

builder.HasIndex\(c => c.Code\).IsUnique\(\);

builder.HasMany\(c => c.Enrollments\).WithOne\(e => e.Course\).HasForei gnKey\(e => e.CourseId\);

\}

The unique index on Code is what makes the duplicate check in Exercise 3 a real business rule and not a courtesy the database itself rejects the second insert. 2. Add and apply the migration

From your solution root:

dotnet ef migrations add RenameCapacityToMaxCapacity Open the generated migration file. You should see a RenameColumn for Capacity → MaxCapacity \(or an AddColumn \+ DropColumn pair if EF could not detect the rename\). Since Code and its unique index are already defined from Module 5, they will not generate any new database changes. If you see a DropTable of Courses followed by CreateTable, **stop** your model has drifted further than this exercise expects. Roll back the model change, fix the entity, and try the migration again. dotnet ef database update

Verify in pgAdmin: Courses table should now show Id, Code, Title, MaxCapacity. 3. Confirm the M4/M5 services and middleware are still wired Open Program.cs in your API project and confirm \(without changing anything yet\) that the M4 baseline is intact:

builder.Services.AddProblemDetails\(\); builder.Services.AddOpenApi\(\);

builder.Services.AddDbContext<TmsDbContext>\(options =>

options.UseNpgsql\(builder.Configuration.GetConnectionString\("TmsDat abase"\)\)\);

builder.Services.AddControllers\(\); var app = builder.Build\(\);

app.UseExceptionHandler\(\);

app.UseStatusCodePages\(\);

**if** \(app.Environment.IsDevelopment\(\)\) \{

app.MapOpenApi\(\);

app.MapScalarApiReference\(\); \}

app.MapControllers\(\);

app.Run\(\);

If AddProblemDetails, UseExceptionHandler, UseStatusCodePages, or MapControllers are missing, fix those before you continue Exercises 2 and 3 rely on the framework returning ValidationProblemDetails automatically and on unhandled exceptions becoming 500 ProblemDetails instead of HTML stack traces.

**Checkpoint:** dotnet build succeeds, the API starts on https://localhost:<your-port>, and https://localhost:<port>/scalar/v1 shows whatever endpoints M4 and M5 left behind. Course rows can be inserted with the new shape via psql or pgAdmin.

Exercise 1: Your First REST Controller \(LO 6.1\) The TMS needs GET /api/courses/\{id\} to fetch a single course and POST /api/courses to create one. Both endpoints must return the correct REST status codes, accept a CancellationToken, and live behind a thin controller that does no business logic.

Step 1 Define the read-side service contract Real .NET teams do not let controllers reach into DbContext directly. They put a service between the controller and persistence so business rules and side effects can be tested in isolation, and so the controller stays focused on HTTP concerns. You will build both halves.

Create Services/ICourseService.cs in your API project: **using** Tms.Api.Entities;

**namespace** Tms.Api.Services;

**public interface** ICourseService \{

Task<Course?> GetByIdAsync\(int id, CancellationToken ct\);

Task<Course> CreateAsync\(Course course, CancellationToken ct\); \}

Create Services/CourseService.cs: **using** Microsoft.EntityFrameworkCore; **using** Tms.Api.Data;

**using** Tms.Api.Entities;

**namespace** Tms.Api.Services;

**public class** CourseService\(TmsDbContext context, ILogger<CourseService>

logger\) : ICourseService

\{

**public** async Task<Course?> GetByIdAsync\(int id, CancellationToken c t\)

\{

*//* **TODO** *1: Use context.Courses.AsNoTracking\(\) //* *and return FirstOrDefaultAsync\(c => c.Id == id, ct\).*

**throw new** NotImplementedException\(\);

\}

**public** async Task<Course> CreateAsync\(Course course, CancellationTo ken ct\)

\{

*//* **TODO** *2: Add course to context.Courses, //* *SaveChangesAsync\(ct\), log info, and return the creat*

*ed course.*

**throw new** NotImplementedException\(\);

\}

\}

Notice three habits already in place: primary-constructor DI for the dependencies, ILogger<CourseService> \(logging belongs at the service boundary, not in controllers\), and CancellationToken on every async method. Step 2 Register the service in Program.cs A service is useless until DI knows about it. Above var app = builder.Build\(\);, register the service **scoped** same lifetime as TmsDbContext, fresh per request: builder.Services.AddScoped<ICourseService, CourseService>\(\); Why scoped, not singleton or transient? CourseService depends on TmsDbContext, which is scoped. Registering the service singleton would capture the DbContext instance forever and crash the second request. Registering it transient would create a new CourseService every time you ask for one in the same request pointless allocation. Scoped matches the unit-of-work boundary EF Core expects.

Step 3 Create the controller

Create Controllers/CoursesController.cs: **using** Microsoft.AspNetCore.Mvc; **using** Tms.Api.Entities;

**using** Tms.Api.Services;

**namespace** Tms.Api.Controllers;

\[ApiController\]

\[Route\("api/courses"\)\]

**public class** CoursesController\(ICourseService courseService\) : Controll erBase

\{

\[HttpGet\("\{id:int\}", Name = nameof\(GetCourseById\)\)\]

**public** async Task<IActionResult> GetCourseById\(int id, Cancellation Token ct\)

\{

*//* **TODO** *3: Call courseService.GetByIdAsync\(id, ct\). //* *Return Ok\(course\) when the result is not null.*

*//* *Return NotFound\(\) when the result is null.* **throw new** NotImplementedException\(\);

\}

\[HttpPost\]

**public** async Task<IActionResult> CreateCourse\(Course course, Cancel lationToken ct\)

\{

*//* **TODO** *4: Call courseService.CreateAsync\(course, ct\). //* *Return CreatedAtAction\(nameof\(GetCourseById\), new \{*

*id = result.Id \}, result\).*

*//* *CreatedAtAction sets the Location header automatical*

*ly.*

**throw new** NotImplementedException\(\);

\}

\}

Three details that matter in production code:

 \[Route\("api/courses"\)\] plural noun, no verbs, exactly the M6 Topic 1

rule.

 \{id:int\} route constraint bad ID like /api/courses/abc returns 404

immediately at the routing layer, never hits your action. Production APIs use route constraints to fail fast.

 Name = nameof\(GetCourseById\) names the route. Exercise 5 uses this

name with LinkGenerator so the HATEOAS self link survives a future route refactor. Adding the name now costs nothing; adding it later means visiting every \[HttpGet\] in the codebase.

Step 4 Verify

1. Run the API. The build must succeed with the NotImplementedException

placeholders intact \(the methods compile they just throw at runtime\).

2. Open https://localhost:<port>/scalar/v1. You should see GET

/api/courses/\{id\} and POST /api/courses listed under whatever group Scalar puts un-tagged endpoints.

3. Once you fill in TODO 3 and TODO 4 \(using the hints no separate worked

solution lives in this handout\), exercise the endpoints:

Step Action Expected Expected body / header

status

1 201 Location: POST /api/courses body

\{ "code": "CSE-101", Created /api/courses/\{newId\} "title": "Web header; body contains the

Development created course entity Fundamentals",

"maxCapacity": 30 \}

2 GET 200 OK \{ "id": …, "code": "CSE-

\(use the ID from step 1\) Development Fundamentals", "maxCapacity": 30, /api/courses/\{newId\} 101", "title": "Web

"enrollments": \[\] \}

3 GET /api/courses/9999 404 Not Empty body or framework

\(assuming no course with Found ProblemDetails shape that ID\)

If step 1 returns 200 OK instead of 201 Created, you used return Ok\(...\) instead of return CreatedAtAction\(...\). If step 1 returns 201 but with no Location header, the nameof\(GetCourseById\) reference is wrong confirm the action method name matches.

Troubleshooting

Symptom Likely cause Fix 404 app.MapControllers\(\) on every Add it after /api/courses missing in Program.cs UseStatusCodePages\(\) , before route app.Run\(\)

 

Exception on first builder.Services.AddScoped<IC registered ourseService, InvalidOperation ICourseService not Add

POST

CourseService>\(\); in Program.cs

Scalar shows AddOpenApi\(\) / Re-check the M4 wiring listed in nothing under MapOpenApi\(\) / **Before You Begin** /scalar/v1 MapScalarApiReference

\(\) missing

POST returns 200 Used Ok\(result\) instead Replace with the OK CreatedAtAction\(nameof\(GetCou instead of 201 of

CreatedAtAction\(...\) rseById\), new \{ id =

result.Id \}, result\) pattern

404 on the GET \[HttpGet\("\{id\}"\)\] does Both must use the same action that is supposed not match the name and the same route name Symptom Likely cause Fix to return \[HttpGet\("\{id:int\}", 200

Name = ...\)\] you used in CreatedAtAction

 

Exercise 2: DTOs and Input Validation \(LO 6.2\) Two issues hit during a peer review of Exercise 1. First, the Course entity has an Enrollments navigation property returning that to the wire produces a circular reference crash the moment a course has even one enrollment. Second, an admin clicked through Scalar and successfully created a course with an empty title the API accepted whatever JSON arrived. Fix both with a DTO firewall and Data Annotations.

Step 1 Define the response DTO

Create Dtos/CourseResponseDto.cs: **namespace** Tms.Api.Dtos;

**public** record CourseResponseDto\(

int Id,

string Code,

string Title,

int MaxCapacity,

int EnrollmentCount\);

A record is the right shape here DTOs are immutable value containers, and the positional syntax keeps the type to a single line. EnrollmentCount is computed at the query boundary, so the wire format never includes a navigation property. Step 2 Define the request DTO with validation Create Dtos/CreateCourseRequest.cs: **using** System.ComponentModel.DataAnnotations;

**namespace** Tms.Api.Dtos;

**public** record CreateCourseRequest \{

\[Required, RegularExpression\(@"^\[A-Z\]\{3\}-\\d\{3\}$",

ErrorMessage = "Code must follow the pattern XXX-000 \(e.g., CSE

-101\)."\)\]

**public** required string Code \{ **get**; init; \}

\[Required, MaxLength\(200\)\]

**public** required string Title \{ **get**; init; \}

\[Range\(1, 200\)\]

**public** int MaxCapacity \{ **get**; init; \} \}

The required modifier and init accessors are the modern C\# pattern: the compiler refuses to construct a CreateCourseRequest without Code and Title, but neither field can be reassigned after binding. The \[ApiController\] attribute on the controller wires up automatic validation against these annotations you do not\#\#\# Step 3 Update the service contract and projection Now that you have defined the DTOs, update the service interface Services/ICourseService.cs to return and accept them: **using** Tms.Api.Dtos;

**namespace** Tms.Api.Services;

**public interface** ICourseService \{

Task<CourseResponseDto?> GetByIdAsync\(int id, CancellationToken ct\);

Task<CourseResponseDto> CreateAsync\(CreateCourseRequest request, Ca ncellationToken ct\);

\}

Open Services/CourseService.cs. Update its signature and replace its method bodies to map from/to these DTOs:

**public** Task<CourseResponseDto?> GetByIdAsync\(int id, CancellationToken ct\) =>

context.Courses

.AsNoTracking\(\)

.Where\(c => c.Id == id\) .Select\(c => **new** CourseResponseDto\(

c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count\)\)

.FirstOrDefaultAsync\(ct\);

Two production habits visible here: AsNoTracking\(\) \(read paths never need change tracking saves CPU and memory in EF\) and Select\(...\) projection at the query layer \(EF translates c.Enrollments.Count into a SQL COUNT\(\*\) subquery; you do **not** load every enrollment row into memory just to compute a number\). For the create path, the cleanest pattern is to insert the entity, save, then re-query through GetByIdAsync so the response uses the same projection: **public** async Task<CourseResponseDto> CreateAsync\(CreateCourseRequest re quest, CancellationToken ct\)

\{

var course = **new** Course

\{

Code = request.Code,

Title = request.Title,

MaxCapacity = request.MaxCapacity

\};

context.Courses.Add\(course\);

await context.SaveChangesAsync\(ct\);

logger.LogInformation\("Created course \{CourseId\} \(\{Code\}\)", course. Id, course.Code\);

**return** \(await GetByIdAsync\(course.Id, ct\)\)\!; \}

The null\! forgiveness on the return value is safe here because we just inserted the row and saved it cannot be missing on the next read. The log line is the kind of breadcrumb you want in production: a single info log per write, never a log per read.

Step 4 Refactor the controller to use DTOs

Open Controllers/CoursesController.cs and refactor it to accept and return the DTOs instead of raw entities. Make sure to import using Tms.Api.Dtos;: **using** Microsoft.AspNetCore.Mvc; **using** Tms.Api.Dtos;

**using** Tms.Api.Services;

**namespace** Tms.Api.Controllers;

\[ApiController\]

\[Route\("api/courses"\)\]

**public class** CoursesController\(ICourseService courseService\) : Controll erBase

\{

\[HttpGet\("\{id:int\}", Name = nameof\(GetCourseById\)\)\]

**public** async Task<IActionResult> GetCourseById\(int id, Cancellation Token ct\)

\{

var course = await courseService.GetByIdAsync\(id, ct\); **return** course **is** not **null** ? Ok\(course\) : NotFound\(\);

\}

\[HttpPost\]

**public** async Task<IActionResult> CreateCourse\(CreateCourseRequest r equest, CancellationToken ct\)

\{

var result = await courseService.CreateAsync\(request, ct\); **return** CreatedAtAction\(nameof\(GetCourseById\), **new** \{ id = result.

Id \}, result\);

\}

\}

By binding to CreateCourseRequest, the \[ApiController\] attribute automatically triggers model validation against the Data Annotations you set in Step 2. Step 5 Verify

Step Action Expecte Expected body

d status

1 POST /api/courses 400 Bad ValidationProblemDetails JSON Request body \{\} with errors.Code , errors.Title ,

errors.MaxCapacity field arrays

 

2 POST /api/courses 400 Bad errors.Code mentions the XXX-000 Request body \{ "code": "abc", pattern, errors.MaxCapacity "title": "x", mentions the 1–200 range "maxCapacity": 9999 \}

 

3 POST /api/courses 201 \{ "id": …, "code": "CSE-102", Created "title": "TypeScript body \{ "code": "CSE- Essentials", "maxCapacity": 25, 102", "title": "enrollmentCount": 0 \} "TypeScript Essentials", "maxCapacity": 25 \}

4 GET /api/courses/\{id\} 200 OK The same DTO shape and crucially

from step 3 **no** enrollments array, **no**

isDeleted , **no** navigation property

The minimum bar to clear here is step 4. If your response includes any property that is not on CourseResponseDto, your service is still returning a Course entity somewhere find the path and project before returning.



Troubleshooting

Symptom Likely cause Fix POST \{\} returns 200 Missing Add \[ApiController\] it enables OK instead of 400 \[ApiController\] on automatic model validation

the controller

Response body Returning the Confirm every return path goes contains Course entity through CourseResponseDto enrollments array directly somewhere projection JsonException: A Same as above DTO projection eliminates the possible object entity leak cycle; never serialise navigation cycle was detected properties to clients Regex rejects cse- Wrong-case input The regex ^\[A-Z\]\{3\}-\\d\{3\}$ is 101 case-sensitive on purpose;

uppercase is the contract

Validation passes \[Range\(1, 200\)\] Confirm the attribute is on when MaxCapacity not applied CreateCourseRequest.MaxCapacity is

0 and DTO is the bound type

 

Exercise 3: Status Codes and Business Rule Errors \(LO 6.3\) The Exercise 1 \+ 2 path handles input validation cleanly but breaks on two business rule failures. First, posting a duplicate course code returns 500 Internal Server Error because the unique-index INSERT from your M5 migration crashes inside SaveChangesAsync. Second, when the M4 in-memory EnrollmentService is replaced with a database-backed enrolment endpoint, enrolling into a full course should return 409 Conflict instead it crashes the same way. You fix both by **checking the rule before the database does**. Step 1 Add the duplicate-code check to the service Extend ICourseService.cs:

**public interface** ICourseService \{

Task<CourseResponseDto?> GetByIdAsync\(int id, CancellationToken ct\);

Task<CourseResponseDto> CreateAsync\(CreateCourseRequest request, Ca ncellationToken ct\);

Task<bool> CodeExistsAsync\(string code, CancellationToken ct\); \}

In CourseService.cs:

**public** Task<bool> CodeExistsAsync\(string code, CancellationToken ct\) =>

context.Courses.AsNoTracking\(\).AnyAsync\(c => c.Code == code, ct\); AnyAsync is the right call EF translates it to SELECT EXISTS \(SELECT 1 ... LIMIT 1\), which stops at the first row.

Step 2 Have the controller return 409 Conflict In CoursesController.cs, replace TODO 4 \(CreateCourse\) with the pre-check pattern:

\[HttpPost\]

**public** async Task<IActionResult> CreateCourse\(CreateCourseRequest reque st, CancellationToken ct\)

\{

*//* **TODO** *1: Call courseService.CodeExistsAsync\(request.Code, ct\).*

*//* *If it returns true, return Conflict\(new ProblemDetails*

*\{ ... \}\) with:*

*//* *Title = "Course code already exists"*

*//* *Detail = $"A course with code '\{request.Code\}' is alre*

*ady registered."*

*//* *Status = StatusCodes.Status409Conflict*

*//* *You do not need a try/catch the framework's ProblemDeta*

*ils middleware handles unhandled exceptions.*

var result = await courseService.CreateAsync\(request, ct\);

**return** CreatedAtAction\(nameof\(GetCourseById\), **new** \{ id = result.Id

\}, result\);

\}

**Bad Practices:** returning 200 OK for an error condition \(Status Smokescreen\) and a 409 without a valid ProblemDetails body \(Missing Contract\). Both are observable just by reading the response in Scalar.

Step 3 Add the enrolment side

Create the request DTO at Dtos/EnrollStudentRequest.cs: **using** System.ComponentModel.DataAnnotations;

**namespace** Tms.Api.Dtos;

**public** record EnrollStudentRequest \{

\[Range\(1, int.MaxValue, ErrorMessage = "StudentId must be a positiv e integer."\)\]

**public** required int StudentId \{ **get**; init; \} \}

Create the response DTO at Dtos/EnrollmentResponseDto.cs: **namespace** Tms.Api.Dtos;

**public** record EnrollmentResponseDto\(

int Id,

int CourseId,

int StudentId,

DateTime EnrolledAt\);

Create Services/IEnrollmentService.cs: **namespace** Tms.Api.Services;

**public interface** IEnrollmentService \{

Task<EnrollmentResponseDto?> GetByIdAsync\(int courseId, int id, Can cellationToken ct\);

Task<EnrollmentResponseDto> CreateAsync\(int courseId, EnrollStudent Request request, CancellationToken ct\); \}

Create Services/EnrollmentService.cs. Two of the three method bodies are scaffolding \(constructor \+ read path\); the **graded** business decision the order of the look-ups, which 4xx code to choose at which gate is yours: **public class** EnrollmentService\(TmsDbContext context, ILogger<Enrollment Service> logger\) : IEnrollmentService \{

**public** Task<EnrollmentResponseDto?> GetByIdAsync\(int courseId, int id, CancellationToken ct\) =>

context.Enrollments

.AsNoTracking\(\)

.Where\(e => e.Id == id && e.CourseId == courseId\) .Select\(e => **new** EnrollmentResponseDto\(e.Id, e.CourseId, e.

StudentId, e.EnrolledAt\)\)

.FirstOrDefaultAsync\(ct\);

**public** async Task<EnrollmentResponseDto> CreateAsync\(int courseId, EnrollStudentRequest request, CancellationToken ct\)

\{

*//* **TODO** *2: Insert a new Enrollment with CourseId = courseId, St*

*udentId = request.StudentId,*

*//* *and EnrolledAt = DateTime.UtcNow. SaveChangesAsync\(c*

*t\), log info, then re-read*

*//* *through GetByIdAsync\(courseId, enrollment.Id, ct\).*

**throw new** NotImplementedException\(\);

\}

\}

Register it scoped in Program.cs next to the course service: builder.Services.AddScoped<IEnrollmentService, EnrollmentService>\(\);

Step 4 Build the nested controller

Create Controllers/EnrollmentsController.cs: **using** Microsoft.AspNetCore.Mvc; **using** Tms.Api.Dtos;

**using** Tms.Api.Services;

**namespace** Tms.Api.Controllers;

\[ApiController\]

\[Route\("api/courses/\{courseId:int\}/enrollments"\)\] **public class** EnrollmentsController\(

ICourseService courseService,

IEnrollmentService enrollmentService\) : ControllerBase \{

\[HttpGet\("\{id:int\}", Name = nameof\(GetEnrollment\)\)\]

**public** async Task<IActionResult> GetEnrollment\(int courseId, int id, CancellationToken ct\)

\{

var enrollment = await enrollmentService.GetByIdAsync\(courseId,

id, ct\);

**return** enrollment **is** not **null** ? Ok\(enrollment\) : NotFound\(\);

\}

\[HttpPost\]

**public** async Task<IActionResult> EnrollStudent\(int courseId, Enroll StudentRequest request, CancellationToken ct\)

\{

*//* **TODO** *3: Look up the parent course \(courseService.GetByIdAsyn*

*c\). If null, return NotFound\(\).*

*//* *Then check capacity \(course.EnrollmentCount >= cours*

*e.MaxCapacity\).*

*//* *If full, return Conflict\(new ProblemDetails \{ ... \}\)*

*with:*

*//* *Title = "Course is full" //* *Detail = $"Course '\{course.Title\}' has reached its*

*maximum capacity of \{course.MaxCapacity\}."*

*//* *Status = StatusCodes.Status409Conflict //* *Otherwise, call enrollmentService.CreateAsync and re*

*turn CreatedAtAction\(nameof\(GetEnrollment\),*

*//* *new \{ courseId, id = enrollment.Id \}, enrollment\).*

**throw new** NotImplementedException\(\);

\}

\}

The order matters: **404 before 409**. A client posting into a course that does not exist deserves the 404, not a 409 about a course that was never there. Step 5 Verify

Step Action Expected Expected body

status

 

1 409 \{ "title": "Course code POST /api/courses with a Conflict already exists", code from earlier \(e.g. CSE-101 \) "status": 409, "detail": "A course with code 'CSE-101' …" \}

 

2 201 Create a fresh course CSE-103 Location header points at Created with maxCapacity: 1 . Then GetEnrollment ; body is the POST new /api/courses/\{id\}/enrollmen EnrollmentResponseDto ts with body \{ "studentId": 1 \}

 

3 409 \{ "title": "Course is Repeat step 2’s POST \(so Conflict full", "status": 409, course CSE-103 is now at 1/1\) "detail": "Course '…' has reached its maximum capacity of 1." \}

 

4 POST 404 Not Empty or framework /api/courses/9999/enrollmen Found ProblemDetails body ts

body \{ "studentId": 1 \}

\(Step 2 assumes Student with Id = 1 exists from M5’s seed data. If it does not, create one in pgAdmin or pick an existing StudentId.\)



Troubleshooting

Symptom Likely cause Fix POST with CodeExistsAsync not Add the pre-check in the controller duplicate code Conflict\(new ProblemDetails called; DB unique returns \{ ... \}\) before calling CreateAsync 500 constraint hit instead

409 body is Conflict\(\) called Pass new ProblemDetails \{ Title = empty "...", Status = 409, Detail = \{\} with no argument "..." \}

Nested route \[Route\("api/courses/\{courseId:in Route template

never matches t\}/enrollments"\)\] ↔ int courseId parameter name and

action parameter parameter names must match name disagree exactly

Enrolling into a Capacity check ran Reorder: parent look-up → missing course before the parent NotFound\(\) if null → capacity check returns 409 look-up → Conflict\(\) if full instead of 404

EnrollmentCount CourseService.GetBy The projection in Exercise 2 reads always reads 0 on IdAsync does not c.Enrollments.Count from the the parent fetch project a fresh count database every call confirm you re-

fetched the course inside the

enrolment action, not a stale variable

from before the insert

 

Why this session ends here

You just shipped the contract for one TMS resource end-to-end:

 **Routes name resources, not actions.** A new developer reading

\[Route\("api/courses"\)\] knows without any documentation that GET lists, POST creates, and \{id\} is the single-item path. The \{id:int\} constraint stops malformed IDs at the routing layer.

 **The wire and the database are different things.** CourseResponseDto is

what a client sees; Course is what EF persists. A future migration that adds IsDeleted or LastUpdatedBy does not leak through the API. A migration that renames Title does but only because the DTO references Title by name, which is exactly the seam where the contract is supposed to break visibly.

 **Validation is declarative and consistent.** Data Annotations \+

\[ApiController\] produce 400 ValidationProblemDetails for free. You did not write if \(string.IsNullOrEmpty\(...\)\) even once.

 **Business rules return** **409****, not** **500****.** A duplicate code or a full course is a

known, expected failure; the controller checks the rule, returns Conflict\(new ProblemDetails \{ ... \}\), and the integration team’s HTTP client catches it as an error instead of trusting a 200 and corrupting state.

You did **not** build pagination, links, or rich Scalar metadata yet. That is deliberate. Session 2 takes the same CoursesController and bolts on safe collection retrieval that is where pagination earns its place. Session 3 makes the response self-describing for the Angular team and any integration partner. Today’s deliverable is the contract for one resource, lit up across all the failure modes a real client will hit on day one.

Session 1 Checkpoint

Before moving to Session 2, confirm all five rows pass against your live API: Check Pass means POST /api/courses with a valid body 201 Created, Location header points at

GET /api/courses/\{newId\}, body is CourseResponseDto

POST /api/courses with empty \{\} 400 Bad Request, body is body ValidationProblemDetails with field-

level errors

POST /api/courses with a code that 409 Conflict, body is ProblemDetails already exists with Title = "Course code already

exists"

POST 409 Conflict, body is ProblemDetails /api/courses/\{id\}/enrollments into with Title = "Course is full" a course where EnrollmentCount >= MaxCapacity

Any course response inspected in No enrollments array, no isDeleted, no Scalar other navigation property only the five

DTO fields

If any row fails, fix it before Session 2. Pagination assumes the contract for a single resource is solid; you cannot count what you cannot return cleanly.

Module 6 Lab Session 2: Scale and Cross-Cutting **Module** M6 Building RESTful Web APIs **Session** 2 of 3

**Exercises** 4 \(Pagination, Filtering, and the Audit Filter\)

 

Prerequisites Check Before You Start

Your TMS API must already have:

 CoursesController with GET /api/courses/\{id:int\} and POST

/api/courses returning the right status codes \(Session 1 Exercises 1 \+ 3\).

 CourseResponseDto projected at the query layer with AsNoTracking\(\)

\(Session 1 Exercise 2\).

 EnrollmentsController with the nested route

api/courses/\{courseId:int\}/enrollments and the 404 → 409 ordering \(Session 1 Exercise 3\).

 ICourseService and IEnrollmentService registered scoped in Program.cs.

If any item is missing, finish Session 1 first. Pagination is a service-layer change on top of a working CourseService, not a parallel rewrite.

 

What This Session Is Really About

Session 1 built the contract for a single resource. Today the contract has to survive load.

The lab opens with one demo: GET /api/courses returning every row in the Courses table at once. With the M5 seed of three or four courses it is fine. With the deterministic seed you add at the start of this session twenty-five courses, all uniquely coded the response is still small, but the **architecture** of the call is wrong: there is no upper bound on what the database returns. Production data has a way of finding the largest SELECT \* in the codebase. So today you build the bounded version. One controller action, one service method, one supporting filter, one seeder:

 **PagedRequest** the input contract. Page number, page size with a hard cap,

optional search term, optional ordering.

 **PagedResponse<T>** the output contract. Items plus the metadata Angular

needs to render pagination controls without a second round-trip.

 **GetCoursesAsync** the service method that ties it all together: filter, count,

sort, page, project. The order is the lesson.

 **AuditLogFilter** a registered global action filter that logs every API call

and its status code. Cross-cutting, not business.

 **DataSeeder** a deterministic seed of twenty-five courses so verification is

reproducible across cohorts.

By the end of this session, GET /api/courses?page=2&pageSize=10&search=fund returns ten or fewer items, the right totalCount, the right totalPages, and a single SQL statement \(or two one count, one page\) in the EF log. That, not the response body, is the proof.

 

Before You Begin Add the Deterministic Seeder Pagination verification is impossible without data. M5’s seed is too small \(three or four courses\) to demonstrate paging boundaries; manual POST to twenty-five courses through Scalar is a forty-five-minute scavenger hunt that no production team would tolerate. Real .NET teams ship a seeder. So will you. 1. Create Data/DataSeeder.cs in your API project **using** Microsoft.EntityFrameworkCore;

**namespace** Tms.Api.Persistence;

**public static class** DataSeeder

\{

**private static readonly** \(string Code, string Title, int MaxCapacity\) \[\] Courses =

\[

\("CSE-101", "Web Development Fundamentals", 30\), \("CSE-102", "TypeScript Essentials", 30\), \("CSE-103", "Git and Collaborative Workflows", 25\), \("CSE-201", "ASP.NET Core Fundamentals", 28\), \("CSE-202", "Entity Framework Core and PostgreSQL", 28\), \("CSE-203", "Building RESTful Web APIs", 28\), \("CSE-301", "Advanced Web API Patterns", 24\), \("CSE-302", "Angular Fundamentals", 26\), \("CSE-303", "Angular Advanced", 24\), \("CSE-304", "Full-Stack Integration", 22\), \("CSE-305", "Testing and Quality Assurance", 22\), \("CSE-306", "Security and Authentication", 20\),

\("DAT-101", "Database Design Foundations", 30\), \("DAT-201", "Advanced SQL and Indexing", 26\), \("DAT-202", "Data Modelling for the Web", 26\), \("ARC-101", "Software Architecture Patterns", 22\), \("ARC-201", "Cloud-Native Architecture", 22\), \("DEV-101", "DevOps Foundations", 24\), \("DEV-201", "Continuous Delivery Pipelines", 22\), \("MOB-101", "Mobile App Foundations", 24\), \("MOB-201", "Cross-Platform Mobile", 22\), \("AI-101", "Applied Machine Learning", 20\), \("AI-201", "Generative AI for Developers", 18\), \("UX-101", "UX Research and Wireframing", 24\), \("UX-201", "Design Systems and Tokens", 22\),

\];

**public static** async Task SeedAsync\(TmsDbContext context, Cancellati onToken ct = **default**\)

\{

await context.Database.MigrateAsync\(ct\); **if** \(await context.Courses.AnyAsync\(ct\)\) \{

**return**;

\}

**foreach** \(var \(code, title, maxCapacity\) **in** Courses\) \{

context.Courses.Add\(**new** Course \{

Code = code,

Title = title,

MaxCapacity = maxCapacity

\}\);

\}

await context.SaveChangesAsync\(ct\);

\}

\}

Three production habits in this seeder. First, the data is **deterministic** the same twenty-five rows on every cohort, every machine. Random data with Bogus looks impressive but turns lab verification into a guessing game \(“does step 4 expect 47 or 52 results?”\). Second, the seeder is **idempotent** running it twice does not double-insert. Third, the seeder calls MigrateAsync first; on a fresh dev database, schema and data come up in one call.

2. Run the seeder on application startup, only in Development Open Program.cs. Just before app.Run\(\);, add: **if** \(app.Environment.IsDevelopment\(\)\) \{

**using** var scope = app.Services.CreateScope\(\);

var context = scope.ServiceProvider.GetRequiredService<TmsDbContex t>\(\);

await DataSeeder.SeedAsync\(context\); \}

The IsDevelopment\(\) gate matters. You do **not** want this code to run in production staging and production data is owned by the operations team, not the seed file. The using var scope = … pattern is the recommended way to resolve a scoped service \(TmsDbContext is scoped\) outside of a request without it, you would resolve the context from the root provider and leak a connection. 3. Verify the seed

Run the API. Confirm in pgAdmin \(or psql\): **SELECT** COUNT\(\*\) **FROM** "Courses"; Expected: 25. If you already had courses from Session 1’s manual POST requests, the seeder skipped the seed \(the idempotency guard\). Truncate the table or drop the database and re-run if you want a clean baseline: **TRUNCATE** "Courses" RESTART IDENTITY **CASCADE**; \(Adjust if your migration uses a different schema or table name. RESTART IDENTITY resets the auto-increment so seeded IDs start at 1 again.\) **Checkpoint:** SELECT COUNT\(\*\) FROM "Courses"; returns 25 \(or your Session 1 manual rows \+ 25 if you did not truncate\). The API still starts cleanly.

 

Exercise 4: Pagination, Filtering, and the Audit Filter \(LO 6.4\) **Scenario:** Yared opens the dashboard and runs GET /api/courses. Today it returns twenty-five rows; in three semesters it will return five hundred. The browser will not freeze yet, but the **architecture** is wrong every page-load downloads the entire table. You make pagination mandatory, with a capped page size, a stable sort, and a PagedResponse<T> shape Angular can render without a second round-trip. Then you register a global AuditLogFilter so every API call is logged with its status code, and the cross-cutting concern stays out of every controller action.

This exercise has four parts. Do them in order Part B depends on Part A’s types, Part C depends on Part B’s controller action, Part D is independent but easier once the paginated path is producing log lines.

Part A Define the pagination contracts

Create Dtos/PagedRequest.cs:

**namespace** Tms.Api.Dtos;

**public** record PagedRequest

\{

**private** const int MaxPageSize = 50;

**private** int \_pageSize = 20;

**public** int Page \{ **get**; init; \} = 1;

**public** int PageSize

\{

get => \_pageSize;

init => \_pageSize = value < 1 ? 20 : value > MaxPageSize ? MaxP

ageSize : value;

\}

**public** string? Search \{ **get**; init; \}

**public** string OrderBy \{ **get**; init; \} = "Title";

**public** bool Descending \{ **get**; init; \} \}

Three details that earn their place in production code. The MaxPageSize constant is a single source of truth never inlined as a magic 50. The PageSize setter clamps both ends: a hostile client sending ?pageSize=10000 lands on 50; a confused client sending ?pageSize=0 lands on the default 20. And OrderBy is a plain string with a default Part C decides which subset of OrderBy values is actually safe to translate into LINQ \(you do **not** want clients smuggling arbitrary strings into your query\).

Create Dtos/PagedResponse.cs:

**namespace** Tms.Api.Dtos;

**public** record PagedResponse<T>

\{

**public** required IReadOnlyList<T> Items \{ **get**; init; \}

**public** required int TotalCount \{ **get**; init; \}

**public** required int Page \{ **get**; init; \}

**public** required int PageSize \{ **get**; init; \}

**public** int TotalPages => \(int\)Math.Ceiling\(TotalCount / \(double\)Pag eSize\);

**public** bool HasPrevious => Page > 1;

**public** bool HasNext => Page < TotalPages; \}

TotalPages, HasPrevious, HasNext are computed properties the wire format will include them in the JSON because they are public getters, but the writer never has to set them. The Angular pagination control reads hasNext and hasPrevious to enable or disable the next/previous buttons, and reads totalPages to render Page 2 of 5. All in one response.

Part B Add the paginated GET action

Open Controllers/CoursesController.cs. The new action sits next to GetCourseById. Both can coexist because attribute routing distinguishes \[HttpGet\] \(collection\) from \[HttpGet\("\{id:int\}", ...\)\] \(single item\): \[HttpGet\]

**public** async Task<IActionResult> GetCourses\(

\[FromQuery\] PagedRequest request, CancellationToken ct\) \{

var result = await courseService.GetCoursesAsync\(request, ct\);

**return** Ok\(result\);

\}

\[FromQuery\] binds the request from query-string parameters ?page=2&pageSize=10&search=fund becomes a populated PagedRequest. Without \[FromQuery\], ASP.NET Core would try to bind from the body, which is wrong for a GET.

Part C Implement GetCoursesAsync in the service Extend ICourseService.cs:

**public interface** ICourseService \{

Task<CourseResponseDto?> GetByIdAsync\(int id, CancellationToken ct\);

Task<CourseResponseDto> CreateAsync\(CreateCourseRequest request, Ca ncellationToken ct\);

Task<bool> CodeExistsAsync\(string code, CancellationToken ct\); builder.Services.AddControllers\(options => \{

Task<PagedResponse<CourseResponseDto>> GetCoursesAsync\(PagedRequest request, CancellationToken ct\); \}

In CourseService.cs, this is the **graded** part of the lab the order of LINQ operations is exactly what the integrity-lab Fake Pagination deduction \(-5 if TotalCount is computed after Skip/Take\) inspects. The hints below tell you what each step does; the assembly is yours:

**public** async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync\(

PagedRequest request, CancellationToken ct\) \{

*//* **TODO** *1: Start with a no-tracking IQueryable<Course>:*

*//* *IQueryable<Course> query = context.Courses.AsNoTracking\(\);*

*//* **TODO** *2: If request.Search has a value, append a Where clause:*

*//* *query = query.Where\(c => EF.Functions.ILike\(c.Title, $"%\{reque*

*st.Search\}%"\)*

*//* *|| EF.Functions.ILike\(c.Code, $"%\{reque*

*st.Search\}%"\)\);*

*//* *ILike is the case-insensitive LIKE in PostgreSQL using it her*

*e means*

*//* *the search "fund" finds "Web Development Fundamentals" without*

*learners*

*//* *being surprised by case-sensitivity at lab time.*

*//* **TODO** *3: Count BEFORE paging:*

*//* *var totalCount = await query.CountAsync\(ct\);*

*//* *This produces one SELECT COUNT\(\*\) statement. If you Count afte*

*r Skip/Take,*

*//* *you would get the count of the page, not the total.*

*//* **TODO** *4: Apply OrderBy, then Skip/Take, then Select projection.*

*//* *For OrderBy, branch on request.OrderBy* ∈ *\{ "Title", "Code", " MaxCapacity" \}*

*//* *and apply Descending if request.Descending. Reject unknown Ord*

*erBy values*

*//* *silently by falling back to "Title" never let an arbitrary st*

*ring*

*//* *into the LINQ tree.*

*//* **TODO** *5: Materialise:*

*//* *var items = await sortedQuery*

*//* *.Skip\(\(request.Page - 1\) \* request.PageSize\)*

*//* *.Take\(request.PageSize\)*

*//* *.Select\(c => new CourseResponseDto\(c.Id, c.Code, c.Title,*

*c.MaxCapacity, c.Enrollments.Count\)\)*

*//* *.ToListAsync\(ct\);*

*//* **TODO** *6: Return new PagedResponse<CourseResponseDto> \{ Items = it ems, TotalCount = totalCount, Page = request.Page, PageSize = request.P ageSize \};*

**throw new** NotImplementedException\(\); \}

A note on the SQL log: with logging on, you should see exactly two statements per request one SELECT COUNT\(\*\) and one SELECT … LIMIT 10 OFFSET 10 \(or whatever your PageSize and Page produce\). If you see twenty-six statements, the Select projection is happening in C\# memory instead of SQL confirm the projection is **inside** the IQueryable chain and not after a .ToListAsync\(\). Part D Add the global audit filter

Cross-cutting concerns live in filters, not in every action. The audit log line “TMS API call: GET /api/courses → 200” is the smallest possible breadcrumb a production operations team needs to answer the question “did the dashboard try to call us at 14:03?”.

Create Filters/AuditLogFilter.cs: **using** Microsoft.AspNetCore.Mvc.Filters; **namespace** Tms.Api.Filters;

**public class** AuditLogFilter\(ILogger<AuditLogFilter> logger\) : IActionFi lter

\{

**public** void OnActionExecuting\(ActionExecutingContext context\)

\{

var route = context.HttpContext.Request.Path; var method = context.HttpContext.Request.Method; logger.LogInformation\("TMS API call: \{Method\} \{Route\}", method,

route\);

\}

**public** void OnActionExecuted\(ActionExecutedContext context\)

\{

var status = context.HttpContext.Response.StatusCode; logger.LogInformation\("TMS API response: \{StatusCode\}", status\);

\}

\}

Register it globally in Program.cs:

options.Filters.Add<AuditLogFilter>\(\); \}\);

Add<T>\(\) \(the generic overload\) registers the filter type so DI can resolve ILogger<AuditLogFilter> for you. Add\(new AuditLogFilter\(...\)\) would force you to construct the logger by hand unnecessary.

The boundary that matters in the deck and in interviews: filters are for **cross-****cutting** concerns. If you find yourself writing if \(course.IsFull\) inside a filter, that decision belongs in EnrollmentService the M6 Topic 6 Socratic question is “what goes wrong if an action filter starts checking ‘is this course full?’” The answer: the filter becomes part of the business contract, gets duplicated as EnrollmentService evolves, and breaks the moment two calls disagree. Step 5 Verify

Step Action Expected Expected body / observation

status

 

1 GET 200 OK items.length == 10, totalCount == 25, /api/courses? totalPages == 3 , hasNext == true , page=1&pageSi hasPrevious == false ze=10

 

2 GET 200 OK items.length == 5, totalCount == 25, /api/courses? hasNext == false , hasPrevious == true page=3&pageSi

ze=10

3 GET 200 OK pageSize == 50 in the response the cap

/api/courses? kicked in pageSize=9999

 

4 GET 200 OK One item: CSE-101 Web Development /api/courses? Fundamentals . totalCount == 1 . \( fund search=fund matches Fundamentals case-insensitively.\)

 

5 GET 200 OK First item is UX-201 \(alphabetically last by /api/courses? code\), descending order orderBy=Code& descending=tr ue&pageSize=5

6 Tail the API \(logs\) Two log lines per request: TMS API call:

console while GET /api/courses and TMS API step 1 runs response: 200

7 Tail EF Core \(SQL\) One SELECT COUNT\(\*\) FROM "Courses"

SQL logs while and one SELECT ... LIMIT 10 OFFSET 0 Step Action Expected Expected body / observation

status

step 1 runs exactly two statements \(M5 logging

habit\)

For step 4, if your local PostgreSQL collation is case-sensitive and the search returns zero results, the EF.Functions.ILike call recommended in TODO 2 fixes it Like is case-sensitive on PostgreSQL by default, ILike is the case-insensitive sibling. If your team is on SQL Server, the equivalent is EF.Functions.Like\(c.Title, $"%\{request.Search\}%"\) which is collation-dependent \(usually case-insensitive\).

For step 7, if you see twenty-six statements, the Select projection is happening in memory re-read the order: IQueryable → Where → CountAsync → OrderBy → Skip → Take → Select → ToListAsync. The projection must stay inside the IQueryable chain.

Troubleshooting

Symptom Likely cause Fix

 

equals after Skip/Take matters. This is the Fake Pagination items.Count totalCount CountAsync called Count first, then skip/take/project order

deduction in the integrity lab.

instead of the

table total

 

returns 9999 1 ? 20 : value > MaxPageSize ? cap missing MaxPageSize : value; pageSize=9999 Property setter Confirm init => \_pageSize = value < items

?search=fund PostgreSQL Switch to EF.Functions.ILike\(...\) returns nothing collation is case- \(PostgreSQL-specific, case-insensitive\)

sensitive; Like is case-sensitive

?orderBy=DROP OrderBy string Whitelist the allowed columns \(Title, TABLE produces a passed straight Code, MaxCapacity\); fall back to Title for runtime into a dynamic anything else exception LINQ helper

 

match \[HttpGet\("\{id:int\}", Name = GET decorated with nameof\(GetCourseById\)\)\] /api/courses/5 Two routes both Both actions Single-item action must be

bare \[HttpGet\]

Audit filter logs Filter applied to Filters only run on requests that reach a Symptom Likely cause Fix every static-file non-controller controller action if you see static-file request endpoints \(it logs, you wired this as middleware, not a

should not\) filter

EF logs show 26 Projection runs Move the Select\(c => new statements per after ToListAsync CourseResponseDto\(...\)\) **before** page request ToListAsync\(ct\)

 

Why this session ends here

The dashboard now scales:

 **Collections are bounded by default.** A new endpoint added next

semester that forgets pagination is a code-review failure, not a production incident because every existing collection action takes \[FromQuery\] PagedRequest, the team eye learns to flag the absence.

 **The page-size cap is server-controlled, not client-honour-**

**system.** ?pageSize=10000 lands on fifty. The Angular team can request whatever feels right; the server protects itself.

 **The order of LINQ operations is the lesson, not an implementation**

**detail.** Filter, count, sort, skip/take, project. Every senior interview asks at least one variant of this and now you can answer it with the file path of your own service method as proof.

 **Cross-cutting logic stays cross-cutting.** Every API call produces two log

lines, regardless of which controller handled it. Add a thirty-first controller next quarter it logs without anyone editing the seeder, the controller, or the filter.

You did **not** add HATEOAS links to the paginated response. That is deliberate. Links belong on the detail endpoint where a client picks one resource and asks “what can I do with this?” a list of fifty courses with a links array per item is noise. Session 3 adds the links to GET /api/courses/\{id\} and enriches the Scalar metadata so the documentation page becomes a usable contract.

Session 2 Checkpoint

Before moving to Session 3, confirm all six rows pass: Check Pass means

GET items.length == 10, totalCount == 25, totalPages == 3 /api/courses?pa

ge=1&pageSize=1

0

GET Response pageSize == 50 \(capped\) /api/courses?pa

geSize=9999

GET One item, totalCount == 1 /api/courses?se

arch=fund

EF SQL log for Exactly one SELECT COUNT\(\*\) and one paged SELECT ... any paginated LIMIT … OFFSET … GET

Console log for Two lines: TMS API call: METHOD route and TMS API any API call response: STATUS GET Still returns the Session 1 CourseResponseDto the new /api/courses/1 collection action did not break the existing single-item action

\(single item\)

If any row fails, fix it before Session 3. HATEOAS links and Scalar metadata sit on top of a paginated, audited collection they do not paper over a broken one.



Module 6 Lab Session 3: Discoverability and Documentation

**Module** M6 Building RESTful Web APIs **Session** 3 of 3

**Exercises** 5 \(HATEOAS Links via LinkGenerator\), 6 \(Scalar Documentation

and Endpoint Metadata\) \+ final lab checkpoint

 

Prerequisites Check Before You Start

Your TMS API must already have:

 The full Session 1 contract on CoursesController and

EnrollmentsController \(Exercises 1–3\).

 The Session 2 paginated collection action with PagedResponse<T>, capped

PageSize, and the filter-then-count-then-page sequence inside CourseService.GetCoursesAsync \(Exercise 4\).

 The global AuditLogFilter registered through AddControllers\(options

=> options.Filters.Add<AuditLogFilter>\(\)\).

 The deterministic DataSeeder populating exactly 25 courses on

Development startup.

If any item is missing, finish Session 2 before continuing. HATEOAS links sit on top of a working detail endpoint; Scalar metadata sits on top of endpoints that already return the right shapes.

 

What This Sessions Is Really About

Session 1 made the contract correct. Session 2 made it scale. Today the contract becomes **self-describing**.

Two questions the Angular team in M8 and any external integration partner is going to ask the moment they open /scalar/v1:

1. *“Given a course, what can I do with it?”* the answer is HATEOAS links in the

detail response. Self, update, delete, list-enrolments, and conditionally a “create-enrolment” link the frontend uses to render or hide the Enrol button.

2. *“What does each endpoint accept and return?”* the answer is

\[ProducesResponseType\], \[EndpointSummary\], \[EndpointDescription\], and \[Tags\] decorating every action so Scalar can generate a real integration guide instead of a list of routes with no schemas.

Two short exercises then, and the rest of the lab time is for the **final checkpoint** **table** the eight-row verification at the end of this handout that proves every Session 1, 2, and 3 deliverable is green before the assessment.

 

Exercise 5: HATEOAS Links with LinkGenerator \(LO 6.5\) **Scenario:** The Angular team in M8 will hard-code URLs like /api/courses/$\{id\}/enrollments in TypeScript constants. The next time the backend team renames enrollments to enrolments \(or moves it under /api/registrations/\{id\}/courses\), every Angular component breaks. HATEOAS shifts that responsibility to the API: the response includes the URLs the client should call next, and the client follows links instead of constructing them. A second realism point: even *inside* the API, you do not want to build links by string interpolation. $"/api/courses/\{id\}" is a counter-example, not a teaching path duplicate route information that breaks silently the day a route changes. Real .NET teams use LinkGenerator \(or IUrlHelper\) so links are derived from the same routing metadata the controller is already using. Step 1 Define the link DTO

Create Dtos/LinkDto.cs:

**namespace** Tms.Api.Dtos;

**public** record LinkDto\(string Href, string Rel, string Method\);

Step 2 Define the detail DTO

Create Dtos/CourseDetailDto.cs. Note that CourseDetailDto is the **detail** shape it includes everything in CourseResponseDto plus the Links array. The list/page response keeps using CourseResponseDto \(no per-item links a 50-item list with links per item is mostly noise\):

**namespace** Tms.Api.Dtos;

**public** record CourseDetailDto

\{

**public** required int Id \{ **get**; init; \}

**public** required string Code \{ **get**; init; \}

**public** required string Title \{ **get**; init; \}

**public** required int MaxCapacity \{ **get**; init; \}

**public** required int EnrollmentCount \{ **get**; init; \}

**public** required IReadOnlyList<LinkDto> Links \{ **get**; init; \} \}

Step 3 Wire LinkGenerator into the controller LinkGenerator is a service the framework registers automatically you ask for it in the controller constructor. Update CoursesController: **using** Microsoft.AspNetCore.Mvc; **using** Microsoft.AspNetCore.Routing; **using** Tms.Api.Dtos;

**using** Tms.Api.Services;

**namespace** Tms.Api.Controllers;

\[ApiController\]

\[Route\("api/courses"\)\]

**public class** CoursesController\(

ICourseService courseService,

LinkGenerator linkGenerator\) : ControllerBase \{

*// ... existing actions ...*

\}

Step 4 Replace GetCourseById to return CourseDetailDto with links This is the **graded** part the order of operations and the choice of LinkGenerator.GetPathByName \(preferred when you have stable route names\) over hand-rolled strings is exactly what the integrity lab inspects: \[HttpGet\("\{id:int\}", Name = nameof\(GetCourseById\)\)\] **public** async Task<IActionResult> GetCourseById\(int id, CancellationToke n ct\)

\{

var course = await courseService.GetByIdAsync\(id, ct\);

**if** \(course **is null**\) **return** NotFound\(\);

*//* **TODO** *1: Use linkGenerator.GetPathByName\(HttpContext, routeName, values\) to build each href.*

*//* *The route names are the ones you set with \`Name = nameof*

*\(...\)\` on the actions:*

*//* *- nameof\(GetCourseById\)* *for /api/courses/\{i d\}*

*//* *- nameof\(GetEnrollment\)* *for /api/courses/\{c ourseId\}/enrollments/\{id\}*

*//* *For the list-enrolments link, you will need to build the*

*path manually with*

*//* *linkGenerator.GetPathByAction\(HttpContext, action: "GetE*

*nrollments",*

*//* *controller: "Enrollments",*

*values: new \{ courseId = id \}\)*

*//* *OR add Name = "ListCourseEnrollments" on the \[HttpGet\] o*

*f EnrollmentsController and use GetPathByName.*

*//* **TODO** *2: Build a List<LinkDto> with these entries:*

*//* *- \{ rel: "self",* *method: "GET",* *href: GetCours eById name with \{ id \} \}*

*//* *- \{ rel: "update",* *method: "PUT",* *href: GetCours eById name with \{ id \} \}*

*//* *- \{ rel: "delete",* *method: "DELETE", href: GetCours eById name with \{ id \} \}*

*//* *- \{ rel: "enrollments", method: "GET",* *href: <list-en rollments path> \}*

*//* *If course.EnrollmentCount < course.MaxCapacity, also add:*

*//* *- \{ rel: "enroll",* *method: "POST",* *href: <list-en rollments path> \}*

*//* *The conditional link is what makes HATEOAS earn its cost*

*the Angular*

*//* *team uses its presence/absence to render or hide the Enr*

*ol button without*

*//* *duplicating the capacity rule in TypeScript.*

*//* **TODO** *3: Build a CourseDetailDto from \`course\` plus the Links lis t and return Ok\(detailDto\).*

**throw new** NotImplementedException\(\); \}

A note on null-handling: GetPathByName returns string? the path is null if the route name does not exist or the values do not match the route template. In production code you assert non-null after building \(the route name is yours to control\). For lab clarity, treat a null path as a bug to fix, not as a runtime branch the Lab Verification table at the end of the handout flags it if the JSON contains "href": null.

Step 5 Verify

Step Action Expected Expected body / observation

status

1 200 OK GET /api/courses/1 \(any Body has the five fields plus a

seeded course not at links array of **5** entries \(self, Step Action Expected Expected body / observation

status

capacity\) update, delete, enrollments,

enroll\)

2 200 OK Use the enrollments href Returns the enrolment list for

from step 1 in a fresh GET that course \(you may need a

request GetEnrollments action on

EnrollmentsController first see Step 6\)

 

3 201 Fill the course to capacity Each enrolment succeeds Created \( POST \(each call\) /api/courses/\{id\}/enrollm

ents with valid studentId

until EnrollmentCount ==

MaxCapacity \)

4 GET /api/courses/\{id\} 200 OK links.length == 4 the

after step 3 fills capacity enroll link is **gone**

5 Inspect any href in the \(read\) Starts with /api/courses/...

response never null, never $\{id\} literal

Step 6 Add the list-enrolments action you discovered you needed Step 2 of verification reveals a gap: you have a route template for GET /api/courses/\{courseId\}/enrollments/\{id\} \(the single enrolment\) but not for GET /api/courses/\{courseId\}/enrollments \(the list\). Add it now to EnrollmentsController :

\[HttpGet\(Name = "ListCourseEnrollments"\)\] **public** async Task<IActionResult> GetEnrollments\(int courseId, Cancellat ionToken ct\)

\{

*//* **TODO** *4: Confirm the parent course exists \(courseService.GetByIdA sync\); 404 if not.*

*//* *Then return Ok\(await enrollmentService.GetByCourseAsync*

*\(courseId, ct\)\)*

*//* *where GetByCourseAsync projects to a List<EnrollmentResp*

*onseDto>.*

**throw new** NotImplementedException\(\); \}

You will need to add GetByCourseAsync to IEnrollmentService and EnrollmentService. The pattern matches Session 1’s GetByIdAsync shape AsNoTracking, Where, Select to DTO, ToListAsync\(ct\). With this in place, the enrollments href from your LinkGenerator call resolves to a real endpoint, and Step 5’s verification table passes end-to-end.

Troubleshooting

Symptom Likely cause Fix href is null in Route name does Confirm the action has Name = the JSON not exist or values nameof\(...\) and the values object

do not match the names match the route parameters template \(e.g. id\)

 

/api/courses/ linkGenerator.GetPathByName\(HttpCon interpolation instead text, nameof\(...\), new \{ id \}\) \{id\} href is You used string Replace with

\(literal of LinkGenerator

\{id\}\)

enroll link Conditional check Wrap the enroll link push in if appears even \(course.EnrollmentCount < missing

when course is course.MaxCapacity\) full

links is Links Default JSON ASP.NET Core uses camelCase by default in the JSON serialiser case is not verify no custom

camelCase JsonSerializerOptions overrides it

enrollments List-enrolments Add Step 6’s GetEnrollments action and href returns action does not exist the supporting service method 404 when yet

followed

 

Exercise 6: Scalar Documentation and Endpoint Metadata \(LO 6.6\)

**Scenario:** A potential integration partner opens https://localhost:<port>/scalar/v1 and finds your endpoints listed with no descriptions, no response types, and no grouping. They email you asking what POST /api/courses returns on validation failure. The right answer is not “I will write you a doc” it is “open Scalar again, the metadata is there now”. This exercise turns the live Scalar page into a real integration guide.

Step 1 Decorate CoursesController actions Open Controllers/CoursesController.cs. Add the metadata attributes to the three actions you already have. The \[Tags\("Courses"\)\] goes on the controller class so all course endpoints group together in Scalar: \[ApiController\]

\[Route\("api/courses"\)\]

\[Tags\("Courses"\)\]

\[Produces\("application/json"\)\]

\[ProducesResponseType\(**typeof**\(ProblemDetails\), StatusCodes.Status500Inte rnalServerError\)\]

**public class** CoursesController\(

ICourseService courseService,

LinkGenerator linkGenerator\) : ControllerBase \{

\[HttpGet\]

\[ProducesResponseType\(**typeof**\(PagedResponse<CourseResponseDto>\), Sta tusCodes.Status200OK\)\]

\[EndpointSummary\("List courses with pagination"\)\]

\[EndpointDescription\("Returns a paginated, optionally filtered list of TMS courses. PageSize is capped at 50."\)\]

**public** async Task<IActionResult> GetCourses\(

\[FromQuery\] PagedRequest request, CancellationToken ct\) \{ */\* un*

*changed \*/* \}

\[HttpGet\("\{id:int\}", Name = nameof\(GetCourseById\)\)\]

\[ProducesResponseType\(**typeof**\(CourseDetailDto\), StatusCodes.Status20 0OK\)\]

\[ProducesResponseType\(**typeof**\(ProblemDetails\), StatusCodes.Status404 NotFound\)\]

\[EndpointSummary\("Get a course by ID"\)\]

\[EndpointDescription\("Returns course details with HATEOAS links. Re turns 404 if the course does not exist."\)\]

**public** async Task<IActionResult> GetCourseById\(int id, Cancellation Token ct\) \{ */\* unchanged \*/* \}

\[HttpPost\]

\[ProducesResponseType\(**typeof**\(CourseResponseDto\), StatusCodes.Status 201Created\)\]

\[ProducesResponseType\(**typeof**\(ValidationProblemDetails\), StatusCodes. Status400BadRequest\)\]

\[ProducesResponseType\(**typeof**\(ProblemDetails\), StatusCodes.Status409 Conflict\)\]

\[EndpointSummary\("Create a new course"\)\]

\[EndpointDescription\("Creates a course with a unique code. Returns 409 if the course code already exists."\)\]

**public** async Task<IActionResult> CreateCourse\(

CreateCourseRequest request, CancellationToken ct\) \{ */\* unchang ed \*/* \}

\}

A few production habits visible here. \[Produces\("application/json"\)\] declares the response content type so Scalar shows the right Accept and Content-Type headers in its “Try It” panel. The class-level

\[ProducesResponseType\(typeof\(ProblemDetails\),

StatusCodes.Status500InternalServerError\)\] declares the catch-all for unhandled exceptions every action inherits it, every Scalar entry shows that 500 returns ProblemDetails. And \[Tags\("Courses"\)\] at the class level keeps the actions grouped applying \[Tags\] per-action would un-group them by accident. Step 2 Decorate EnrollmentsController actions \[ApiController\]

\[Route\("api/courses/\{courseId:int\}/enrollments"\)\] \[Tags\("Enrollments"\)\]

\[Produces\("application/json"\)\]

\[ProducesResponseType\(**typeof**\(ProblemDetails\), StatusCodes.Status500Inte rnalServerError\)\]

**public class** EnrollmentsController\(

ICourseService courseService,

IEnrollmentService enrollmentService\) : ControllerBase \{

\[HttpGet\(Name = "ListCourseEnrollments"\)\]

\[ProducesResponseType\(**typeof**\(IReadOnlyList<EnrollmentResponseDto>\), StatusCodes.Status200OK\)\]

\[ProducesResponseType\(**typeof**\(ProblemDetails\), StatusCodes.Status404 NotFound\)\]

\[EndpointSummary\("List enrolments for a course"\)\]

**public** async Task<IActionResult> GetEnrollments\(int courseId, Cance llationToken ct\) \{ */\* unchanged \*/* \}

\[HttpGet\("\{id:int\}", Name = nameof\(GetEnrollment\)\)\]

\[ProducesResponseType\(**typeof**\(EnrollmentResponseDto\), StatusCodes.St atus200OK\)\]

\[ProducesResponseType\(**typeof**\(ProblemDetails\), StatusCodes.Status404 NotFound\)\]

\[EndpointSummary\("Get one enrolment for a course"\)\]

**public** async Task<IActionResult> GetEnrollment\(int courseId, int id, CancellationToken ct\) \{ */\* unchanged \*/* \}

\[HttpPost\]

\[ProducesResponseType\(**typeof**\(EnrollmentResponseDto\), StatusCodes.St atus201Created\)\]

\[ProducesResponseType\(**typeof**\(ValidationProblemDetails\), StatusCodes. Status400BadRequest\)\]

\[ProducesResponseType\(**typeof**\(ProblemDetails\), StatusCodes.Status404 NotFound\)\]

\[ProducesResponseType\(**typeof**\(ProblemDetails\), StatusCodes.Status409 Conflict\)\]

\[EndpointSummary\("Enrol a student in a course"\)\]

\[EndpointDescription\("Returns 404 if the course does not exist, 409 if the course has reached MaxCapacity."\)\]

**public** async Task<IActionResult> EnrollStudent\(

int courseId, EnrollStudentRequest request, CancellationToken c

t\) \{ */\* unchanged \*/* \}

\}

Step 3 Verify

Open https://localhost:<port>/scalar/v1. The verification is visual five things should be true on the page:

1. The left-hand index shows two collapsible groups: **Courses** and

**Enrollments**.

2. Expand POST /api/courses. The summary reads “Create a new course”.

The “Responses” panel lists 201 Created with a CourseResponseDto schema, 400 Bad Request with a ValidationProblemDetails schema, and 409 Conflict with a ProblemDetails schema.

3. Expand GET /api/courses/\{id\}. The summary reads “Get a course by ID”.

The “Responses” panel lists 200 OK with CourseDetailDto \(schema includes the links array\), and 404 Not Found with ProblemDetails.

4. Click “Try It” on GET /api/courses/\{id\} with id = 1. Scalar should pre-fill

the URL, send the request, and show the JSON response inline. The links array is visible and follows the rules from Exercise 5.

5. The POST /api/courses/\{courseId\}/enrollments page lists all four

response codes \(201, 400, 404, 409\) that single endpoint is the most documented one in the API for a reason.

If your screenshot of Scalar matches those five points, the documentation is the integration guide. If anything is missing a status code, a schema, a tag you know which attribute is missing and on which action.

Troubleshooting

Symptom Likely cause Fix Scalar shows \[EndpointSummary\] Add it to each action; the using endpoints with not added Microsoft.AspNetCore.Http; no summary namespace must be imported Endpoints not \[Tags\] on actions Move \[Tags\("Courses"\)\] to the class

instead of the Symptom Likely cause Fix grouped flat list controller class level; remove any per-action \[Tags\] 400 schema Wrong type in the Use shows typeof\(ValidationProblemDetails\) attribute

ProblemDetail for the 400 status they have different s not shapes in the JSON ValidationPro

blemDetails

Try It panel Browser blocked the If you serve Scalar on the same origin, returns CORS cross-origin call this should not happen. M8 covers errors CORS for now, run the API and Scalar

from the same

https://localhost:<port>

 

409 \[ProducesResponseTy not listed Add it to the action pe\(typeof\(ProblemDe under POST tails\), /api/courses StatusCodes.Status4 in Scalar 09Conflict\)\] not

added

500 declared That is fine 500 is the Scalar shows it once on the controller but never catch-all block; individual actions do not need to appears in the repeat it response panel

 

Final Lab Checkpoint All Six Exercises

Run this table from top to bottom against your live API. Every row must pass. The integrity lab grading mirrors these exact checks; if they pass here, the assessment is recording the same evidence.

\# Check Pass means Lab 1 POST 201 Created, Location header points at S1 / E1

/api/courses with GetCourseById, response is valid body CourseResponseDto

 

2 POST 400 Bad Request, body is S1 / E2 /api/courses with ValidationProblemDetails with errors for \{\}

Code , Title, MaxCapacity

3 POST 409 Conflict, body is ProblemDetails S1 / E3

/api/courses with with Title = "Course code already a duplicate code exists"

\# Check Pass means Lab 4 POST 409 Conflict, body is ProblemDetails S1 / E3

/api/courses/\{id with Title = "Course is full" \}/enrollments

into a course at

capacity

 

5 GET 404 Not Found S1 / E1 /api/courses/999 9

 

6 GET PagedResponse<CourseResponseDto> with S2 / E4 /api/courses?pag totalCount == 25 , totalPages == 3 , e=2&pageSize=10 items.length ≤ 10 7 GET Response pageSize == 50 \(capped\) S2 / E4

/api/courses?pag

eSize=9999

8 EF SQL log on a One SELECT COUNT\(\*\) and one paged S2 / E4

paged GET SELECT … LIMIT … OFFSET … exactly two

statements

9 Console log on Two lines from AuditLogFilter: TMS API S2 / E4

any API call call: … and TMS API response: …

 

10 GET CourseDetailDto with links.length == 5 S3 / E5 /api/courses/\{id \( self , update , delete , enrollments , \} for a course not enroll \) at capacity

 

11 GET CourseDetailDto with links.length == 4 S3 / E5 /api/courses/\{id \(no enroll link\) \} for a course at

capacity

12 Any link href in Starts with /api/courses/...; never null; S3 / E5

the response never $\{id\}

13 /scalar/v1 in the Two grouped sections \(Courses, S3 / E6

browser Enrollments\); every endpoint has summary

\+ response schemas \+ status codes

14 POST Lists 201 Created \(CourseResponseDto\), S3 / E6

/api/courses row 400 Bad Request in Scalar \(ValidationProblemDetails\), 409

Conflict \(ProblemDetails\)

If all fourteen rows pass, you have built a production-quality REST API contract: predictable routes, validated DTOs, business-rule status codes, mandatory bounded collections, hyperlinked discovery, and a documentation page that earns its place. The Facilitator Answer Key contains reference solutions for cross-checking. Move on to the M6 Assessment Pack.

 

Why this session ends here and what M7 brings You finish M6 with the **interface** of the TMS API solid: every endpoint has a name, a contract, a documented set of response shapes, and a self-describing JSON body that tells the client what to do next. The next four problems **versioning** \(when the contract has to change without breaking existing clients\), **caching** \(when a GET does not have to hit the database every time\), **rate limiting** \(when a hostile or buggy client hammers the same endpoint\), and **CQRS** \(when read paths and write paths have genuinely different shapes\) all assume this contract is in place. M7 is about hardening it; the contract itself is done here.



Module 7 Lab: Session 0: The M6→M7 Handoff \(Split into Clean Architecture\)

**Module** M7: Advanced Web API Development **Session** 0 of 4 \(pre-session, not graded\) **What you do** Split the single M6 TmsApi project into the four-project clean-

architecture shape the rest of M7 assumes

 

Why this exists

In M6 you shipped a working TMS API. It is one ASP.NET project: TmsApi.csproj: with Entities/, Data/, Services/, DTOs/, and Controllers/ folders all in the same assembly. The Angular team in M8 and every integration partner downstream depend on that API being *correct* and *production-shaped*. “Production-shaped” in M7 means **clean architecture**: the rule the rest of the labs assume is that **dependencies point inward** \(the outer Api layer can see the inner layers; the inner layers cannot see the outer ones\). M7’s Session 1 exercise 2 opens with cd TmsApi.Api && dotnet run and then immediately writes TmsApi.Application/Common/Result.cs, TmsApi.Infrastructure/Caching/..., TmsApi.Api/Controllers/V1/.... None of those paths exist in the M6 monolith. Session 0 is that turns the M6 monolith into the M7 four-project solution.

If you skip this and try to follow Session 1, the first dotnet add

TmsApi.Api package … command will fail because TmsApi.Api does not

exist. If you try to “wing it” by hand, you will spend the entire Session 1

debugging namespace mismatches instead of learning versioning and

CQRS. Two hours now saves a full day later.

**Windows Users: Shell Recommendation:** We recommend using **Git**

**Bash** \(installed by default with Git\) for all terminal commands in this

guide. This ensures that Unix utility commands \(mv, mkdir, grep, rmdir\)

work identically on your machine without requiring conversion to

PowerShell equivalents.

Words you need before you start

You will see these terms in every step. Read this once and refer back when something doesn’t ring a bell.

 **.csproj** : an XML file at the root of every C\# project. It declares the SDK

\(what kind of project it is\), the target framework, and the package/project references. You will be opening and editing .csproj files in Step 1.

 **SDK**: the “kit” that knows how to build a project. A classlib SDK builds a

class library \(no entry point, no web\). The web SDK \(full name Microsoft.NET.Sdk.Web \) builds an ASP.NET application.

 **Project reference** vs **package reference**: both are listed inside a .csproj,

but they are different. A *project reference* points to another .csproj in the same workspace \(<ProjectReference Include="../TmsApi.Domain/TmsApi.Domain.csproj" />\): it lets one project use another project’s classes. A *package reference* points to NuGet packages. We use both.

 **Startup project**: the project dotnet run \(and dotnet ef\) actually launches.

It is the project that has Program.cs and the web SDK. In the M7 shape, the startup project is **always** TmsApi.Api.

 **dotnet new classlib -n X -f net10.0**: create a new class library

project named X targeting .NET 10.0.

 **dotnet add Y reference Z**: add a *project reference* from project Y to

project Z.

 **dotnet sln add Y**: register project Y inside the solution file \(TmsApi.sln\).

 

Before You Begin: confirm the M6 baseline You should already have completed M6 with a working TMS API exposing GET /api/courses, GET /api/courses/\{id\}, POST /api/courses/\{courseId\}/enrollments, DataSeeder running on dev startup, and Scalar at the path your M6 project mapped \(typically https://localhost:5001/scalar/v1, or /scalar\). From the project root:

dotnet build

dotnet run--project TmsApi.csproj You should see the API start on https://localhost:5001 \(or whatever HTTPS URL is in your launchSettings.json\). **Stop here and fix any M6 build errors** **before continuing.** Session 0 stacks on a working M6 baseline: a broken build wastes the whole refactor.

Commit the baseline before continuing:

git add-A

git commit-m "M6: green baseline before M7 clean-architecture split"

 

The shape we are splitting into

Read this once before you start moving files. The Onion / Clean Architecture rule is **dependencies point inward**. The outer layer \(Api\) knows about every inner layer. The inner layers know nothing about the outer ones. The compiler enforces the rule for free once the projects are wired correctly: TmsApi.Domain.csproj will literally refuse to reference TmsApi.Infrastructure, because the project reference does not exist.

Project SDK References Holds TmsApi.Domain classlib \(none\) Pure C\# entities, value

objects, domain events.

No NuGet. No EF. No

ASP.NET.

TmsApi.Applic TmsApi.Domain classlib DTOs, service interfaces, ation MediatR

commands/queries/handle

rs, Result<T,E>, pipeline behaviors.

TmsApi.Infras classlib TmsApi.Application, EF Core TmsDbContext \+ tructure TmsApi.Domain IEntityTypeConfiguratio

n<>s, repository implementations, external

HTTP clients, HybridCache adapter.

TmsApi.Api web TmsApi.Application, Controllers, middleware,

TmsApi.Infrastructure Program.cs ,

appsettings.json, Program.cs l evel DI registration.

The M6 monolith’s TmsApi.csproj keeps the web project \(TmsApi.Api\) but **must** be renamed so the new TmsApi.Api.csproj can take over. Step 1 renames the existing .csproj to TmsApi.Api.csproj and switches it to the web SDK.

**Why four projects, not three?** You will see some M7 handouts put the

domain entities in TmsApi.Application and collapse to three projects.

Either shape is valid; the four-project version is the M7 standard because

\(a\) the Domain layer never needs to take a MediatR or FluentValidation

dependency, \(b\) when you eventually split the worker out into its own

process for Exercise 5, the worker can reference TmsApi.Application

without dragging in the entire web project, and \(c\) the four-project

shape mirrors Section 8 of the M7 essentials verbatim, which makes the

M7 capstone grading consistent with the diagram.

 

Step 1: Move the M6 project file to TmsApi.Api and create the three new projects

You will perform this step from the **project root directory** \(where your existing TmsApi.csproj and TmsApi.sln reside\). 1. Create the TmsApi.Api directory and move/rename the project file Create the directory for your API project, rename the existing monolith’s .csproj file, and move it inside the new folder. Run these commands in your Git Bash terminal:

mkdir TmsApi.Api

mv TmsApi.csproj TmsApi.Api/TmsApi.Api.csproj mv Program.cs TmsApi.Api/Program.cs

 

2. Create the three new class libraries

Check if your project have TmsApi.sln file. If there is no one, add it by running dotnet new sln

Run the following commands to create the other Clean Architecture layers: dotnet new classlib-n TmsApi.Domain -f net10.0 dotnet new classlib-n TmsApi.Application -f net10.0 dotnet new classlib-n TmsApi.Infrastructure-f net10.0 3. Register all four projects in your solution file Add the newly created class libraries and the renamed API project to the solution: dotnet sln TmsApi.slnx add TmsApi.Domain/TmsApi.Domain.csproj dotnet sln TmsApi.slnx add TmsApi.Application/TmsApi.Application.csproj dotnet sln TmsApi.slnx add TmsApi.Infrastructure/TmsApi.Infrastructure. csproj

dotnet sln TmsApi.slnx add TmsApi.Api/TmsApi.Api.csproj

4. Wire the project references \(dependencies point inward\) Enforce Clean Architecture boundaries by adding project-to-project references: *\# Application references Domain \(innermost\)*

dotnet add TmsApi.Application/TmsApi.Application.csproj reference TmsAp i.Domain/TmsApi.Domain.csproj

*\# Infrastructure references Application and Domain* dotnet add TmsApi.Infrastructure/TmsApi.Infrastructure.csproj reference

TmsApi.Application/TmsApi.Application.csproj

dotnet add TmsApi.Infrastructure/TmsApi.Infrastructure.csproj reference

TmsApi.Domain/TmsApi.Domain.csproj

*\# Api references Application and Infrastructure* dotnet add TmsApi.Api/TmsApi.Api.csproj reference TmsApi.Application/Tm sApi.Application.csproj

dotnet add TmsApi.Api/TmsApi.Api.csproj reference TmsApi.Infrastructure /TmsApi.Infrastructure.csproj

**Audit checkpoint: do this now.** Run dotnet build from the root. The build will fail; that is expected: Steps 2–5 move the code. Confirm the error is only about unresolved using TmsApi.Entities; and using TmsApi.Data; namespaces, not about missing NuGet packages.

Step 2: Move the entities to TmsApi.Domain The M6 monolith’s Entities/ folder is the cleanest move. Pure C\# classes, no EF attributes, no NuGet. They belong in the innermost layer. 1. Create the entities folder and move the files Run the following commands in Git Bash from the project root: mkdir-p TmsApi.Domain/Entities mv Entities/\*.cs TmsApi.Domain/Entities/

2. Update the namespaces in the moved files Open TmsApi.Domain/Entities/Certificate.cs in your editor. You will see the namespace declaration near the top of the file. Change the namespace declaration to the Clean Architecture format. Follow the editor as the image below



 

**Audit checkpoint.** dotnet build from the root should now fail with error CS5001: Program does not contain a static ‘Main’ method suitable for an entry point.



Step 3: Move TmsDbContext and EF configurations to TmsApi.Infrastructure

TmsDbContext and the IEntityTypeConfiguration<> classes are the EF-specific glue. They depend on Microsoft.EntityFrameworkCore and Npgsql.EntityFrameworkCore.PostgreSQL, both of which the Domain layer must not take on. The Infrastructure layer is their home.

1. Create the Infrastructure folder structure and move EF configurations

Run the following commands in Git Bash from the project root: mkdir-p TmsApi.Infrastructure/Persistence/Configurations mv Data/TmsDbContext.cs TmsApi.Infrastructure/Persistence/ mv Data/Configurations/\* TmsApi.Infrastructure/Persistence/Configurati ons/ 2>/dev/null **||** true

mkdir-p TmsApi.Infrastructure/Services mv Services/\* TmsApi.Infrastructure/Services/ mkdir-p TmsApi.Infrastructure/Persistence/Migrations/ mv Migrations/\* TmsApi.Infrastructure/Persistence/Migrations/ rmdir Data/Configurations 2>/dev/null **||** true rmdir Data 2>/dev/null **||** true

2. Update namespaces in the moved files

Update the namespace declarations inside the moved files:

 Change namespace TmsApi.Data; to namespace

TmsApi.Infrastructure.Persistence;

 Change using TmsApi.Entities; to using TmsApi.Domain.Entities;

\(Infrastructure is allowed to reference Domain\)

3. Add Entity Framework Core packages to Infrastructure Since the data model now lives in TmsApi.Infrastructure, you need to add the required EF Core NuGet packages to this project:

dotnet add TmsApi.Infrastructure/TmsApi.Infrastructure.csproj package M icrosoft.EntityFrameworkCore

dotnet add TmsApi.Infrastructure/TmsApi.Infrastructure.csproj package M icrosoft.EntityFrameworkCore.Design

dotnet add TmsApi.Infrastructure/TmsApi.Infrastructure.csproj package M icrosoft.EntityFrameworkCore.Relational

dotnet add TmsApi.Infrastructure/TmsApi.Infrastructure.csproj package N pgsql.EntityFrameworkCore.PostgreSQL

*Now remove the following packages from Tms.Api.Csproj*

-<PackageReference Include="Microsoft.EntityFrameworkCore" Version="1

0.0.7" />

-<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Ve

rsion="10.0.1" />

4. Ensure Design package is on the Startup project The Microsoft.EntityFrameworkCore.Design package is required on **both** TmsApi.Infrastructure \(where the model lives\) **and** TmsApi.Api \(the startup project\).

The dotnet ef CLI builds a design-time service provider by running the **startup** **project** \(it does not walk project references to find Design at run time\). Add the design package to TmsApi.Api:

dotnet add TmsApi.Api/TmsApi.Api.csproj package Microsoft.EntityFramewo rkCore.Design

5. Update Program.cs DbContext registration Update the using statement and context registration inside TmsApi.Api/Program.cs :

**using** TmsApi.Infrastructure.Persistence; builder.Services.AddDbContext<TmsDbContext>\(options =>

options.UseNpgsql\(builder.Configuration.GetConnectionString\("TmsDat abase"\)\)\);

6. Run migrations to verify database schema parity Run these commands from the project root to verify your model is recognized properly:

dotnet ef migrations add PostSplitBaseline \\

--project TmsApi.Infrastructure/TmsApi.Infrastructure.csproj \\

--startup-project TmsApi.Api/TmsApi.Api.csproj

The generated migration file should be **empty** because the model shape did not change. Apply the migration to verify:

dotnet ef database update \\

--project TmsApi.Infrastructure/TmsApi.Infrastructure.csproj \\

--startup-project TmsApi.Api/TmsApi.Api.csproj

**Audit checkpoint.** The M6 database schema must be byte-identical after the move. Open pgAdmin \(or psql\) and confirm tables are present and unchanged. Step 4: Move services, DTOs, and validators to TmsApi.Application

Services/ and DTOs/ from the M6 monolith are application-layer concerns. 1. Create the Application folder structure and move DTOs and Interfaces

Run the following commands in Git Bash from the project root: mkdir-p TmsApi.Application/DTOs TmsApi.Application/Interfaces TmsApi.A pplication/Services

mv DTOs/\* TmsApi.Application/DTOs/ mv Services/I\*.cs TmsApi.Application/Interfaces/ 2>/dev/null **||** true

2. Determine where to place concrete service implementations For concrete service classes, decide where they belong based on their dependencies:

 **Move to Application** \(TmsApi.Application/Services/\) if they only

depend on interfaces and contain purely orchestration/business logic.

 **Move to Infrastructure** \(TmsApi.Infrastructure/Persistence/\) if they

directly depend on TmsDbContext or external infrastructure/APIs.

3. Add FluentValidation package to Application Since validators \(which use AbstractValidator<> from FluentValidation\) are moved to the Application project, you need to add the required package: dotnet add TmsApi.Application/TmsApi.Application.csproj package FluentV alidation

\[\!IMPORTANT\] **Namespace Import Warning:** Because

FluentValidation is a third-party library, its namespace is not part

of .NET’s default implicit usings. Once the validators are moved to

TmsApi.Application, you will likely get a compile error: error CS0246:

The type or namespace name 'AbstractValidator<>' could not be

found

To resolve this, open your validator files \(e.g. CreateCourseValidator.cs\)

and ensure you add the using directive at the top:

**using** FluentValidation;

4. Update namespaces in the moved files like we did before

 TmsApi.DTOs → TmsApi.Application.DTOs

 TmsApi.Services \(for interfaces\) → TmsApi.Application.Interfaces

 TmsApi.Services \(for implementations in Application\) →

TmsApi.Application.Services

 TmsApi.Services \(for implementations in Infrastructure\) →

TmsApi.Infrastructure.Persistence

 TmsApi.Entities → TmsApi.Domain.Entities

Ensure that no old namespaces remain in your projects. **Option A: VS Code Search \(OS-Agnostic / Editor-first\)** Press Ctrl \+ Shift \+ F \(or Cmd \+ Shift \+ F on macOS\), toggle **Regular Expression mode** \(the .\* icon in the search bar\), and search for:

TmsApi\\.\(Services|DTOs|Data|Entities\)

**Option B: Terminal Search \(Git Bash / macOS / Linux\)** Run grep from the project root:

grep-rn "TmsApi.Services\\|TmsApi.DTOs\\|TmsApi.Data\\|TmsApi.Entities" T msApi.Api/

Every hit must be rewritten to the new namespaces. None should remain. Step 5: Move controllers and Program.cs into TmsApi.Api The controllers, middleware, configurations, and startup settings belong in TmsApi.Api.

1. Create the Controllers folder and move files Run the following commands in Git Bash from the project root: mkdir-p TmsApi.Api/Controllers mv Controllers/\* TmsApi.Api/Controllers/ mkdir-p TmsApi.Api/Middlewares mv Middlewares/\* TmsApi.Api/Middlewares/ mkdir-p TmsApi.Api/Options

mv Options/\* TmsApi.Api/Options/ mkdir-p TmsApi.Api/Properties

mv Properties/\* TmsApi.Api/Properties/ mv Program.cs TmsApi.Api/ 2>/dev/null **||** true mv appsettings.json TmsApi.Api/ 2>/dev/null **||** true mv appsettings.Development.json TmsApi.Api/ 2>/dev/null **||** true *Note: Delete the empty directories* 2. Update namespaces in controllers and Program.cs Update controller namespaces:

 TmsApi.Controllers → TmsApi.Api.Controllers

 TmsApi.DTOs → TmsApi.Application.DTOs

 TmsApi.Services → TmsApi.Application.Interfaces \(or

TmsApi.Application.Services / TmsApi.Infrastructure.Persistence depending on where implementations landed\)

 TmsApi.Entities → TmsApi.Domain.Entities

 TmsApi.Data → TmsApi.Infrastructure.Persistence

Update Program.cs to resolve dependency references using the correct Clean Architecture layers.

**Audit checkpoint.** dotnet build from the root should now succeed with **zero** **errors and zero warnings**.

Step 6: Run the M6 verification suite

Run these commands from the project root:

dotnet build

dotnet run--project TmsApi.Api/TmsApi.Api.csproj Verify the course lists, course detail, and enrollment functionality using Scalar at the local API path.

Step 7: Commit the refactor and move to Session 1 Commit the clean architecture structure to Git:

git add-A

git commit-m "M7 Session 0: split M6 monolith into clean architecture \(4 projects\)"

What you should now have

After this session, your workspace at the root should look like: TmsApi.sln

TmsApi.Api/ ← web SDK, controllers, Program.cs, appset

tings

Controllers/

Program.cs

appsettings.json

appsettings.Development.json

TmsApi.Application/ ← classlib, DTOs \+ interfaces \+ MediatR ha

ndlers

Common/

DTOs/

Interfaces/

Services/

TmsApi.Infrastructure/ ← classlib, EF Core \+ repository implement

ations

Persistence/

TmsDbContext.cs

Configurations/

TmsApi.Domain/ ← classlib, pure entities, no NuGet

Entities/

No file from the M6 monolith should be sitting at the root directory’s Entities/, Data/, Services/, or DTOs/ anymore. If any old empty folder remains, remove it: rmdir Entities Data Services DTOs Controllers 2>/dev/null **||** true

Session 0 Checkpoint

Tick all of these before you move to **Guided Lab Session 1**:

☐ TmsApi.sln exists at the root and references all four projects.

☐ TmsApi.Domain/Entities/ contains all five TMS entities with the

TmsApi.Domain.Entities namespace.

☐ TmsApi.Application/ holds DTOs, service interfaces, and any in-

Application service implementations.

☐ TmsApi.Infrastructure/Persistence/TmsDbContext.cs compiles and is

reachable from Program.cs.

☐ Microsoft.EntityFrameworkCore.Design is referenced by **both**

TmsApi.Infrastructure and TmsApi.Api \(the startup project\).

☐ TmsApi.Api/Controllers/ holds the M6 controllers with the

TmsApi.Api.Controllers namespace.

☐ TmsApi.Api/Program.cs is the only Program.cs in the workspace.

If any box is unchecked, fix it before opening **Guided Lab Session 1**.

Module 7 Lab: Session 1: Contracts That Survive Change

**Module** M7: Advanced Web API Development **Exercises** 1 \(API Versioning \+ Deprecation\), 2 \(CQRS, MediatR Pipelines,

Result<T> , IExceptionHandler\)

**What you** A TMS API that serves V1 and V2 simultaneously, signals V1’s **build** sunset in HTTP headers, and routes every business decision

through MediatR with typed Result<T,E> and a global RFC 7807 ProblemDetails translator

 

Welcome to the Hardening Sprint

In M6 you built the API contract, clean resource URLs, DTOs, status codes, pagination, Scalar docs. Hand that to the Angular team in M8 and they can integrate.

Then production happens.

**Wednesday, 09:14.** A teammate pushes a change to the V1 course endpoint that renames a JSON field. Fifty tablets in a rural training centre running last quarter’s mobile app stop working at 09:00. The dashboard team’s deploy is rolled back. The rural centre is told to wait. Two days of trust evaporate. **Friday, 16:30.** The team opens EnrollmentsController.cs to add a small audit-log line. They scroll past 400 lines of validation, conditional logic, email triggers, and database calls before finding the controller action. The “small” change ships with a typo because no one can hold the whole file in their head. Both stories have the same root cause: the team wrote a CRUD API and called it done. **Session 1 is where you stop doing that.** By the end of today your TMS API can evolve without breaking existing clients \(Exercise 1\) and is structured so a single line of business logic can be added without re-reading the controller \(Exercise 2\).

**Stop and tell the person next to you, in one sentence each:** what

would have prevented Wednesday’s incident, and what would have

prevented Friday’s incident? Hold those two sentences in your head.

when you finish today, you will have shipped both fixes.

Before You Begin: Session 1 sync

You should already have completed M6 with a working TMS API exposing **GET** **/api/courses** \(list and detail as you built in M6\), **POST /api/courses/\{courseId\}/enrollments** \(nested enrollment from M6\), and Scalar at the explorer path your TMS project mapped \(typically **https://localhost:5001/scalar/v1**, or **/scalar** if that is what resolves in the browser\). From the workspace root:

cd TmsApi

dotnet build

dotnet run

Confirm the API starts on https://localhost:5001 \(or the HTTPS URL shown in your \[launchSettings.json\]\). **Stop here and fix any M6 build errors before** **continuing.** M7 piles new conventions on top. Stacking on a broken foundation wastes the whole session.

Install the Session 1 Exercise 1 packages:

dotnet add TmsApi package Asp.Versioning.Mvc

dotnet add TmsApi package Asp.Versioning.Mvc.ApiExplorer Run dotnet build again after the installs. If it fails with Asp.Versioning.Http not found, you ran dotnet add against the wrong project, the versioning packages belong to the API project, the validation packages to the Application project. Fix it before moving on.

 

Exercise 1: API Versioning and Deprecation Strategy

**Why this exists:** Without versioning, every change is a breaking change. With

URL-segment versioning *and* Sunset/Deprecation headers *and* a one-page policy, every change is either non-breaking \(additive\) or scheduled \(sunset\). That difference is the boundary between “we ship features” and “we keep getting hauled into incident calls.”

Step 1 Configure versioning in Program.cs

Open TmsApi/Program.cs. Find the line that says var builder = WebApplication.CreateBuilder\(args\);. Below your existing service registrations \(after builder.Services.AddControllers\(\) if you have it; before var app = builder.Build\(\);\), add the versioning services: **using** Asp.Versioning; *// add to the top of the file with the*

*other usings*

 

builder.Services.AddOpenApi\("v1", options => \{

options.ShouldInclude = description =>

description.GroupName == "v1";

\}\);

builder.Services.AddOpenApi\("v2", options => \{

options.ShouldInclude = description =>

description.GroupName == "v2";

\}\);

builder.Services.AddApiVersioning\(options => \{

options.DefaultApiVersion = **new** ApiVersion\(1, 0\);

options.AssumeDefaultVersionWhenUnspecified = **true**;

options.ReportApiVersions = **true**;

options.ApiVersionReader = **new** UrlSegmentApiVersionReader\(\); \}\)

.AddApiExplorer\(options =>

\{

options.GroupNameFormat = "'v'VVV";

options.SubstituteApiVersionInUrl = **true**; \}\);

 

// update your scalar config

app.MapScalarApiReference\(options =>

\{

options.WithTitle\("TMS API Reference"\)

.WithTheme\(ScalarTheme.DeepSpace\) .WithDefaultHttpClient\(ScalarTarget.CSharp,

ScalarClient.HttpClient\);

// Tell Scalar to pull both documents into its sidebar dropdown options

.AddDocument\("v1", "API Version 1.0"\) .AddDocument\("v2", "API Version 2.0"\);

\}\);

 

**What each option does, in plain language.**

 DefaultApiVersion: what version a client gets if it does not ask for one.

 AssumeDefaultVersionWhenUnspecified: controls whether unversioned

URLs \(/api/courses\) are accepted. Leave on while migrating; turn off once everyone is versioned.

 ReportApiVersions: adds api-supported-versions: 1.0, 2.0 to every

response. The browser tab and the curl output both surface what versions exist; partner teams find this far more useful than your wiki.

 UrlSegmentApiVersionReader: tells the framework versions live in the

URL \(/api/v1/...\).

 GroupNameFormat = "'v'VVV" and SubstituteApiVersionInUrl = true

make Scalar render v1 and v2 as separate documents.

Run dotnet build. It must compile. If it does not, the most common error is missing using Asp.Versioning; at the top. Step 2 Create the V1 controller

V1 is the version your existing clients depend on. Treat it as a frozen contract: no field renames, no new required fields, no removed fields. Create the file TmsApi/Controllers/V1/CoursesController.cs: **using** Asp.Versioning;

**using** Microsoft.AspNetCore.Mvc; **using** Microsoft.EntityFrameworkCore; **namespace** TmsApi.Controllers.V1; \[ApiController\]

\[Route\("api/v\{version:apiVersion\}/courses"\)\] \[ApiVersion\("1.0"\)\]

**public class** CoursesController\(TmsDbContext context\) : ControllerBase \{

\[HttpGet\]

**public** async Task<IActionResult> GetCourses\(

\[FromQuery\] int page = 1, \[FromQuery\] int pageSize = 20, CancellationToken ct = **default**\)

\{

page = Math.Max\(1, page\); pageSize = Math.Clamp\(pageSize, 1, 50\); var baseQuery = context.Courses.AsNoTracking\(\);

var totalCount = await baseQuery.CountAsync\(ct\);

var items = await baseQuery

.OrderBy\(c => c.Title\) .Skip\(\(page-1\) \* pageSize\) .Take\(pageSize\)

.Select\(c => **new**

\{

c.Id,

c.Code,

c.Title,

c.MaxCapacity,

EnrollmentCount = c.Enrollments.Count

\}\)

.ToListAsync\(ct\);

var totalPages = \(int\)Math.Ceiling\(totalCount / \(double\)pageSiz

e\);

**return** Ok\(**new**

\{

items,

totalCount,

page,

pageSize,

totalPages,

hasNext = page < totalPages, hasPrevious = page > 1

\}\);

\}

\}

Two details that are not obvious. Put **V1** and **V2** course controllers in **different .NET namespaces** so they are two distinct controller types; **\[ApiVersion\("1.0"\)\]** vs **\[ApiVersion\("2.0"\)\]** plus the **v\{version:apiVersion\}** segment are what route a request to the right type. The \[Route\("api/v\{version:apiVersion\}/courses"\)\] template uses the apiVersion route constraint registered by AddApiVersioning; do not invent your own template like a hard-coded \[Route\("api/v1/courses"\)\] on the class only, then every new version needs a duplicate controller class for the same resource. The list action preserves the **M6 TMS API contract** for catalogues: **items** plus paging metadata at the JSON root, and each row carries **id**, **code**, **title**, **maxCapacity**, **enrollmentCount** so tablets and Angular list screens stay on-spine. Step 3 Create the V2 controller

V2 is where the dashboard team’s enrichment lives: the **data** **/** **meta** **/** **links** envelope from the Module 7 spine \(rows in **data**, paging fields in **meta**, discoverability hints in **links**\). Create TmsApi/Controllers/V2/CoursesController.cs: **using** Asp.Versioning;

**using** Microsoft.AspNetCore.Mvc; **using** Microsoft.EntityFrameworkCore; **namespace** TmsApi.Controllers.V2; \[ApiController\]

\[Route\("api/v\{version:apiVersion\}/courses"\)\] \[ApiVersion\("2.0"\)\]

**public class** CoursesController\(TmsDbContext context\) : ControllerBase \{

\[HttpGet\]

**public** async Task<IActionResult> GetCourses\(

\[FromQuery\] int page = 1, \[FromQuery\] int pageSize = 20, CancellationToken ct = **default**\)

\{

page = Math.Max\(1, page\); pageSize = Math.Clamp\(pageSize, 1, 50\); var baseQuery = context.Courses.AsNoTracking\(\); var totalCount = await baseQuery.CountAsync\(ct\);

var rows = await baseQuery

.OrderBy\(c => c.Title\) .Skip\(\(page-1\) \* pageSize\) .Take\(pageSize\)

.Select\(c => **new**

\{

c.Id,

c.Title,

c.Code,

c.MaxCapacity,

EnrollmentCount = c.Enrollments.Count

\}\)

.ToListAsync\(ct\);

var totalPages = \(int\)Math.Ceiling\(totalCount / \(double\)pageSiz

e\);

var hasNext = page < totalPages; var hasPrevious = page > 1; **return** Ok\(**new**

\{

data = rows,

meta = **new**

\{

totalCount,

page,

pageSize,

totalPages,

hasNext,

hasPrevious

\},

links = **new**

\{

self = $"/api/v2/courses?page=\{page\}&pageSize=\{pageSize\}

",

next = hasNext

? $"/api/v2/courses?page=\{page \+ 1\}&pageSize=\{pageS

ize\}"

: \(string?\)**null**,

prev = hasPrevious

? $"/api/v2/courses?page=\{page - 1\}&pageSize=\{pageS

ize\}"

: \(string?\)**null**,

enroll = "/api/v2/enrollments"

\}

\}\);

\}

\}

**Run the API and prove both versions answer.** In the terminal: dotnet run--project TmsApi.Api In a second terminal \(leave the first showing logs\):

curl-i https://localhost:5001/api/v1/courses curl-i https://localhost:5001/api/v2/courses You must see:

 /api/v1/courses returns **paged** JSON \(root object with **items** \+

**totalCount** / **page**, same **TMS API contract** family as the programme pivot\), **not** a bare \[...\] array at the root.

 /api/v2/courses returns the **envelope** from the Module 7 spine: **data** \(the

rows for that page\), **meta**, and **links** \(**self**, **next**, …\).

 Both responses include the header api-supported-versions: 1.0, 2.0.

If you get AmbiguousMatchException at startup, both controllers are not in distinct namespaces, fix the namespaces and re-run. If V1 returns 404, the v\{version:apiVersion\} route template is wrong \(or SubstituteApiVersionInUrl = true is missing\).

Step 4 Mark V1 as deprecated *with the right HTTP headers* URL-segment versioning is the *mechanism*. The *discipline* is telling V1 clients the version is going away, and where to go next. Real APIs \(Stripe, GitHub, Twilio\) do this through three response headers on every V1 response:

 Deprecation: true V1 is officially deprecated \(IETF draft\).

 Sunset: <RFC 7231 date> the date V1 will stop responding \(RFC 8594\).

 Link: </api/v2/courses>; rel="successor-version" where the client

should migrate \(RFC 5988\).

Add a small middleware that stamps V1 responses with these headers. Create TmsApi.Api/Middleware/V1DeprecationMiddleware.cs : **namespace** TmsApi.Middleware;

**public class** V1DeprecationMiddleware\(RequestDelegate next\) \{

**private static readonly** DateTimeOffset SunsetDate =

**new**\(2026, 12, 31, 0, 0, 0, TimeSpan.Zero\);

**public** async Task InvokeAsync\(HttpContext context\)

\{

context.Response.OnStarting\(\(\) => \{

**if** \(context.Request.Path.StartsWithSegments\("/api/v1"\)\) \{

context.Response.Headers\["Deprecation"\] = "true"; context.Response.Headers\["Sunset"\] = SunsetDate.ToStrin

g\("R"\);

context.Response.Headers\["Link"\] =

$"<\{context.Request.Scheme\}://\{context.Request.Host\}

/api/v2\{context.Request.Path.Value?\[7..\]\}>; rel=\\"successor-version\\"";

\}

**return** Task.CompletedTask;

\}\);

await next\(context\);

\}

\}

Wire it in Program.cs **before** app.MapControllers\(\): app.UseMiddleware<V1DeprecationMiddleware>\(\);

**Why a middleware and not an attribute?** The middleware applies to

every V1 endpoint without anyone remembering to decorate it. New V1

routes inherit the deprecation policy automatically. Forgetting one route

is exactly the kind of silent drift that breaks production migrations.

**Why** **Response.OnStarting** **and not just setting headers directly?**

Inside InvokeAsync\(...\) you do not yet know what status code or path

the controller will return. OnStarting is invoked once headers are

flushed but before the body, the latest possible point that headers can

still be added. Setting headers earlier is harmless here but bites you later

when conditional headers depend on response state \(you will see this

pattern again with rate limiting in Session 2\).

**Prove the headers land.** Restart the API. Then: curl-i https://localhost:5001/api/v1/courses Read the response headers carefully. You must see:

HTTP/1.1 200 OK

Content-Type: application/json

Deprecation: true

Sunset: Thu, 31 Dec 2026 00:00:00 GMT

Link: <https://localhost:5001/api/v2/courses>; rel="successor-version" api-supported-versions: 1.0, 2.0

Then:

curl-i https://localhost:5001/api/v2/courses The V2 response must **not** include Deprecation, Sunset, or Link headers. If V2 has them, the middleware path check is matching /api/v2 too. fix the StartsWithSegments\("/api/v1"\) line.

**Stop and observe.** Look at your terminal three lines of HTTP machinery

just communicated, in the protocol’s own words, that V1 is on a sunset

clock and V2 is the destination. No email. No support page. No phone

call. Every well-behaved client now knows the future of the API the

moment it makes a request. **This is the senior-grade detail**

**interviewers probe for** when they ask “how do you migrate clients off

old API versions?”

Step 5 Write a one-page versioning policy

Create docs/api-versioning-policy.md in your repo. Real engineering teams have one, interviewers ask to see it. Write it in your own words, on one page, and cover:

1. **What counts as a breaking change:** removing a field, renaming a field,

changing a status code, tightening validation, changing a default sort order.

2. **What counts as additive \(non-breaking\):** adding a new optional field,

adding a new endpoint, adding a new optional query parameter.

3. **Sunset window:** how long V1 keeps running after V2 ships. The TMS

commits to **6 months minimum** so rural training centres on quarterly maintenance schedules can migrate.

4. **Communication:** Deprecation / Sunset / Link headers from day one of

V2; a CHANGELOG entry; an email to every team that holds an API key; a calendar invite for the V1 shutdown date.

5. **Skipping versions:** V1 → V3 is allowed; clients are not forced to migrate

through every intermediate version.

Keep it short. One page, no decoration. **The audit test:** hand the file to your bench partner. They should be able to read it in three minutes and immediately know whether a proposed change is breaking. If they cannot, rewrite it.

**Why this matters at interview.** The most common follow-up question

to “how do you version your API?” is “how do you decide what is and is

not a breaking change?” Without a written policy, the answer is opinion.

With one, the answer is a one-page document committed in the repo.

Senior engineers ship policy; juniors ship code. This is the moment you

start shipping policy.

Step 6 \(Optional, 15 min\) Add a header-based reader as an escape hatch

Some partners \(especially mobile clients with cached CDN URLs\) prefer not to have version in the URL. ASP.NET supports multiple readers at once. In AddApiVersioning:

options.ApiVersionReader = ApiVersionReader.Combine\(

**new** UrlSegmentApiVersionReader\(\),

**new** HeaderApiVersionReader\("X-Api-Version"\)\);

Now GET /api/courses with the header X-Api-Version: 2.0 resolves to V2 even though the URL is unversioned. Document this in the policy doc as **partner-by-****partner opt-in**, not the default, one of the two has to be primary, and URL-segment is more visible during incident response.

Exercise 1 Final evidence \(do not skip\)

Your repo must contain, all simultaneously:

☐ TmsApi.Api/Controllers/V1/CoursesController.cs returning the **TMS**

**API contract** V1 paged list \(**items** \+ paging fields\).

☐ TmsApi.Api/Controllers/V2/CoursesController.cs returning the

wrapped **\{ data, meta, links \}** envelope.

☐ TmsApi.Api/Middleware/V1DeprecationMiddleware.cs stamping the three

headers on every V1 response.

☐ docs/api-versioning-policy.md one page, written in your own voice.

☐ A captured curl -i against /api/v1/courses showing the deprecation

headers \(paste the output into your repo as docs/evidence/v1-deprecation-curl.txt for the capstone\).

☐ A captured curl -i against /api/v2/courses showing **no** deprecation

headers.

 

Troubleshooting Exercise 1

Symptom Likely cause Fix AmbiguousMatchE Two CoursesController Move V1 to xception at classes in the same TmsApi.Api.Controllers.V1 and startup namespace V2 to TmsApi.Api.Controllers.V2 404 on Route template missing Use /api/v1/courses v\{version:apiVersion\} \[Route\("api/v\{version:apiVersio

n\}/courses"\)\] exactly

 

shows in Scalar builder.Services.AddApiVersioni chained after ng\(...\).AddApiExplorer\(...\) AddApiVersioning Only one version AddApiExplorer not Chain it:

 

missing on V1 registered after app.UseMiddleware<V1Deprecation MapControllers Middleware>\(\) Sunset UseMiddleware header Move

*before*

app.MapControllers\(\)

Sunset header Path check is too loose Use appears on V2 context.Request.Path.StartsWith Symptom Likely cause Fix too Segments\("/api/v1"\) not

Contains\("v1"\)

Link header is The \[7..\] slice cuts off Verify the path starts with exactly malformed the slash /api/v1 \(no trailing slash\); log it

once to confirm

 

Exercise 2: CQRS, MediatR Pipelines, Result<T,E>, and IExceptionHandler

Install the Session 1 Exercise 2 packages:

dotnet add TmsApi.Application package MediatR

dotnet add TmsApi.Application package FluentValidation dotnet add TmsApi.Application package

FluentValidation.DependencyInjectionExtensions

 

**Why this exists:** the controller you ship to the Angular team is the contract. If that controller has business logic in it, every contract change forces a code change and every code change risks breaking the contract. CQRS keeps the contract layer thin \(HTTP in, HTTP out\) and the decision layer testable \(commands, queries, handlers\). The two pipeline behaviors and the global IExceptionHandler are what turn this from theory into a production pattern. **M6 → M7 enrollment shape.** In M6, enrollment is **POST** **/api/courses/\{courseId\}/enrollments** with courseId in the path. This exercise introduces a **versioned, flat write**: **POST /api/v2/enrollments** with **studentId** and **courseCode** in the JSON body: and keeps reads on **GET /api/v2/enrollments/\{studentId\}/schedule**. Before you test, **remove or** **disable the old nested enrollment POST** \(or any duplicate enrollment endpoint\) so only one write route owns enrollment; otherwise you will fight ambiguous routes or “which contract did I just hit?” confusion.

**Handler dependencies.** EnrollStudentHandler and GetStudentScheduleHandler require **ICourseService** and **IEnrollmentService** with at least: GetByCodeAsync \(return a Course that includes **Enrollments** for the capacity check\), ExistsAsync\(studentId, courseCode\), AddAsync, and GetByStudentIdAsync \(**include** **Course** for the schedule query\). If M6 only registered **services**, add EF-backed implementations of these interfaces and register them **AddScoped** before you expect dotnet run to succeed. Match method names to the handlers below; use your real TmsDbContext and **Enrollment** model \(**CourseId** foreign key per M5/M6, not a free-standing CourseCode column unless your schema actually has one\).

Step 1 Define a typed Result<T,E> and a domain error type The first thing to fix is the return contract. bool \+ int? \+ string? is not a result. It is three optional fields glued together with hope. Real enterprise CQRS uses a typed Result<T,E> with an Error value object so callers cannot forget which case they are in.

Create TmsApi.Application/Common/EnrollmentError.cs: **namespace** TmsApi.Application.Common; **public sealed** record EnrollmentError\(string Code, string Message\) \{

**public static** EnrollmentError CourseNotFound\(string code\) =>

**new**\("course\_not\_found", $"Course '\{code\}' was not found."\);

**public static** EnrollmentError CourseFull\(string title, int capacity\)

=>

**new**\("course\_full", $"Course '\{title\}' is full \(capacity \{capaci

ty\}\)."\);

**public static** EnrollmentError AlreadyEnrolled\(int studentId, string

code\) =>

**new**\("already\_enrolled", $"Student \{studentId\} is already enroll

ed in \{code\}."\);

\}

Each error variant carries a stable **machine-readable** **Code** \(used in the ProblemDetails.type URI\) and a **human-readable** **Message**. The Code is the contract clients can write if \(error.Code === 'course\_full'\) without parsing the message.

Create TmsApi.Application/Common/Result.cs: **namespace** TmsApi.Application.Common; **public readonly** record **struct** Result<TValue, TError> \{

**private readonly** TValue? \_value;

**private readonly** TError? \_error;

**public** bool IsSuccess \{ **get**; \}

**private** Result\(TValue value\) \{ \_value = value; \_error = **default**; Is Success = **true**; \}

**private** Result\(TError error\) \{ \_value = **default**; \_error = error; Is Success = **false**; \}

**public static** Result<TValue, TError> Success\(TValue value\) => **new**\(v alue\);

**public static** Result<TValue, TError> Failure\(TError error\) => **new**\(e rror\);

**public** TValue Value => IsSuccess

? \_value\!

: **throw new** InvalidOperationException\("Result is failure; call

Match instead of Value."\);

**public** TError Error => \!IsSuccess

? \_error\!

: **throw new** InvalidOperationException\("Result is success; call

Match instead of Error."\);

**public** TOut Match<TOut>\(Func<TValue, TOut> onSuccess, Func<TError, TOut> onFailure\) =>

IsSuccess ? onSuccess\(\_value\!\) : onFailure\(\_error\!\);

\}

**Why not exceptions?** Exceptions are for *unexpected* failures, null

pointers, dropped database connections, bugs. “Course is full” is an

*expected business outcome* of a valid enrollment request. Throwing for

an expected outcome makes stack traces noisy, hurts performance, and

trains learners to treat the type system as advisory. **Use** **Result<T,E>** **for**

**predictable domain failures; let exceptions be loud.**

**Why** **record struct****?** It is a value type, no heap allocation per result,

and value equality is generated for free, which makes test assertions one

line. The readonly modifier prevents handlers from mutating a result

after creation, which is the kind of bug that takes an afternoon to chase. Step 2 Define the command using Result Create TmsApi.Application/Enrollments/Commands/EnrollStudentCommand.cs: **using** MediatR;

**using** TmsApi.Application.Common; **namespace** TmsApi.Application.Enrollments.Commands; **public** record EnrollStudentCommand\(int StudentId, string CourseCode\)

: IRequest<Result<EnrollmentCreated, EnrollmentError>>; **public** record EnrollmentCreated\(int EnrollmentId, int StudentId, string

CourseCode\);

The command’s return type IRequest<Result<EnrollmentCreated, EnrollmentError>> is the contract MediatR uses to find the right handler. Note that the controller does not see this type at all; it sees IMediator.Send\(command\) and unwraps the result. The strong typing protects the *handler*, not the controller. Step 3 Create the handler

Create TmsApi.Application/Enrollments/Commands/EnrollStudentHandler.cs: **using** MediatR;

**using** TmsApi.Application.Common; **using** TmsApi.Application.Interfaces; **using** TmsApi.Domain.Entities;

**namespace** TmsApi.Application.Enrollments.Commands; **public class** EnrollStudentHandler\(

IEnrollmentService enrollmentService,

ICourseService courseService\)

: IRequestHandler<EnrollStudentCommand, Result<EnrollmentCreated, E nrollmentError>>

\{

**public** async Task<Result<EnrollmentCreated, EnrollmentError>> Handl e\(

EnrollStudentCommand command, CancellationToken ct\)

\{

var course = await courseService.GetByCodeAsync\(command.CourseC

ode, ct\);

**if** \(course **is null**\)

**return** Result<EnrollmentCreated, EnrollmentError>.Failure\(

EnrollmentError.CourseNotFound\(command.CourseCode\)\);

**if** \(course.Enrollments.Count >= course.MaxCapacity\)

**return** Result<EnrollmentCreated, EnrollmentError>.Failure\(

EnrollmentError.CourseFull\(course.Title, course.MaxCapa

city\)\);

**if** \(await enrollmentService.ExistsAsync\(command.StudentId, comm

and.CourseCode, ct\)\)

**return** Result<EnrollmentCreated, EnrollmentError>.Failure\(

EnrollmentError.AlreadyEnrolled\(command.StudentId, comm

and.CourseCode\)\);

var enrollment = **new** Enrollment \{

StudentId = command.StudentId, CourseId = course.Id, EnrolledAt = DateTime.UtcNow

\};

await enrollmentService.AddAsync\(enrollment, ct\); **return** Result<EnrollmentCreated, EnrollmentError>.Success\(

**new** EnrollmentCreated\(enrollment.Id, enrollment.StudentId,

course.Code\)\);

\}

\}

**Stop and read this handler carefully.** There is no try/catch. There is

no if/else that throws. Every domain failure is an early return with a

typed Result.Failure\(...\). Every domain success is a single return at

the bottom with Result.Success\(...\). The handler is short, linear, and

*exhaustively reviewable*. You can convince yourself, in 30 seconds, that

every code path produces a typed result. This is the shape every CQRS

handler should have.

Step 4 Create a query \(commands write, queries read\) The “Q” in CQRS is “Query”: a *read* request. Queries are simpler than commands because they have no failure modes worth modelling \(no “course full” on a read\), so they return raw DTOs.

Create TmsApi.Application/Enrollments/Queries/GetStudentScheduleQuery.cs: **using** MediatR;

**namespace** TmsApi.Application.Enrollments.Queries; **public** record GetStudentScheduleQuery\(int StudentId\) : IRequest<Schedul eDto>;

**public** record ScheduleDto\(int StudentId, List<ScheduleItemDto> Courses\); **public** record ScheduleItemDto\(string CourseCode, string Title, string S chedule\);

Create

TmsApi.Application/Enrollments/Queries/GetStudentScheduleHandler.cs: **using** MediatR;

**using** TmsApi.Application.Interfaces; **namespace** TmsApi.Application.Enrollments.Queries; **public class** GetStudentScheduleHandler\(IEnrollmentService repo\)

: IRequestHandler<GetStudentScheduleQuery, ScheduleDto> \{

**public** async Task<ScheduleDto> Handle\(

GetStudentScheduleQuery query, CancellationToken ct\)

\{

var enrollments = await repo.GetByStudentIdAsync\(query.StudentI

d, ct\);

var items = enrollments.Select\(e => **new** ScheduleItemDto\(

e.Course.Code,

e.Course.Title,

"TBD"\)\).ToList\(\);

**return new** ScheduleDto\(query.StudentId, items\);

\}

\}

The M6 **Course** shape does not include a separate timetable string; the third field is a **placeholder** until you add something like Course.MeetingSummary in your own schema. The teaching point is the query \+ DTO projection, not the column name.

Step 5 Create a ValidationBehavior pipeline Behaviors run *before* the handler. They are how you keep cross-cutting concerns \(validation, logging, auth, metrics\) out of every handler. Create TmsApi.Application/Behaviors/ValidationBehavior.cs: **using** FluentValidation;

**using** MediatR;

**namespace** TmsApi.Application.Behaviors; **public class** ValidationBehavior<TRequest, TResponse>\(

IEnumerable<IValidator<TRequest>> validators\)

: IPipelineBehavior<TRequest, TResponse> where TRequest : notnull \{

**public** async Task<TResponse> Handle\(

TRequest request,

RequestHandlerDelegate<TResponse> next, CancellationToken ct\)

\{

**if** \(\!validators.Any\(\)\)

**return** await next\(\);

var context = **new** ValidationContext<TRequest>\(request\); var failures = validators

.Select\(v => v.Validate\(context\)\) .SelectMany\(result => result.Errors\) .Where\(f => f **is** not **null**\) .ToList\(\);

**if** \(failures.Count > 0\)

**throw new** ValidationException\(failures\);

**return** await next\(\);

\}

\}

Create TmsApi.Application/Enrollments/Commands/EnrollStudentValidator.cs: **using** FluentValidation;

**namespace** TmsApi.Application.Enrollments.Commands; **public class** EnrollStudentValidator : AbstractValidator<EnrollStudentCo mmand>

\{

**public** EnrollStudentValidator\(\)

\{

RuleFor\(x => x.StudentId\).GreaterThan\(0\)

.WithMessage\("Student ID must be a positive number."\);

RuleFor\(x => x.CourseCode\).NotEmpty\(\)

.WithMessage\("Course code is required."\);

RuleFor\(x => x.CourseCode\).Matches\(@"^\[A-Z\]\{3\}-\\d\{3\}$"\)

.WithMessage\("Course code must follow the format XXX-000 \(e.

g., CSE-101\)."\);

\}

\}

**Why** **throw** **here, after we just said “do not throw”?** Validation failures

are a programming-or-input failure, not a business outcome. The

request is *malformed*. Rejecting it with 400 Bad Request is one line in

IExceptionHandler \(Step 7\). If we returned Result.Failure\(...\) from

validation, every caller would have to translate it back to 400 duplicated

logic in every controller. The exception is the cheap, central path. Step 6 Add a LoggingBehavior with correlation IDs A real CQRS pipeline has more than one behavior. The next one every team adds is structured logging with a correlation id, so a request can be traced from the controller through the handler to the database. Create TmsApi.Application/Behaviors/LoggingBehavior.cs : **using** System.Diagnostics;

**using** MediatR;

**using** Microsoft.Extensions.Logging; **namespace** TmsApi.Application.Behaviors;

**public class** LoggingBehavior<TRequest, TResponse>\(

ILogger<LoggingBehavior<TRequest, TResponse>> logger\)

: IPipelineBehavior<TRequest, TResponse> where TRequest : notnull \{

**public** async Task<TResponse> Handle\(

TRequest request,

RequestHandlerDelegate<TResponse> next, CancellationToken ct\)

\{

var requestName = **typeof**\(TRequest\).Name; var correlationId = Activity.Current?.TraceId.ToString\(\) ?? Gui

d.NewGuid\(\).ToString\("N"\);

var stopwatch = Stopwatch.StartNew\(\);

**using** var scope = logger.BeginScope\(**new** Dictionary<string, obje

ct>

\{

\["RequestName"\] = requestName, \["CorrelationId"\] = correlationId

\}\);

logger.LogInformation\("Handling \{RequestName\} \(cid=\{Correlation

Id\}\)", requestName, correlationId\);

**try**

\{

var response = await next\(\); stopwatch.Stop\(\);

logger.LogInformation\(

"Handled \{RequestName\} in \{ElapsedMs\}ms \(cid=\{Correlati

onId\}\)",

requestName, stopwatch.ElapsedMilliseconds, correlation

Id\);

**return** response;

\}

**catch** \(Exception ex\)

\{

stopwatch.Stop\(\);

logger.LogError\(ex,

"Failed \{RequestName\} after \{ElapsedMs\}ms \(cid=\{Correla

tionId\}\)",

requestName, stopwatch.ElapsedMilliseconds, correlation

Id\);

**throw**;

\}

\}

\}

**Order matters and the wrong order is a silent failure.** When multiple

behaviors are registered, MediatR runs them in **registration order**.

Register LoggingBehavior first so it wraps ValidationBehavior. That

way the log scope is open before validation throws, and you see a

Failed EnrollStudentCommand after 3ms line for every validation

rejection. Reverse the order and a validation rejection appears as silent

dead air in the logs while a 400 goes back to the client. You will spend

an afternoon debugging “why is my logging not working” before you

find this.

Step 7 Wire up IExceptionHandler for global ProblemDetails ValidationBehavior throws ValidationException. Other unexpected things \(NullReferenceException, EF DbUpdateException\) can throw too. None of those should reach the controller unhandled. .NET 10 ships IExceptionHandler for exactly this one place to translate exceptions into RFC 7807 ProblemDetails. Create TmsApi.Api/ExceptionHandlers/GlobalExceptionHandler.cs: **using** FluentValidation;

**using** Microsoft.AspNetCore.Diagnostics; **using** Microsoft.AspNetCore.Mvc;

**namespace** TmsApi.Api.ExceptionHandlers; **public class** GlobalExceptionHandler\(ILogger<GlobalExceptionHandler> log ger\) : IExceptionHandler

\{

**public** async ValueTask<bool> TryHandleAsync\(

HttpContext httpContext, Exception exception, CancellationToken

ct\)

\{

var \(status, title, detail, errors\) = exception **switch** \{

ValidationException ve => \(

StatusCodes.Status400BadRequest, "Validation failed", "One or more fields are invalid. See errors for details.

",

\(IDictionary<string, string\[\]>?\)ve.Errors

.GroupBy\(e => e.PropertyName\) .ToDictionary\(g => g.Key, g => g.Select\(e => e.Erro

rMessage\).ToArray\(\)\)\),

\_ => \(

StatusCodes.Status500InternalServerError, "Server error", $"An unexpected error occurred. Trace ID: \{httpContext.

TraceIdentifier\}",

**null**\)

\};

**if** \(status == StatusCodes.Status500InternalServerError\)

logger.LogError\(exception, "Unhandled exception \(trace=\{Tra

ceId\}\)", httpContext.TraceIdentifier\);

var problem = **new** ProblemDetails \{

Status = status,

Title = title,

Detail = detail,

Instance = httpContext.Request.Path

\};

**if** \(errors **is** not **null**\) problem.Extensions\["errors"\] = errors; httpContext.Response.StatusCode = status; httpContext.Response.ContentType = "application/problem\+json"; await httpContext.Response.WriteAsJsonAsync\(problem, ct\); **return true** ;

\}

\}

The handler does three things every senior reviewer expects:

1. Maps FluentValidation.ValidationException to HTTP **400** with a

structured errors dictionary keyed by field, the same shape ASP.NET produces for \[ApiController\] model state failures, so the Angular team has one error contract to handle for every kind of validation problem.

2. Logs unexpected exceptions with the request trace id, so support can find

the failure in logs from the user’s complaint email.

3. Never leaks an exception message or stack trace to the client.

Step 8 Register MediatR, behaviors, and the exception handler Open TmsApi.Api/Program.cs and add \(top of builder.Services\): builder.Services.AddMediatR\(cfg =>

cfg.RegisterServicesFromAssembly\(**typeof**\(EnrollStudentHandler\).Assem bly\)\);

builder.Services.AddValidatorsFromAssembly\(**typeof**\(EnrollStudentValidato r\).Assembly\);

*// LoggingBehavior FIRST—it must wrap ValidationBehavior* builder.Services.AddTransient\(**typeof**\(IPipelineBehavior<,>\), **typeof**\(Logg ingBehavior<,>\)\);

builder.Services.AddTransient\(**typeof**\(IPipelineBehavior<,>\), **typeof**\(Vali dationBehavior<,>\)\);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>\(\); builder.Services.AddProblemDetails\(\); After var app = builder.Build\(\);, add app.UseExceptionHandler\(\) **before** app.UseRouting\(\) \(or app.MapControllers\(\) if you do not use UseRouting\): app.UseExceptionHandler\(\);

The order in Program.cs matters: LoggingBehavior is registered *before*

ValidationBehavior so it runs *first* and wraps validation failures inside

its log scope. If you register them the other way around, validation

rejections will not appear in the structured log scope and you will think

your logging is broken.

Step 9 Refactor the controller

Replace the business logic in your enrollments controller with MediatR dispatch and use the typed Result to translate domain errors into the right HTTP status: **using** Asp.Versioning;

**using** MediatR;

**using** Microsoft.AspNetCore.Mvc; **using** TmsApi.Application.Enrollments.Commands; **using** TmsApi.Application.Enrollments.Queries;

\[ApiController\]

\[Route\("api/v\{version:apiVersion\}/enrollments"\)\] \[ApiVersion\("2.0"\)\]

**public class** EnrollmentsController\(IMediator mediator\) : ControllerBase \{

\[HttpPost\]

**public** async Task<IActionResult> Enroll\(

EnrollStudentCommand command, CancellationToken ct\)

\{

var result = await mediator.Send\(command, ct\); **return** result.Match<IActionResult>\(

onSuccess: created => CreatedAtAction\(

nameof\(GetSchedule\), **new** \{ studentId = created.StudentId \}, created\),

onFailure: error => \{

var status = error.Code **switch** \{

"course\_not\_found" => StatusCodes.Status404NotFound, "course\_full" or "already\_enrolled" => StatusCodes.

Status409Conflict,

\_ => StatusCodes.Status400BadRequest

\};

**return** Problem\(

statusCode: status, title: "Enrollment rejected", detail: error.Message, type: $"https://tms.local/errors/\{error.Code\}"\);

\}\);

\}

\[HttpGet\("\{studentId\}/schedule"\)\]

**public** async Task<IActionResult> GetSchedule\(

int studentId, CancellationToken ct\)

\{

var schedule = await mediator.Send\(

**new** GetStudentScheduleQuery\(studentId\), ct\);

**return** Ok\(schedule\);

\}

\}

The mapping course\_not\_found → 404 and course\_full /

already\_enrolled → 409 is REST-correct. A 400 says “your request is

malformed”, that is what IExceptionHandler already returns for

validation. A 404 says “your request is well-formed but the resource you

named does not exist.” A 409 Conflict says “your request is well-

formed and the resource exists, but the resource state forbids this

operation.” Keep those three error categories distinct; lazy controllers

throw everything as 400 and lose the diagnostic value of the status code. Step 10 Test the entire pipeline end-to-end \(this is the audit moment\) Run the API:

dotnet build

dotnet run--project TmsApi.Api In a second terminal, run all four cases. **Watch the API’s console log alongside** **each curl** the audit is in both places. **Test 1: Valid enrollment.**

curl-i -X POST https://localhost:5001/api/v2/enrollments \\

-H "Content-Type: application/json" \\

-d '\{"studentId": 1, "courseCode": "CSE-101"\}'

Expected response: HTTP/1.1 201 Created with body \{"enrollmentId":1,"studentId":1,"courseCode":"CSE-101"\} and a Location header pointing at /api/v2/enrollments/1/schedule. Expected log lines \(in order\):

info: Handling EnrollStudentCommand \(cid=…\)

info: Handled EnrollStudentCommand in 47ms \(cid=…\) **Test 2: Invalid course-code format → 400 from** **IExceptionHandler****.** curl-i -X POST https://localhost:5001/api/v2/enrollments \\

-H "Content-Type: application/json" \\

-d '\{"studentId": 1, "courseCode": "invalid"\}'

Expected response: HTTP/1.1 400 Bad Request with Content-Type: application/problem\+json and a body containing "errors": \{ "courseCode": \["Course code must follow the format XXX-000 \(e.g., CSE-101\)."\] \}. Expected log lines:

info: Handling EnrollStudentCommand \(cid=abc…\) fail: Failed EnrollStudentCommand after 3ms \(cid=abc…\)

FluentValidation.ValidationException: ...

**Audit moment.** The \(cid=abc…\) value in both lines is the **same**

**correlation id** \(often the current Activity trace id when one is present,

otherwise the fallback from LoggingBehavior\). Copy it and grep your

logs for that token, you get one record of the whole request. **This is the**

**moment “structured logging” stops being a buzzword.** **Test 3: Non-existent course code → 404 from** **Result.Failure****.** curl-i -X POST https://localhost:5001/api/v2/enrollments \\

-H "Content-Type: application/json" \\

-d '\{"studentId": 1, "courseCode": "ZZZ-999"\}'

Expected response: HTTP/1.1 404 Not Found with Content-Type: application/problem\+json and a body containing "type": "https://tms.local/errors/course\_not\_found".

**Stop. Notice what just happened.** The handler did not throw. It

returned Result.Failure\(EnrollmentError.CourseNotFound\(...\)\). The

controller pattern-matched on the error code and returned Symptom Likely cause Quick fix Validation app.UseExceptionHandler Add it immediately after app = rejection returns \(\) not called, or registered builder.Build\(\);, before 500 after MapControllers MapControllers

Problem\(statusCode: 404, ...\). **Two different code paths produced**

**two different** **application/problem\+json** **bodies, but the Angular**

**team consumes them with the same code.** That is the architectural

payoff of Result<T,E> \+ IExceptionHandler working together. **Test 4: Duplicate enrollment → 409 from** **Result.Failure****.** *\# Run Test 1 again—same student, same course*

curl-i -X POST https://localhost:5001/api/v2/enrollments \\

-H "Content-Type: application/json" \\

-d '\{"studentId": 1, "courseCode": "CSE-101"\}'

Expected response: HTTP/1.1 409 Conflict with body containing "type": "https://tms.local/errors/already\_enrolled". Exercise 2 Final evidence \(do not skip\)

Your repo must contain:

☐ TmsApi.Application/Common/Result.cs and EnrollmentError.cs.

☐ TmsApi.Application/Enrollments/Commands/EnrollStudentCommand.cs \+

EnrollStudentHandler.cs \+ EnrollStudentValidator.cs.

☐ TmsApi.Application/Enrollments/Queries/GetStudentScheduleQuery.cs

\+ handler.

☐ TmsApi.Application/Behaviors/ValidationBehavior.cs and

LoggingBehavior.cs, both registered as IPipelineBehavior<,> in Program.cs \(Logging first, Validation second\).

☐ TmsApi.Api/ExceptionHandlers/GlobalExceptionHandler.cs registered

via AddExceptionHandler<T> and app.UseExceptionHandler\(\) *before* MapControllers.

☐ EnrollmentsController with **no** business if/switch only HTTP-status

mapping from Result.Match\(...\).

☐ All four curl tests above produce the expected status codes and bodies.

Troubleshooting Exercise 2

Symptom Likely cause Quick fix

 

No service for cfg.RegisterServicesFromAss AddMediatR not called or type 'IMediator' embly\(typeof\(EnrollStudentH wrong assembly reference andler\).Assembly\)

 

not run ValidationBehavior AddValidatorsFromAssembly\(t ypeof\(EnrollStudentValidato registered with wrong type Validation does Validator not registered, or Confirm

r\).Assembly\)

 

logs nothing for a registered before first, so it wraps Validation LoggingBehavior LoggingBehavior ValidationBehavior Reverse the order—Logging failed command

Result.Value Calling .Value instead Always .Match\(onSuccess, throws on failure of .Match\(...\) onFailure\) the type system

makes the failure case

unmissable

Handler not IRequestHandler<T, R> Compare types literally resolved signature does not match generics with Result<TValue,

the command exactly TError> are easy to mistype

404 controller.Problem\(...\) returns plain The controller’s onFailure JSON, not not used branch must call application/prob Problem\(statusCode: …, lem\+json type: …\)

 

Session Integration Challenge: End-to-end on the V2 enrollment

When both exercises are done, the integration is the proof. Issue this single curl from a terminal and confirm every layer below it cooperates: curl-i -X POST https://localhost:5001/api/v2/enrollments \\

-H "Content-Type: application/json" \\

-d '\{"studentId": 1, "courseCode": "invalid"\}'

What you should see, **in order**, in the API’s console log and the response:

1. The middleware did **not** stamp deprecation headers \(the request is on V2,

not V1\).

2. The request reached EnrollmentsController.Enroll and was dispatched

via IMediator.Send.

3. LoggingBehavior opened a scope with RequestName =

EnrollStudentCommand and a CorrelationId. A Handling EnrollStudentCommand \(cid=…\) log line is visible in the console.

4. ValidationBehavior ran and **threw** ValidationException because

"invalid" does not match ^\[A-Z\]\{3\}-\\d\{3\}$.

5. The exception was *not* caught in the handler. It propagated out of MediatR.

6. app.UseExceptionHandler\(\) invoked GlobalExceptionHandler, which

produced HTTP 400 with application/problem\+json, a ProblemDetails body, and an errors.courseCode array containing the regex message.

7. LoggingBehavior logged a Failed EnrollStudentCommand after Xms

\(cid=…\) line with the **same** correlation id as the Handling line.

If any one of those is missing, you have a wiring gap somewhere in Session 1. The *order* of those events is the architectural shape, once it lines up, you have the contract the rest of M7 builds on.

Now run the **deprecation cross-check** to prove versioning still works: curl-i https://localhost:5001/api/v1/courses curl-i https://localhost:5001/api/v2/courses Confirm V1 carries the three deprecation headers and V2 does not. Capture both outputs into docs/evidence/session1-curl.txt. The capstone marker grades versioning by reading that file.

**The senior moment.** Look at what you just built. A single client request

\(POST /api/v2/enrollments with a malformed body\) flowed through

versioning routing, controller dispatch, MediatR, two pipeline behaviors,

a validator, an exception, and an exception handler and produced one

greppable correlation id, one well-formed application/problem\+json

response, and one set of structured log lines. **This is the architectural**

**seam every other module in M7 plugs into.** Keep that shape stable

today; the rest of the week is about clipping new features \(caching, rate

limiting, real-time push, resilience, observability\) onto it. Session 1 Checkpoint

Tick all of these before you leave the room:

☐ GET /api/v1/courses returns the V1 shape **and** carries Deprecation: true,

a Sunset date, and a Link to V2.

☐ GET /api/v2/courses returns the V2 shape and does **not** carry

deprecation headers.

☐ docs/api-versioning-policy.md exists, is one page, and a peer can read it

in three minutes.

☐ EnrollmentsController has zero if/switch statements that talk about

business rules.

☐ A POST with an invalid course code returns 400 with Content-Type:

application/problem\+json and ProblemDetails.errors.courseCode populated by IExceptionHandler.

☐ A POST against a non-existent course returns **404** with type:

https://tms.local/errors/course\_not\_found.

☐ A duplicate enrollment returns **409** with type:

https://tms.local/errors/already\_enrolled.

☐ Each request produces *two* structured log lines \(Handling/Handled or

Handling/Failed\) sharing a single CorrelationId.

☐ dotnet test is green.

☐ The four curl commands and their outputs are committed in

docs/evidence/session1-curl.txt.

If any box is unchecked, do not move to Session 2. The next three sessions all attach to the surface you finished today; gaps here cascade.

 

Common Errors: Session 1

Symptom Likely cause Quick fix AmbiguousMatchEx Two controllers with the Move V1 to ception at startup TmsApi.Api.Controllers.V1 same name in the same

namespace and V2 to TmsApi.Api.Controllers.V2

 

V1 response UseMiddleware<V1Deprecat Move it before ionMiddleware>\(\) app.MapControllers\(\) missing registered too late deprecation

headers

Validation app.UseExceptionHandler\( Add it immediately after app = rejection returns \) not called, or registered builder.Build\(\);, before 500 after MapControllers MapControllers

 

logs nothing for a registered before first wraps Validation LoggingBehavior LoggingBehavior ValidationBehavior Reverse the order—Logging failed command

Result.Value Calling .Value instead Always .Match\(onSuccess, throws on failure of .Match\(...\) onFailure\) the type system

makes the failure case

unmissable

Handler not IRequestHandler<T, R> Compare types literally

signature does not match generics with Result<TValue, Symptom Likely cause Quick fix resolved the command exactly TError> are easy to mistype 404 returns plain The controller’s failure Use Problem\(statusCode: JSON, not branch returns NotFound\(\) 404, type: ...\) in the application/prob instead of Problem\(...\) onFailure arm of Match lem\+json

 

Bridge to Session 2

You now have a clean, versioned API surface and a CQRS pipeline that turns business decisions into typed results. Session 2 adds the two operational concerns the dashboard team will hit on Monday morning: **caching that survives** **a 50-request burst with one DB query**, and **rate limiting that distinguishes** **anonymous, free, and paid callers**. Both attach to the surface you finished today without it, neither makes sense.

Read **Module 7 Lab Session 2** \(handout for the next classroom session\) before the Session 2 lecture if you want to follow along faster.



Module 7 Lab: Session 2: Throughput Under Production Load

**Module** M7: Advanced Web API Development **Session** 2 of 4

**Exercises** 3 \(HybridCache with observable stampede protection\), 4

\(Tier-aware rate limiting\)

 

Welcome to Monday Morning

The dashboard goes live Sunday night. By 09:14 Monday the on-call channel is on fire.

200 students log into the TMS. Each opens their dashboard. Each dashboard fetches the “Popular Courses” list once. The “Popular Courses” endpoint queries Courses join Enrollments and aggregates count per course, 200 ms on a warm cache, 1.4 seconds on a cold cache. The cache was empty when the deploy went out, so the first 50 requests all fired the same query at the same time. Database CPU climbed to 95 %, response times for *every* endpoint passed 6 seconds, and the admin dashboard timed out at 30, including the dashboard ops uses to see why the API is slow.

While that was unfolding, the integration team’s nightly export script, left running with a typo’d retry loop, started hammering /api/v2/courses/search at 100 requests per second. The script will run until someone notices, in about 90 minutes.

**Both problems are entirely preventable. Session 2 is where you prevent** **them.**

By the end of today, 50 concurrent students hitting an empty cache produces exactly **one** database query \(Exercise 3\), and one misconfigured script can no longer drown everyone else \(Exercise 4\). You will *prove* both, in logs, not on faith. Before You Begin: Session 2 sync

**Primary check:** Open **Scalar** starting from your API’s HTTPS base URL \(**https://localhost:5001** in most lab profiles\). Try **…/scalar/v1** first, then **…/scalar** if the first path 404s. Use **Try it** on **GET /api/v1/courses** and **GET /api/v2/courses** confirm V1 shows deprecation-related headers where your middleware emits them and V2 does not.

Optional scripted replay:

curl-i https://localhost:5001/api/v1/courses curl-i https://localhost:5001/api/v2/courses If any of those fail, fix Session 1 before continuing, Session 2’s caching attaches to the V2 read path for courses from Session 1, and Session 2’s rate limiting needs the IExceptionHandler plumbing for ProblemDetails. Install the Session 2 package:

dotnet add TmsApi.Api package Microsoft.Extensions.Caching.Hybrid \(Rate limiting ships with Microsoft.AspNetCore.RateLimiting in the runtime. No extra NuGet.\)

 

Exercise 3: HybridCache with Observable Stampede Protection \(LO 7.2\)

**Scenario:** The “Popular Courses” endpoint is queried 1,000 times per minute on Monday mornings. Every request hits the database with the same query. The dashboard team also asked for one specific guarantee: “tell us, in the logs, how many of those requests actually touched the database.” Without that signal you cannot prove the cache works in production. You will implement HybridCache with atomic refetching, tag-based invalidation, **and** a hit/miss log line per call so the cache becomes observable, not magical.

Step 1 Register HybridCache \(and decide about Redis later\) Open TmsApi.Api/Program.cs and add: builder.Services.AddHybridCache\(options => \{

options.DefaultEntryOptions = **new** HybridCacheEntryOptions

\{

Expiration = TimeSpan.FromMinutes\(10\),

LocalCacheExpiration = TimeSpan.FromMinutes\(2\)

\};

\}\);

For this lab, leave the L2 distributed cache as the default in-memory fallback single-node is fine while you build the pattern. **In production**, register a Redis-backed IDistributedCache *before* AddHybridCache and HybridCache will pick it up automatically:

*// Production-only leave commented in lab*

*// builder.Services.AddStackExchangeRedisCache\(options => // \{*

*//* *options.Configuration = builder.Configuration.GetConnectionStrin*

*g\("Redis"\);*

*//* *options.InstanceName = "tms:"; // \}\);*

*// builder.Services.AddHybridCache\(\);*

The instance prefix \("tms:"\) is the small detail every team forgets.

Without it, two services sharing the same Redis cluster collide on

course:CSE-101. With it, they live in separate keyspaces

\(tms:course:CSE-101 vs billing:course:CSE-101\) and ops can scan a

single service’s keys with SCAN MATCH tms:\*. Step 2 Pick a cache key strategy that survives schema changes Cache keys are part of your contract with future-you. The first time you change a DTO shape and forget to clear the cache, every server in production will serve stale shapes for ten minutes. The fix is to embed a **schema version** in the key: *// In TmsApi.Infrastructure/Caching/CacheKeys.cs* **namespace** TmsApi.Infrastructure.Caching; **public static class** CacheKeys

\{

**private** const string SchemaVersion = "v2";

**public static** string Course\(string code\) => $"\{SchemaVersion\}:cours e:\{code\}";

**public static** string CoursesAll => $"\{SchemaVersion\}:courses:all";

**public** const string CoursesTag = "courses"; \}

When you change CourseDto, bump SchemaVersion to "v3" and old entries become unreachable instantly. No coordinated cache flush, no support page, no incident.

Step 3 Create a cached course service with hit/miss observability Create TmsApi.Infrastructure/Services/CachedCourseService.cs: **using** Microsoft.Extensions.Caching.Hybrid; **using** Microsoft.Extensions.Logging; **using** TmsApi.Application.DTOs;

**using** TmsApi.Application.Interfaces; **using** TmsApi.Infrastructure.Caching; **namespace** TmsApi.Infrastructure.Services; **public class** CachedCourseService\(

HybridCache cache,

ICourseServiceservice,

ILogger<CachedCourseService> logger\)

: ICachedCourseService

\{

**public** async Task<CourseDto> GetCourseAsync\(string code, Cancellati onToken ct\)

\{

var key = CacheKeys.Course\(code\); var dbHit = **false**;

var dto = await cache.GetOrCreateAsync\(

key,

\(service, code\),

async \(state, token\) => \{

dbHit = **true**;

logger.LogInformation\("Cache MISS for \{Key\} fetching f

rom DB", key\);

var course = await state.service.GetByCodeAsync\(state.c

ode, token\)

?? **throw new** NotFoundException\($"Course \{state.code\}

not found."\);

**return new** CourseDto\(

course.Id, course.Title, course.Code, course.MaxCapacity, course.Enrollments.Count\);

\},

tags: \[CacheKeys.CoursesTag\], cancellationToken: ct\);

**if** \(\!dbHit\)

logger.LogInformation\("Cache HIT for \{Key\}", key\);

**return** dto;

\}

**public** async Task<List<CourseDto>> GetAllCoursesAsync\(CancellationT oken ct\)

\{

var key = CacheKeys.CoursesAll; var dbHit = **false**;

var list = await cache.GetOrCreateAsync\(

key,

service,

async \(state, token\) => \{

dbHit = **true**;

logger.LogInformation\("Cache MISS for \{Key\} fetching f

rom DB", key\);

var courses = await state.GetAllAsync\(token\); **return** courses.Select\(c => **new** CourseDto\(

c.Id, c.Title, c.Code, c.MaxCapacity, c.Enrollments.Count\)\).ToList\(\);

\},

tags: \[CacheKeys.CoursesTag\], cancellationToken: ct\);

**if** \(\!dbHit\)

logger.LogInformation\("Cache HIT for \{Key\}", key\);

**return** list;

\}

**public** async Task InvalidateCourseCacheAsync\(CancellationToken ct\)

\{

logger.LogInformation\("Invalidating cache tag \{Tag\}", CacheKeys.

CoursesTag\);

await cache.RemoveByTagAsync\(CacheKeys.CoursesTag, ct\);

\}

\}

Two things to notice. First, the state parameter \(\(service, code\) or

service \) is passed explicitly to the factory this avoids capturing repo

and code in a closure, which would allocate a new delegate every call. In

a hot path doing 1,000 req/min, those allocations add up. Second, the

hit/miss log is implemented by setting a flag *inside* the factory \(which

only runs on miss\) and reading it after GetOrCreateAsync returns. There

is no public hit/miss API on HybridCache this is the recommended

pattern.

Step 4 Register the service

In Program.cs:

builder.Services.AddScoped<ICachedCourseService, CachedCourseService>\(\);

Step 5 Use the cached service in your handler Update the query handler that fetches courses to use ICachedCourseService instead of the service directly.

Step 6 Invalidate on writes \(this is the part teams forget\) In every handler that creates, updates, or deletes a course, call InvalidateCourseCacheAsync. The single most common production cache bug is a write path that bypasses invalidation; the read path then serves stale data for the full L2 TTL. Make this a habit, not a “we’ll add it later”: **public class** UpdateCourseHandler\(

ICourseService service,

ICachedCourseService cachedService\)

: IRequestHandler<UpdateCourseCommand, bool> \{

**public** async Task<bool> Handle\(UpdateCourseCommand command, Cancell ationToken ct\)

\{

await service.UpdateAsync\(command, ct\); await cachedService.InvalidateCourseCacheAsync\(ct\); **return true** ;

\}

\}

Step 7 Verify stampede protection in the logs \(not just the response\) Run the API. Send 50 concurrent requests \(Scalar cannot easily do this. use **PowerShell** or **curl** **in parallel** for this load spike\): *\# PowerShell*

1..50 | ForEach-Object-Parallel \{

Invoke-RestMethod https://localhost:5001/api/v2/courses | Out-Null \} -ThrottleLimit 50

Now grep the application log:

*\# In your dotnet run output, you should see these counts \# Cache MISS for v2:courses:all fetching from DB* *\(exactly 1\) \# Cache HIT for v2:courses:all* *\(49\)* This is the proof. **One** miss line, **forty-nine** hit lines. If you see more than one miss line, atomic refetching is not working \(most likely cause: you used GetAsync \+ manual set instead of GetOrCreateAsync\). Also confirm in your EF Core SQL log: exactly **one** SELECT against Courses for those 50 requests.

Step 8 Verify tag invalidation works

Issue a write that triggers invalidation, then re-run the load test: curl-X PUT https://localhost:5001/api/v2/courses/1 \\

-H "Content-Type: application/json" \\

-d '\{"title": "Updated title"\}'

You should see one log line:

Invalidating cache tag courses

The next read produces a fresh MISS \(good the write was visible immediately\) followed by hits.

**Expected result:**

 Cold cache: 50 concurrent requests → 1 database query, 1 MISS log, 49

HIT logs.

 Warm cache: 50 requests → 0 database queries, 50 HIT logs.

 After invalidation: next read produces 1 MISS, then HITs again.

 Bumping SchemaVersion from v2 to v3 causes every cached entry to

become unreachable on next read \(no manual flush needed\).

Troubleshooting

Symptom Fix

No service for type Ensure AddHybridCache\(\) is called in Program.cs 'HybridCache'

Multiple DB queries Verify you are using GetOrCreateAsync \(not GetAsync under concurrent load \+ manual set\). The atomic refetching only works with

the factory overload

Cache never invalidates Confirm you are calling RemoveByTagAsync with the

same tag string used in GetOrCreateAsync

Hit/miss log only shows The factory body sets dbHit = true. Make sure the MISS closure variable is declared in the *outer* method

scope, not inside the factory

Symptom Fix

Stale data after deploy Bump SchemaVersion in CacheKeys whenever a DTO

changes shape; old entries become unreachable on

the next read

Checkpoint

The TMS course endpoint now survives Monday morning traffic spikes **observably**. 50 concurrent requests produce 1 database query and a clean 1-MISS / 49-HIT log signature you can show in incident review. Tag-based invalidation works on every write path. The cache key strategy is forward-compatible with DTO changes via SchemaVersion. Production-readiness gap: swap the in-memory L2 for Redis \(Step 1 commented snippet\) when you scale beyond one pod.

Exercise 4: Tier-Aware Rate Limiting \(LO 7.3\) **Scenario:** A misconfigured script at one training centre is sending 100 requests per second to the course search endpoint. Yesterday’s outage report blamed it for 5 seconds of latency for *every* user. The fix is partitioned rate limiting: every API key gets its own bucket, paid integration partners get a higher tier, and the expensive transcript endpoint gets a separate concurrency limit \(not just per-second, but “how many transcripts can run *at the same time*”\). You will build this end-to-end.

Step 1 Decide what counts as “the caller”

Real APIs do not partition by IP corporate NATs put thousands of users behind one IP. Real APIs partition by **API key** \(or, after M12 lands, JWT subject\). For this lab, accept an X-Api-Key header and a tier inferred from a small lookup table: *// TmsApi.Api/RateLimiting/ApiKeyTier.cs*

**namespace** TmsApi.Api.RateLimiting; **public enum** ApiKeyTier \{ Anonymous, Free, Paid \}

**public static class** ApiKeyResolver \{

**private static readonly** Dictionary<string, ApiKeyTier> Keys = **new**\(S tringComparer.Ordinal\)

\{

\["tms-free-demo-001"\] = ApiKeyTier.Free, \["tms-paid-001"\] = ApiKeyTier.Paid

\};

**public static** \(string PartitionKey, ApiKeyTier Tier\) Resolve\(HttpCo ntext ctx\)

\{

var key = ctx.Request.Headers\["X-Api-Key"\].ToString\(\); **if** \(string.IsNullOrEmpty\(key\)\)

**return** \(ctx.Connection.RemoteIpAddress?.ToString\(\) ?? "anon

ymous", ApiKeyTier.Anonymous\);

**return** Keys.TryGetValue\(key, **out** var tier\)

? \(key, tier\)

: \(key, ApiKeyTier.Anonymous\);

\}

\}

In M12 you will replace this lookup with real authentication. For M7 the

goal is to teach **partitioning** the mechanism is identical whether the

partition key comes from a header, a JWT, or a tenant id pulled from a

database.

Step 2 Configure tier-aware Token Bucket as the default policy Open TmsApi.Api/Program.cs:

**using** System.Threading.RateLimiting; **using** Microsoft.AspNetCore.Mvc; **using** Microsoft.AspNetCore.RateLimiting; **using** TmsApi.Api.RateLimiting;

builder.Services.AddRateLimiter\(options => \{

options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>\(httpContext =>

\{

var \(partitionKey, tier\) = ApiKeyResolver.Resolve\(httpContext\); **return** tier **switch**

\{

ApiKeyTier.Paid => RateLimitPartition.GetTokenBucketLimiter

\(

partitionKey: $"paid:\{partitionKey\}", factory: \_ => **new** TokenBucketRateLimiterOptions \{

TokenLimit = 200, TokensPerPeriod = 100, ReplenishmentPeriod = TimeSpan.FromSeconds\(10\), QueueLimit = 0, AutoReplenishment = **true**

\}\),

ApiKeyTier.Free => RateLimitPartition.GetTokenBucketLimiter

\(

partitionKey: $"free:\{partitionKey\}", factory: \_ => **new** TokenBucketRateLimiterOptions \{

TokenLimit = 30, TokensPerPeriod = 10, ReplenishmentPeriod = TimeSpan.FromSeconds\(10\), QueueLimit = 0, AutoReplenishment = **true**

\}\),

\_ => RateLimitPartition.GetTokenBucketLimiter\(

partitionKey: $"anon:\{partitionKey\}", factory: \_ => **new** TokenBucketRateLimiterOptions \{

TokenLimit = 10, TokensPerPeriod = 5, ReplenishmentPeriod = TimeSpan.FromSeconds\(10\), QueueLimit = 0, AutoReplenishment = **true**

\}\)

\};

\}\);

options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

options.OnRejected = async \(context, ct\) =>

\{

var retryAfter = "10";

**if** \(context.Lease.TryGetMetadata\(MetadataName.RetryAfter, **out** v

ar ts\)\)

retryAfter = \(\(int\)ts.TotalSeconds\).ToString\(\);

context.HttpContext.Response.Headers.RetryAfter = retryAfter; context.HttpContext.Response.ContentType = "application/problem

\+json";

await context.HttpContext.Response.WriteAsJsonAsync\(**new** Problem

Details

\{

Title = "Rate limit exceeded", Detail = $"Too many requests. Retry after \{retryAfter\} seco

nds.",

Status = StatusCodes.Status429TooManyRequests, Type = "https://tms.local/errors/rate\_limit\_exceeded"

\}, ct\);

\};

\}\);

Three things every senior reviewer expects to see here, all present:

1. **Per-caller partition** heavy use by one client cannot starve another.

2. **Tier policy** paid customers get a higher ceiling without code branches in

handlers.

3. **Retry-After** **from the lease metadata** not a hard-coded 10. The token-

bucket limiter knows when the next token replenishes and tells the client truthfully.

Wire the middleware after UseRouting: app.UseRateLimiter\(\);

Step 2b Minimal transcript route for Session 2 \(stub before Exercise 5\) Exercise 5 replaces this with the real **202 Accepted** \+ status URL \+ worker. For **Session 2** you still need a **real HTTP surface** so the concurrency limiter has something to measure.

Add a versioned controller action \(match your API versioning setup from Session 1. For example TranscriptsController under **api/v\{version:apiVersion\}** or a fixed **api/v2/transcripts** if that is how you routed it\) with: \[ApiController\]

\[Route\("api/v2/transcripts"\)\]

**public class** TranscriptsController : ControllerBase \{

\[HttpPost\]

\[EnableRateLimiting\("transcripts"\)\]

**public** IActionResult RequestTranscript\(\[FromBody\] object? \_\)

\{

*// Stub: Exercise 5 swaps this for enqueue \+ 202 \+ Location.* **return** Ok\(\);

\}

\}

Adjust the route template to match your project. The **attribute** **EnableRateLimiting\("transcripts"\)** must be present **before** you run the Step 6 burst.

Step 3 Add a concurrency limiter for the expensive endpoint The transcript endpoint runs a 5–15 second background job per call. Token Bucket limits *how often* you can request but not *how many can run at once*. If 30 requests trigger 30 simultaneous transcript builds, the database connection pool dies. The right tool is a **concurrency limiter**, separately attached to the transcript route:

builder.Services.AddRateLimiter\(options => \{

*// ... GlobalLimiter from Step 2 stays as-is ...*

options.AddConcurrencyLimiter\("transcripts", opt =>

\{

opt.PermitLimit = 5; *// 5 in-flight transcripts maximu*

*m*

opt.QueueLimit = 20; *// queue up to 20 more* opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;

\}\);

\}\);

Then attach the named policy to the transcript controller \(it sits *on top of* the global tier policy both apply\):

\[HttpPost\]

\[EnableRateLimiting\("transcripts"\)\] **public** async Task<IActionResult> RequestTranscript\(...\) \{ ... \}

Step 4 Exempt health checks

A common mistake: rate limit /health and watch your load balancer mark the service unhealthy under load. Disable the limiter for known infrastructure endpoints:

app.MapHealthChecks\("/health/live"\).DisableRateLimiting\(\); app.MapHealthChecks\("/health/ready"\).DisableRateLimiting\(\); \(The health check endpoints themselves are added in Exercise 9. For now, just remember the pattern.\)

Step 5 Apply the search-only token policy where appropriate For routes that need a tighter cap than the global tier \(e.g., a fuzzy search that hits an external index\), keep a named policy and add it explicitly: options.AddTokenBucketLimiter\("search", opt => \{

opt.TokenLimit = 10;

opt.TokensPerPeriod = 5;

opt.ReplenishmentPeriod = TimeSpan.FromSeconds\(10\);

opt.QueueLimit = 2;

\}\);

\[HttpGet\("search"\)\]

\[EnableRateLimiting\("search"\)\]

**public** async Task<IActionResult> SearchCourses\(

\[FromQuery\] string? term, CancellationToken ct\)

\{

var results = await mediator.Send\(**new** SearchCoursesQuery\(term\), ct\);

**return** Ok\(results\);

\}

Step 6 Test all three layers

Run the API. Test the **anonymous** tier first \(no header\): 1..15 | ForEach-Object \{

$r = Invoke-WebRequest https://localhost:5001/api/v2/courses-SkipH ttpErrorCheck

"$\_ : $\($r.StatusCode\) Retry-After=$\($r.Headers.'Retry-After'\)" \}

Now compare the **paid** tier:

1..15 | ForEach-Object \{

$r = Invoke-WebRequest https://localhost:5001/api/v2/courses \`

-Headers @\{ "X-Api-Key" = "tms-paid-001" \} -SkipHttpErrorCheck

"$\_ : $\($r.StatusCode\)"

\}

Test the concurrency limiter on **POST /api/v2/transcripts** \(the Session 2 stub from Step 2b; Exercise 5 upgrades the handler,

**EnableRateLimiting\("transcripts"\)** stays\): 1..30 | ForEach-Object-Parallel \{

Invoke-WebRequest https://localhost:5001/api/v2/transcripts \`

-Method POST-Body '\{"studentId":1\}'-ContentType 'application/

json' \`

-SkipHttpErrorCheck | Out-Null

\} -ThrottleLimit 30

**Expected result:**

 Anonymous: requests 1–10 succeed \(status 200\), 11\+ return 429 with

Retry-After: ~10.

 Paid: all 15 succeed the paid tier has TokenLimit=200.

 Transcripts: at most 5 in flight at any moment \(visible in worker logs\), the

next 20 queue, and the 26th onwards return 429.

 The Retry-After value in 429 responses reflects when the next token will

be available, not a hard-coded constant.

Troubleshooting

Symptom Fix

Rate limiting not Ensure app.UseRateLimiter\(\) runs *after* UseRouting\(\) Symptom Fix

applied and *before* MapControllers\(\) Paid tier still gets 429 Confirm X-Api-Key header value matches a key in quickly ApiKeyResolver.Keys exactly \(case-sensitive\) Retry-After header The OnRejected handler must call always says Lease.TryGetMetadata\(MetadataName.RetryAfter, .. 10

.\) using a static value defeats the limiter

Concurrency limiter Confirm \[EnableRateLimiting\("transcripts"\)\] never queues attribute is present on the action and

AddConcurrencyLimiter\("transcripts", ...\) is registered

/health/live returns Add .DisableRateLimiting\(\) to the health-check 429 under load MapHealthChecks\(...\) call

Checkpoint

Session Integration Challenge: Cache \+ limit, working together

When both exercises are done, run this sequence and confirm every layer cooperates:

*\# Empty the cache by restarting the API once.*

*\# Then:*

*\# 1. Anonymous tier hits the rate limit before the cache warms—bad UX, by design*

1..15 | ForEach-Object \{

$r = Invoke-WebRequest https://localhost:5001/api/v2/courses-SkipH ttpErrorCheck

"anon $\_ : $\($r.StatusCode\)" \}

*\# 2. Paid tier sails through and warms the cache for everyone* 1..15 | ForEach-Object \{

$r = Invoke-WebRequest https://localhost:5001/api/v2/courses \\

-Headers @\{ "X-Api-Key" = "tms-paid-001" \} -SkipHttpErrorCheck

"paid $\_ : $\($r.StatusCode\)" \}

*\# 3. Anonymous tier comes back. Cache is now warm—only retries hit, no DB load.*

1..15 | ForEach-Object \{

$r = Invoke-WebRequest https://localhost:5001/api/v2/courses-SkipH ttpErrorCheck

"anon-warm $\_ : $\($r.StatusCode\)" \}

Open the application log \(or the dotnet run terminal\) and grep the Cache HIT / Cache MISS lines. You should see:

 A burst of MISS at the start while the paid tier warms the cache.

 A long run of HIT for the warm anonymous run.

 Exactly **one** EF Core SQL line for the entire Courses query, the rest came

from cache.

That is the proof of “cache is solving real load,” not a synthetic 50-parallel-curl test.

 

Session 2 Checkpoint

Tick all of these before you leave the room:

☐ 50 concurrent requests on a cold cache produce **1** Cache MISS line and **49**

Cache HIT lines, **and** exactly one EF Core SELECT against Courses.

☐ An anonymous client gets 200 OKs for the first 10 requests in any 10 s

window, then 429 Too Many Requests with application/problem\+json body.

☐ The **same calls** with X-Api-Key: tms-paid-001 succeed, the paid tier is

not throttled at the same rate.

☐ The 429 response carries a Retry-After value derived from the lease

metadata, not a hard-coded constant.

☐ A burst of 30 transcript POSTs respects the **transcripts** concurrency limiter

\(at most **5** in flight; queue then 429s—see Exercise 4 · Step 6 in this handout\). After Exercise 5, confirm the same cap in worker logs, not only HTTP status codes.

☐ After a **course write** that triggers invalidation \(for example **PUT**

**/api/v2/courses/\{id\}** with the id your V2 controller uses, typically **int** per M6\), the next **GET** for the affected catalogue/detail produces a fresh Cache MISS and the response reflects the change immediately.

☐ dotnet test is still green.

Common Errors: Session 2

Symptom Likely cause Quick fix Multiple Cache Used GetAsync \+ Switch to GetOrCreateAsync\(key, MISS state, factory, options, tags, lines under manual SetAsync

concurrent load ct\)—the atomic refetch only exists

in the factory overload

Cache Tag string mismatch Centralise the tag in invalidation does between CacheKeys.CoursesTag and nothing GetOrCreateAsync\(... reference it from both call sites

tags: \[...\]\) and RemoveByTagAsync\(...

\)

Stale shape after DTO changed but Bump SchemaVersion in CacheKeys deploy SchemaVersion was whenever a DTO field is added,

not bumped removed, or renamed

Retry-After: 10 OnRejected returns a Read never changes Lease.TryGetMetadata\(MetadataNa hard-coded string

me.RetryAfter, out var ts\) and stringify \(int\)ts.TotalSeconds

Anonymous tier GlobalLimiter not Register never throttled PartitionedRateLimiter.Create<H registered, or

app.UseRateLimiter\(\) ttpContext, string>\(...\) in missing AddRateLimiter and call

app.UseRateLimiter\(\) after UseRouting\(\)

 

Concurrency \[EnableRateLimiting\( Add the attribute, and confirm "transcripts"\)\] AddConcurrencyLimiter\("transcri limiter never attribute missing on pts", ...\) is registered queues the action

 

Bridge to Session 3

The cache stops the database from melting. The rate limiter stops one bad client from affecting others. Both improve the dashboard’s *steady state*. Tomorrow’s session adds the two patterns the dashboard needs for *long-running and real-**time* work: a transcript that takes 15 seconds without blocking the request thread \(Exercise 5\) and a notification that arrives the moment the transcript is ready \(Exercise 6\). Skim the **Lab Session 3** handout before the lecture; pre-reading saves about an hour at the keyboard.

Module 7 Lab: Session 3: Long Work Without Blocking the Request

**Module** M7: Advanced Web API Development **Session** 3 of 4

**Exercises** 5 \(Background workers \+ status store \+ idempotency\), 6 \(SignalR

groups \+ reconnect\)

 

Welcome to “Don’t Block the Request Thread” The registrar opens the new V2 admin dashboard and clicks **Generate Transcript** for student \#4271. The page locks. Five seconds. Ten. Fifteen. The browser’s “this page is unresponsive” dialog appears. She clicks **Wait** out of habit. At second 22 the response arrives a 200 OK with a PDF link.

She clicks **Generate Transcript** again, this time for student \#4272. Same lock. Same wait. By the end of the morning she has spent thirty minutes staring at a beachball.

Worse: a colleague double-clicked the button at second 14 by mistake. Two transcripts were generated. Two PDFs went out by email. The student got two duplicate transcripts and called support.

**Both problems share a root cause:** the API is doing the work *during* the HTTP request. The fix is the **Async Request-Reply** pattern \(Exercise 5\);POST returns 202 instantly with a status URL, and a BackgroundService does the actual work. The duplicate fix is **idempotency keys**;the second click with the same key returns the original 202 with the original report id, no second worker job. Then there is the polling problem. A 202 returns a status URL;the dashboard now has to poll it every two seconds for fifteen. That works, and it is the safe default, but for users on the page right now the right answer is to **push** the “your transcript is ready” notification the moment it happens. That is Exercise 6, with SignalR’s typed hubs and group dispatch.

**Session 3 builds the production pattern for any work that takes longer than** **a request can wait.** Reports, exports, certificate generation, video transcoding; all the same shape.

Before You Begin: Session 3 sync

Confirm your Session 1 \+ 2 work still passes:

dotnet build

**Primary checks \(Scalar\):** Open **Try it** for **GET /api/v2/courses** \(cache \+ rate-limiter path\). POST **/api/v2/enrollments** with body \{"studentId":1,"courseCode":"CSE-101"\} \(or a valid student/course in your seed\);confirm **202**/**201** behaviour matches your Session 1 CQRS design. Reach Scalar from the same HTTPS origin **dotnet run** prints; try **/scalar/v1**, then **/scalar** if needed.

Optional scripted replay:

curl https://localhost:5001/api/v2/courses

curl-i -X POST https://localhost:5001/api/v2/enrollments \\

-H "Content-Type: application/json" \\

-d '\{"studentId":1,"courseCode":"CSE-101"\}'

If anything fails, fix it before continuing, Session 3’s transcript controller must sit on the same rate-limit and exception-handling plumbing. Install the Session 3 package:

dotnet add TmsApi.Api package Microsoft.AspNetCore.SignalR

 

Exercise 5: Background Workers, Status Store, and Idempotency \(LO 7.4\)

When an admin requests a student transcript, the generation takes 5–15 seconds. The current code returns 202 Accepted but **GET** **/api/v2/transcripts/\{id\}/status** always says "Processing" even after the worker logged “complete.” That is a teaching bug, not a feature. You will fix it: a real status store with a proper state machine, an idempotency-key check so a double-clicked POST does not enqueue twice, and a status endpoint that reflects truth. This is the version that survives an interview.

**Session 2 → 5 bridge:** If you added the **Exercise 4** minimal POST /api/v2/transcripts stub \(200 OK \+ \[EnableRateLimiting\("transcripts"\)\]\), **replace that action** with the full TranscriptsController below, keep **\[EnableRateLimiting\("transcripts"\)\]** on the POST so concurrency limits from Session 2 still apply.

Step 1 Define the contract: request, state, and status Create TmsApi.Application/Transcripts/TranscriptModels.cs: **namespace** TmsApi.Application.Transcripts; **public enum** TranscriptState \{ Queued, Processing, Ready, Failed \}

**public** record TranscriptRequest\(int StudentId, string? ReportId = **null**\) \{

**public** TranscriptRequest WithReportId\(string id\) => **this** with \{ Rep ortId = id \};

\}

**public** record TranscriptStatus\(

string ReportId,

int StudentId,

TranscriptState State,

DateTimeOffset RequestedAt,

DateTimeOffset? StartedAt = **null**,

DateTimeOffset? CompletedAt = **null**,

string? DownloadUrl = **null**,

string? ErrorMessage = **null**\);

The state machine is Queued → Processing → \(Ready | Failed\). There

is no path from Failed back to Queued a failed transcript requires a new

POST with a fresh report id. That keeps the status endpoint honest: a

200 OK transcript stays 200 OK, not “we’ll try again silently.” Step 2 Build a status store with a state machine In-memory for the lab; the same interface points at Redis/SQL/DynamoDB for production. Create

TmsApi.Infrastructure/Transcripts/ITranscriptStatusStore.cs: **using** TmsApi.Application.Transcripts; **namespace** TmsApi.Infrastructure.Transcripts; **public interface** ITranscriptStatusStore \{

Task<TranscriptStatus> CreateAsync\(string reportId, int studentId, CancellationToken ct\);

Task MarkProcessingAsync\(string reportId, CancellationToken ct\);

Task MarkReadyAsync\(string reportId, string downloadUrl, Cancellati onToken ct\);

Task MarkFailedAsync\(string reportId, string error, CancellationTok en ct\);

Task<TranscriptStatus?> GetAsync\(string reportId, CancellationToken

ct\);

*// Idempotency*

Task<string?> GetReportIdForIdempotencyKeyAsync\(string idempotencyK ey, CancellationToken ct\);

Task LinkIdempotencyKeyAsync\(string idempotencyKey, string reportId, CancellationToken ct\);

\}

Create the in-memory implementation

TmsApi.Infrastructure/Transcripts/InMemoryTranscriptStatusStore.cs: **using** System.Collections.Concurrent; **using** TmsApi.Application.Transcripts; **namespace** TmsApi.Infrastructure.Transcripts; **public class** InMemoryTranscriptStatusStore : ITranscriptStatusStore \{

**private readonly** ConcurrentDictionary<string, TranscriptStatus> \_by ReportId = **new**\(\);

**private readonly** ConcurrentDictionary<string, string> \_idempotencyT oReportId = **new**\(\);

**public** Task<TranscriptStatus> CreateAsync\(string reportId, int stud entId, CancellationToken ct\)

\{

var status = **new** TranscriptStatus\(

reportId, studentId, TranscriptState.Queued, RequestedAt: DateTimeOffset.UtcNow\);

\_byReportId\[reportId\] = status; **return** Task.FromResult\(status\);

\}

**public** Task MarkProcessingAsync\(string reportId, CancellationToken ct\) =>

Transition\(reportId, current => current with \{

State = TranscriptState.Processing, StartedAt = DateTimeOffset.UtcNow

\}, allowedFrom: TranscriptState.Queued\);

**public** Task MarkReadyAsync\(string reportId, string downloadUrl, Can cellationToken ct\) =>

Transition\(reportId, current => current with \{

State = TranscriptState.Ready,

CompletedAt = DateTimeOffset.UtcNow, DownloadUrl = downloadUrl

\}, allowedFrom: TranscriptState.Processing\);

**public** Task MarkFailedAsync\(string reportId, string error, Cancella tionToken ct\) =>

Transition\(reportId, current => current with \{

State = TranscriptState.Failed, CompletedAt = DateTimeOffset.UtcNow, ErrorMessage = error

\}, allowedFrom: TranscriptState.Processing\);

**public** Task<TranscriptStatus?> GetAsync\(string reportId, Cancellati onToken ct\) =>

Task.FromResult\(\_byReportId.TryGetValue\(reportId, **out** var s\) ?

s : **null**\);

**public** Task<string?> GetReportIdForIdempotencyKeyAsync\(string key, CancellationToken ct\) =>

Task.FromResult\(\_idempotencyToReportId.TryGetValue\(key, **out** var

id\) ? id : **null**\);

**public** Task LinkIdempotencyKeyAsync\(string key, string reportId, Ca ncellationToken ct\)

\{

\_idempotencyToReportId.TryAdd\(key, reportId\); **return** Task.CompletedTask;

\}

**private** Task Transition\(string reportId, Func<TranscriptStatus, Tra nscriptStatus> change, TranscriptState allowedFrom\)

\{

**if** \(\!\_byReportId.TryGetValue\(reportId, **out** var current\)\)

**throw new** InvalidOperationException\($"Unknown report id \{re

portId\}."\);

**if** \(current.State \!= allowedFrom\)

**throw new** InvalidOperationException\(

$"Cannot move \{reportId\} from \{current.State\} via this

transition \(expected \{allowedFrom\}\)."\);

\_byReportId\[reportId\] = change\(current\); **return** Task.CompletedTask;

\}

\}

A ConcurrentDictionary is enough for a single-process lab. Production

swaps the implementation for Redis \(TTL \+ atomic ops\) or a SQL

transcript\_status table with an index on report\_id. The interface

stays identical the controller and worker do not care. Register it as a singleton in Program.cs: builder.Services.AddSingleton<ITranscriptStatusStore, InMemoryTranscrip tStatusStore>\(\);

Step 3 Create the bounded channel

Open Program.cs:

**using** System.Threading.Channels; builder.Services.AddSingleton\(Channel.CreateBounded<TranscriptRequest>\(

**new** BoundedChannelOptions\(100\)

\{

FullMode = BoundedChannelFullMode.Wait

\}\)\);

Step 4 Create the background worker that updates the store Because **TmsApi.Infrastructure** is a class library project \(**classlib**\), it does not implicitly reference ASP.NET Core framework abstractions like **BackgroundService**. Add **Microsoft.Extensions.Hosting.Abstractions** to the Infrastructure project before creating the worker:

**dotnet add TmsApi.Infrastructure package Microsoft.Extensions.Hosting.Abstractions** Create TmsApi.Infrastructure/Workers/TranscriptWorker.cs: **using** System.Threading.Channels; **using** TmsApi.Application.Transcripts; **using** TmsApi.Infrastructure.Transcripts; **namespace** TmsApi.Infrastructure.Workers; **public class** TranscriptWorker\(

Channel<TranscriptRequest> channel,

IServiceScopeFactory scopeFactory,

ITranscriptStatusStore statusStore,

ILogger<TranscriptWorker> logger\)

: BackgroundService

\{

**protected override** async Task ExecuteAsync\(CancellationToken ct\)

\{

logger.LogInformation\("Transcript worker started."\);

await **foreach** \(var request **in** channel.Reader.ReadAllAsync\(ct\)\) \{

var reportId = request.ReportId

?? **throw new** InvalidOperationException\("ReportId must b

e set before queueing."\);

**try**

\{

await statusStore.MarkProcessingAsync\(reportId, ct\); logger.LogInformation\(

"Generating transcript \{ReportId\} for student \{Stud

entId\}",

reportId, request.StudentId\);

**using** var scope = scopeFactory.CreateScope\(\); *// Real production: pull the EF context, render PDF, sa*

*ve to blob storage.*

await Task.Delay\(TimeSpan.FromSeconds\(5\), ct\); var downloadUrl = $"/api/v2/transcripts/\{reportId\}/down

load";

await statusStore.MarkReadyAsync\(reportId, downloadUrl,

ct\);

logger.LogInformation\("Transcript ready: \{ReportId\}", r

eportId\);

\}

**catch** \(OperationCanceledException\) when \(ct.IsCancellationR

equested\)

\{

logger.LogWarning\("Worker shutdown transcript \{ReportI

d\} did not complete", reportId\);

**throw**;

\}

**catch** \(Exception ex\) \{

logger.LogError\(ex, "Failed to generate transcript \{Rep

ortId\}", reportId\);

await statusStore.MarkFailedAsync\(reportId, ex.Message,

CancellationToken.None\);

\}

\}

\}

\}

Register:

builder.Services.AddHostedService<TranscriptWorker>\(\); Step 5 Controller with idempotency-key handling Create TmsApi.Api/Controllers/V2/TranscriptsController.cs: **using** System.Threading.Channels; **using** Asp.Versioning;

**using** Microsoft.AspNetCore.Mvc; **using** Microsoft.AspNetCore.RateLimiting; **using** TmsApi.Application.Transcripts; **using** TmsApi.Infrastructure.Transcripts; **namespace** TmsApi.Api.Controllers.V2;

\[ApiController\]

\[Route\("api/v2/transcripts"\)\]

\[ApiVersion\("2.0"\)\]

**public class** TranscriptsController\(

Channel<TranscriptRequest> channel,

ITranscriptStatusStore statusStore\) : ControllerBase \{

\[HttpPost\]

\[EnableRateLimiting\("transcripts"\)\]

**public** async Task<IActionResult> RequestTranscript\(

TranscriptRequest request, \[FromHeader\(Name = "Idempotency-Key"\)\] string? idempotencyKey, CancellationToken ct\)

\{

**if** \(\!string.IsNullOrWhiteSpace\(idempotencyKey\)\) \{

var existing = await statusStore.GetReportIdForIdempotencyK

eyAsync\(idempotencyKey, ct\);

**if** \(existing **is** not **null**\) \{

var existingStatus = await statusStore.GetAsync\(existin

g, ct\);

**return** Accepted\(

Url.Action\(nameof\(GetStatus\), **new** \{ id = existing

\}\),

existingStatus\);

\}

\}

var reportId = Guid.NewGuid\(\).ToString\("N"\)\[..12\]; var status = await statusStore.CreateAsync\(reportId, request.St

udentId, ct\);

**if** \(\!string.IsNullOrWhiteSpace\(idempotencyKey\)\)

await statusStore.LinkIdempotencyKeyAsync\(idempotencyKey, r

eportId, ct\);

await channel.Writer.WriteAsync\(request.WithReportId\(reportId\),

ct\);

Response.Headers.RetryAfter = "5"; **return** Accepted\(

Url.Action\(nameof\(GetStatus\), **new** \{ id = reportId \}\), status\);

\}

\[HttpGet\("\{id\}/status"\)\]

**public** async Task<IActionResult> GetStatus\(string id, CancellationT oken ct\)

\{

var status = await statusStore.GetAsync\(id, ct\); **return** status **is null**

? NotFound\(**new** ProblemDetails \{

Title = "Transcript not found", Detail = $"No transcript request with id '\{id\}'.", Status = StatusCodes.Status404NotFound

\}\)

: Ok\(status\);

\}

\}

The Idempotency-Key header is the same pattern Stripe popularised: the

*client* picks a unique value \(a UUID is the convention\) and the server

promises that two POSTs with the same key produce the same logical

effect. A double-clicked enrollment, a retry after a flaky network, or a

duplicate webhook all become safe the second call returns the original

202 with the original report id, no second worker job. The Retry-After:

5 header gives the client a polite hint about polling cadence \(transcripts

take ~5 seconds in this lab\).

Step 6 Run the full flow

dotnet run--project TmsApi.Api Issue the first request with an idempotency key:

curl-i -X POST https://localhost:5001/api/v2/transcripts \\

-H "Content-Type: application/json" \\

-H "Idempotency-Key: 11111111-2222-3333-4444-555555555555" \\

-d '\{"studentId": 1\}'

Repeat the **exact same request** within a couple of seconds. You should receive 202 with the **same** reportId and a status of Queued or Processing no new worker job kicked off. Then poll:

curl https://localhost:5001/api/v2/transcripts/<reportId>/status **Expected result over time:**

Time after POST state startedAt completedAt downloadUrl 0 s Queued null null null ~0.1 s Processing set null null ~5 s Ready /api/v2/.../downl set set

oad

A second POST with the same Idempotency-Key:

 Returns 202 with the **same** reportId.

 Does *not* trigger a second worker run \(logs show only one “Generating

transcript …” line for that id\).

Querying a non-existent id returns 404 with ProblemDetails. Step 7 A note on durability

InMemoryTranscriptStatusStore is single-process. If the API restarts, in-flight transcripts and idempotency keys vanish. Production options, in increasing weight:

1. **Redis** with TTL on the idempotency-key entries \(24h is the conventional

Stripe default\).

2. **A SQL** **transcript\_status** **table** read by the worker on startup survives

restarts, supports multiple workers behind a leader-election lock.

3. **A real broker** Azure Service Bus, RabbitMQ, AWS SQS when the worker

needs to scale to a separate process. The Channel-based pattern in this lab maps 1:1 to a broker queue; the controller’s WriteAsync becomes a SendMessageAsync and nothing else changes.

You do not need to swap implementations in this lab. You do need to be able to explain, in interview, which option you would choose for which scale. Checkpoint

POST /api/v2/transcripts returns 202 immediately. The status endpoint reflects truth Queued, then Processing, then Ready with a real downloadUrl and timestamps. A double POST with the same Idempotency-Key is safely deduplicated. The state machine refuses illegal transitions. Failures are recorded as Failed with an error message rather than disappearing into logs. The pattern lifts cleanly to Redis or a message broker without changing the controller.

 

Exercise 6: SignalR with Groups, Reconnect, and Backplane Awareness \(LO 7.5\)

When a student’s transcript is ready, their dashboard should update without

polling. The naive answer “broadcast to everyone, the client filters” leaks information and burns bandwidth. The interview-correct answer uses **groups**, the **strongly-typed hub**, and **automatic reconnection** on the client. Authorization will land in M12; until then, students join their own group on connect using the student id they pass on the query string. You will build that, plus understand what you would change to scale to two pods.

Step 1 Define the hub client interface

Create TmsApi.Application/Hubs/ITmsHubClient.cs: **namespace** TmsApi.Application.Hubs; **public interface** ITmsHubClient

\{

Task ReceiveTranscriptReady\(string reportId, string downloadUrl\);

Task ReceiveCourseUpdate\(string courseCode, string message\);

Task ReceiveGradePosted\(string courseCode, int studentId, decimal g rade\);

\}

Step 2 Create the hub with student auto-join Create TmsApi.Api/Hubs/TmsHub.cs. The hub auto-joins the student to their own group on connect \(using studentId from the connection query string\), and exposes explicit join/leave for course-level groups:

**using** Microsoft.AspNetCore.SignalR; **using** TmsApi.Application.Hubs;

**namespace** TmsApi.Api.Hubs;

**public class** TmsHub : Hub<ITmsHubClient> \{

**public override** async Task OnConnectedAsync\(\)

\{

var studentId = Context.GetHttpContext\(\)?.Request.Query\["studen

tId"\].ToString\(\);

**if** \(\!string.IsNullOrWhiteSpace\(studentId\)\) \{

await Groups.AddToGroupAsync\(Context.ConnectionId, GroupNam

es.Student\(studentId\)\);

\}

await **base**.OnConnectedAsync\(\);

\}

**public** async Task JoinCourseGroup\(string courseCode\)

\{

await Groups.AddToGroupAsync\(Context.ConnectionId, GroupNames.C

ourse\(courseCode\)\);

\}

**public** async Task LeaveCourseGroup\(string courseCode\)

\{

await Groups.RemoveFromGroupAsync\(Context.ConnectionId, GroupNa

mes.Course\(courseCode\)\);

\}

**public override** async Task OnDisconnectedAsync\(Exception? exception\)

\{

*// SignalR removes the connection from all groups automatically.* await **base**.OnDisconnectedAsync\(exception\);

\}

\}

**public static class** GroupNames

\{

**public static** string Student\(string studentId\) => $"student-\{studen tId\}";

**public static** string Course\(string courseCode\) => $"course-\{courseC ode\}";

\}

Why a query-string studentId rather than Clients.User\(...\)?

Clients.User requires SignalR to identify the user via IUserIdProvider,

which in practice means JWT auth which the curriculum lands in **M12**.

Until then, a per-student group is functionally identical for this lab and

gives learners the production-correct mental model: **never broadcast**

**to all, always send to a group**. The day M12 wires JWT, swap auto-join

from studentId query to Context.UserIdentifier and the rest of the

code does not change.

Centralising group names in a static class \(GroupNames.Student\(...\)\) is

the production discipline. The day someone typos student-as studnet-,

this lab catches it in code review instead of in production silence.

 

Step 3 Register SignalR

In Program.cs:

builder.Services.AddSignalR\(\);

*// After app.Build\(\)*

app.MapHub<TmsHub>\("/hubs/tms"\);

Step 4 Create the notification abstraction

TranscriptWorker lives in the Infrastructure layer. TmsHub lives in the Api layer. In Clean Architecture, Infrastructure cannot reference Api — dependencies point inward. The fix is a notification interface in Application that the worker calls, with the SignalR implementation living in Api where TmsHub is visible. The worker never learns that SignalR exists.

Create

TmsApi.Application/Notifications/ITranscriptNotificationService.cs: **namespace** TmsApi.Application.Notifications; **public interface** ITranscriptNotificationService \{

Task NotifyTranscriptReadyAsync\(int studentId, string reportId, str ing downloadUrl\);

\}

Create TmsApi.Api/Notifications/SignalRTranscriptNotificationService.cs: **using** Microsoft.AspNetCore.SignalR; **using** TmsApi.Api.Hubs;

**using** TmsApi.Application.Hubs;

**using** TmsApi.Application.Notifications;

**namespace** TmsApi.Api.Notifications; **public class** SignalRTranscriptNotificationService\(IHubContext<TmsHub, I TmsHubClient> hubContext\)

: ITranscriptNotificationService \{

**public** async Task NotifyTranscriptReadyAsync\(int studentId, string reportId, string downloadUrl\)

\{

await hubContext.Clients

.Group\(GroupNames.Student\(studentId.ToString\(\)\)\) .ReceiveTranscriptReady\(reportId, downloadUrl\);

\}

\}

Register the notification service in Program.cs \(alongside the AddSignalR call from Step 3\):

builder.Services.AddSingleton<ITranscriptNotificationService, SignalRTr anscriptNotificationService>\(\);

Why AddSingleton? IHubContext<TmsHub, ITmsHubClient> is itself a

singleton, the DI container hands out the same hub context for the

lifetime of the app. The notification service holds no mutable state, so

singleton is the natural lifetime.

Step 5 Update the worker to notify through the abstraction Update TranscriptWorker to inject ITranscriptNotificationService instead of IHubContext. Replace your worker class from Exercise 5: **public class** TranscriptWorker\(

Channel<TranscriptRequest> channel,

ITranscriptStatusStore statusStore,

ITranscriptNotificationService notificationService,

ILogger<TranscriptWorker> logger\)

: BackgroundService

\{

**protected override** async Task ExecuteAsync\(CancellationToken ct\)

\{

logger.LogInformation\("Transcript worker started."\); await **foreach** \(var request **in** channel.Reader.ReadAllAsync\(ct\)\) \{

var reportId = request.ReportId\!; **try**

\{

await statusStore.MarkProcessingAsync\(reportId, ct\); await Task.Delay\(TimeSpan.FromSeconds\(5\), ct\);

var downloadUrl = $"/api/v2/transcripts/\{reportId\}/down

load";

await statusStore.MarkReadyAsync\(reportId, downloadUrl,

ct\);

await notificationService.NotifyTranscriptReadyAsync\(

request.StudentId, reportId, downloadUrl\);

logger.LogInformation\(

"Transcript ready, notification sent: \{ReportId\} fo

r student \{StudentId\}",

reportId, request.StudentId\);

\}

**catch** \(Exception ex\) \{

logger.LogError\(ex, "Transcript generation failed: \{Rep

ortId\}", reportId\);

await statusStore.MarkFailedAsync\(reportId, ex.Message,

CancellationToken.None\);

\}

\}

\}

\}

Add the using to the top of TranscriptWorker.cs: **using** TmsApi.Application.Notifications;

Step 6 Test with browser DevTools \(with auto-reconnect\) Open the API in your browser \(the Scalar docs page is fine\). Open DevTools \(F12\) → Console. Run:

**const** signalR = **await import**\("https://cdn.jsdelivr.net/npm/@microsoft

/signalr@8.0.0/\+esm"\);

**const** connection = **new** signalR.HubConnectionBuilder\(\)

.withUrl\("/hubs/tms?studentId=1"\)

.withAutomaticReconnect\(\[0, 2000, 10000, 30000\]\)

.configureLogging\(signalR.LogLevel.Information\)

.build\(\);

connection.onreconnecting\(\(err\) **=>**

console.warn\("Reconnecting…", err?.message ?? ""\),

\);

connection.onreconnected\(\(id\) **=>**

console.info\("Reconnected with connection id", id\),

\);

connection.on\("ReceiveTranscriptReady", \(reportId, url\) **=>**

console.log\(\`Transcript ready: $\{reportId\} at $\{url\}\`\),

\);

**await** connection.start\(\);

console.log\("Connected to TmsHub for student 1"\); withAutomaticReconnect\(\[0, 2000, 10000, 30000\]\) tells the client to retry immediately, then after 2 s, 10 s, and 30 s before giving up. Without it, the **first** time you dotnet run after a code change, every connected client silently dies. With it, learners see “Reconnecting… Reconnected” log lines and then the next transcript notification arrives the experience real users expect. Now, in another terminal, POST a transcript for student 1: curl-X POST https://localhost:5001/api/v2/transcripts \\

-H "Content-Type: application/json" \\

-d '\{"studentId": 1\}'

Within ~5 seconds the browser console logs Transcript ready: <reportId> at /api/v2/transcripts/<reportId>/download. Open a *second* browser tab connected with ?studentId=2. Trigger the transcript for student 1 again. Tab 2 should see **nothing** the message went only to the student-1 group.

**Expected result:**

 The SignalR connection establishes; logs show the connection id.

 A POST for student 1 results in **only** student-1’s tab receiving

ReceiveTranscriptReady.

 Restarting the API briefly causes the client to log “Reconnecting…” then

“Reconnected” and the next transcript notification arrives normally.

Step 7 Production: backplane and authorization \(one paragraph each\) When you scale to **two API pods** behind a load balancer, the moment student 1 connects to pod A but the worker on pod B finishes their transcript, the message goes nowhere pod B doesn’t know about pod A’s groups. The fix is a **backplane**: every pod publishes group sends to a shared transport, every pod subscribes. Two production options:

*// Option A: Redis backplane \(self-hosted\)*

builder.Services.AddSignalR\(\).AddStackExchangeRedis\(

builder.Configuration.GetConnectionString\("Redis"\)\!,

options => options.Configuration.ChannelPrefix = "tms-signalr"\); *// Option B: Azure SignalR Service \(managed; recommended at scale\)* builder.Services.AddSignalR\(\).AddAzureSignalR\(

builder.Configuration.GetConnectionString\("AzureSignalR"\)\); You do not enable a backplane in this lab single-pod is fine for learning. You should be able to explain *when* you would \(the moment the deployment goes from one replica to two\).

For **authorization**, the pattern in M12 will be: \[Authorize\] *// require JWT for hub connection* **public class** TmsHub : Hub<ITmsHubClient> \{ ... \} …and Context.UserIdentifier replaces the studentId query string in the auto-join. The hub method names and group naming convention stay identical. Checkpoint

Session Integration Challenge — Two students, one transcript

When both exercises are done, this scenario should “just work”: *\# Tab 1, in browser DevTools, connect as student 1: \# \(paste the client snippet from Exercise 6 · Step 5 in this handout\) \# Tab 2, in browser DevTools, connect as student 2.*

*\# Now: from a terminal, generate a transcript for student 1* $key = \[guid\]::NewGuid\(\)

Invoke-WebRequest-Method POST https://localhost:5001/api/v2/transcript s \\

-Headers @\{ "Idempotency-Key" = $key \} \\

-Body '\{"studentId": 1\}'-ContentType 'application/json' *\# Repeat the exact same call within a couple of seconds* Invoke-WebRequest-Method POST https://localhost:5001/api/v2/transcript s \\

-Headers @\{ "Idempotency-Key" = $key \} \\

-Body '\{"studentId": 1\}'-ContentType 'application/json' Expected behaviour:

 **First POST** returns 202 with a fresh reportId and status Queued.

 **Second POST** within seconds returns 202 with the **same** reportId. Worker

logs show only **one** “Generating transcript …” line.

 Status endpoint progression for that reportId: Queued \(instant\) →

Processing \(within ~100 ms\) → Ready \(around 5 s\) with startedAt, completedAt, and downloadUrl.

 **Tab 1** logs Transcript ready: <reportId> at

/api/v2/transcripts/<reportId>/download exactly **once**.

 **Tab 2** logs nothing—the message went to student-1, not student-2.

 Restart the API. Both tabs log Reconnecting… → Reconnected. Repeat the

POST; tab 1 receives the notification again.

That sequence is the entire long-running-async-with-realtime pattern, end to end. If any step is missing, you have a wiring gap somewhere in Session 3.

 

Session 3 Checkpoint

Tick all of these before you leave the room:

☐ POST /api/v2/transcripts returns 202 immediately. The Location header

points to the status URL. The body is the Queued status with requestedAt.

☐ GET /api/v2/transcripts/\{id\}/status reflects truth: Queued →

Processing → Ready, with timestamps populated correctly and a real downloadUrl on Ready.

☐ A second POST with the same Idempotency-Key returns the **same**

reportId. Worker logs show one—not two—“Generating transcript …” lines.

☐ The state machine refuses illegal transitions. \(Sanity check: try forcing a

transition from Ready back to Queued in a unit test and confirm it throws.\)

☐ A SignalR client connected ?studentId=1 receives

ReceiveTranscriptReady after a transcript for student 1 completes.

☐ A SignalR client connected ?studentId=2 receives **nothing** when student

1’s transcript completes.

☐ Both clients survive a dotnet run restart via withAutomaticReconnect.

☐ Clients.All is **not** used anywhere in the codebase.

☐ dotnet test is still green.



Module 8 Lab Session 1: Shell and Signals **Module** M8 Angular 22 Fundamentals **Session** 1

 

Before you begin

 Confirm **Node 20\+** and the **Angular CLI** install.

 

Checkpoint Tier 1 evidence path

When you finish Excercise 1 below, you should be able to show:

 A working **ng serve** loop on tms-client.

 **app.config.ts** configured for the programme baseline \(including zone-

friendly change detection as specified in the steps\).

 Dashboard UI driven by **signal\(\)** and **computed\(\)** clicking or interacting

updates the screen without stale template reads.

You are building the **TMS client** the browser application that students, instructors, and admins will actually use. Every Excercise in this workbook adds a layer to the same project. By the end, you will have a working Angular application that talks to the .NET API you built in M6 and M7.

**Ground rules for this workbook:**

 Every component is standalone. There is no app.module.ts.

 Use input\(\), output\(\), signal\(\), computed\(\). Not the decorator versions.

 Use @if and @for. Not \*ngIf or \*ngFor.

 Use Reactive Forms \(FormGroup\). Not \[\(ngModel\)\] on complex forms.

 Work in short cycles: write code, save, check the browser, fix, repeat. Do

not write 50 lines before checking if it works.

 

Shared Type \(Create Once, Reuse Everywhere\) Before starting Excercise 1, create the file src/app/models/course.model.ts: */\*\**

*\* List row from the TMS API — mirrors \`CourseResponseDto\` on \`GET /api*

*/courses\`.*

*\* ASP.NET Core defaults to camelCase JSON \(\`id\`, \`maxCapacity\`, …\).*

*\*/*

**export interface** Course \{

id: number;

code: string;

title: string;

maxCapacity: number;

enrollmentCount: number;

\}

*/\*\* Envelope for \`GET /api/courses\` — TMS API contract list shape \(\`Pag edResponse***<T>***\`\). \*/*

**export interface** PagedResponse<T> \{

items: T\[\];

totalCount: number;

page: number;

pageSize: number;

totalPages: number;

hasPrevious: boolean;

hasNext: boolean;

\}

*/\*\* One link from \`CourseDetailDto.Links\` on \`GET /api/courses/\{id\}\`. \* /*

**export interface** CourseLink \{

href: string;

rel: string;

method: string;

\}

*/\*\* Detail payload — mirrors \`CourseDetailDto\` \(list rows do not includ e \`links\`\). \*/*

**export interface** CourseDetail **extends** Course \{

links: **readonly** CourseLink\[\];

\}

You will use Course in every Excercise; PagedResponse, CourseDetail, and CourseLink matter from Excercise 6 onward \(and any time you call getById\).

Excercise 1 Setting Up the Project and Your First Signals Liya is a student at CoTBE. She opens the TMS in her browser expecting to see her dashboard: her name, how many credits she has earned, and whether she is eligible for graduation. Right now, there is nothing. You are going to build that dashboard.

Step 1: Create the Angular Project

Open a terminal. Navigate to where you want the project to live. Here is the command you will run but before you type it, understand what each piece means: ng new tms-client--style=scss--ssr=false--standalone --routing Flag What It Does Why We Use It tms-client The name of the This is our Training Management

project folder that System frontend gets created

--style=scss Sets the SCSS lets you nest styles and use

stylesheet format variables cleaner than raw CSS for a

to SCSS instead real project of plain CSS

--ssr=false Disables Server- SSR is for public websites that need

Side Rendering search engine indexing. The TMS is a

login-protected app we do not need it,

and it adds complexity we do not want

right now

--standalone Generates the Modern Angular uses standalone

project without components by default. This flag the legacy ensures the CLI does not generate

NgModule app.module.ts system

--routing Sets up an We need URL routing from the start

app.routes.ts \(/dashboard, /courses/:id, etc.\) file and <router-outlet>

Now run the command. The CLI may ask a few follow-up questions accept the defaults. When it finishes, move into the project:

cd tms-client

Code .

\]Step 2: Configure the Application Shell Open src/app/app.config.ts. The CLI generated some of this already, but you need to add a few things. Here is the complete file replace what is there with this: **import** \{ ApplicationConfig, provideZonelessChangeDetection \} **from** "@ang ular/core";

**import** \{ provideRouter, withComponentInputBinding \} **from** "@angular/rout er";

**import** \{ provideHttpClient \} **from** "@angular/common/http"; **import** \{ routes \} **from** "./app.routes"; **export const** appConfig: ApplicationConfig = \{

providers: \[

provideZonelessChangeDetection\(\),

provideRouter\(routes, withComponentInputBinding\(\)\),

provideHttpClient\(\),

\],

\};

This file is the entry point for Angular’s global configuration. Each provideX\(\) function registers a capability:

Provider What It Registers What Breaks Without It provideZone Controls when Angular checks the Angular cannot detect ChangeDetec screen for updates. The any changes the tion\(...\) eventCoalescing: true option batches screen never updates

multiple rapid events into a single

check.

 

er\(routes, withComponentInputBinding\(\) part is a shows a blank page withCompone provideRout Connects URLs to components. The Navigating to any URL ntInputBind shortcut we will use in Excercise 4 it lets ing\(\)\) URL parameters flow directly into

component inputs.

provideHttp Gives Angular the ability to make HTTP Any service using Client\(\) requests to your .NET API HttpClient crashes

with

NullInjectorError

We are adding provideHttpClient\(\) now even though you will not use it until Excercise 6. It costs nothing to add it early, and forgetting it later produces a confusing error message.

 

Step 3: Generate the Dashboard Component

ng generate component features/student-dashboard --type=component This creates a folder at src/app/features/student-dashboard/ with four files: File Purpose student-dashboard.component.ts The TypeScript class your

component’s logic and data

student-dashboard.component.html The HTML template what the user

sees

student-dashboard.component.scss The styles how it looks student-dashboard.component.spec.ts The test file you will use this in M11

**Architecture Note: File Suffix Conventions in Modern Angular** Modern Angular CLI tools default to suffix-less file generation \(e.g., student-dashboard.ts instead of student-dashboard.component.ts\). In this Training Management System \(TMS\) codebase, we explicitly

retain .component.ts, .service.ts, and .model.ts type suffixes to prevent naming collisions when colocating domain features. If your CLI environment defaults to suffix-less generation, pass--type=component when generating \(ng generate component features/student-dashboard --type=component \) or add "type": "component" under @schematics/angular:component in your angular.json.

 

Step 4: Build the Signal State

Open student-dashboard.component.ts. Replace the generated content with the code below. Read the comments every line is explained: *// These are the Angular functions we need. signal\(\) and computed\(\) com e from Angular's core.*

**import** \{ Component, signal, computed \} **from** "@angular/core"; *// The @Component decorator tells Angular: "This class is a visual comp onent."*

*// It is metadata it describes how this class connects to the HTML tem plate.*

@Component\(\{

selector: "app-student-dashboard", *// The HTML tag name: <app-student*

*-dashboard />*

standalone: **true**, *// This component manages its own imports \(no NgMod*

*ule\)*

templateUrl: "./student-dashboard.component.html", *// Points to the H*

*TML file*

styleUrl: "./student-dashboard.component.scss", *// Points to the styl*

*es file*

\}\)

**export class** StudentDashboardComponent \{

*// signal\('Liya Kebede'\) creates a reactive variable. Angular watches*

*it.*

*// When its value changes, Angular automatically updates the part of*

*the screen that displays it.*

studentName = signal\("Liya Kebede"\);

earnedCredits = signal\(45\);

*// computed\(\) creates a read-only signal that derives its value from*

*other signals.*

*// It recalculates automatically whenever earnedCredits\(\) changes no*

*manual refresh.*

graduationStatus = computed\(\(\) **=>**

**this** .earnedCredits\(\) >= 120 ? "Eligible for Graduation" : "In Progr ess",

\);

*// A regular method. When called, it updates the earnedCredits signal.*

*// The .update\(\) method receives the current value \(c\) and returns th*

*e new value \(c \+ 3\).*

registerForClass\(\) \{

**this** .earnedCredits.update\(\(c\) **=>** c \+ 3\);

\}

\}

Step 5: Build the Template

Open student-dashboard.component.html. Delete whatever the CLI generated and paste this:

<**div** class="dashboard">

<**h1**>Welcome, \{\{ studentName\(\) \}\}</**h1**>

<**p**>Credits Earned: \{\{ earnedCredits\(\) \}\}</**p**>

<**p**>Graduation Status: \{\{ graduationStatus\(\) \}\}</**p**>

<**button** \(click\)="registerForClass\(\)">

Register for a Class \(\+3 credits\)

</**button**>

</**div**>

A few things to understand about this template:

 **\{\{ studentName\(\) \}\}** The double curly braces \(\{\{ \}\}\) are Angular’s way of

displaying a value from the TypeScript class. The parentheses \(\) are required because studentName is a signal you are calling it to get the current value inside.

 **\(click\)="registerForClass\(\)"** The parentheses around click mean this

is an **event binding**. When the user clicks the button, Angular calls the registerForClass\(\) method in your TypeScript class. This is Angular’s

equivalent of onclick in plain HTML, but it is wired directly to your component.

Step 6: Add a Route

Open src/app/app.routes.ts. This file tells Angular which component to display for each URL:

**import** \{ Routes \} **from** "@angular/router"; **export const** routes: Routes = \[

\{

path: "dashboard",

loadComponent: \(\) **=>**

**import** \("./features/student-dashboard/student-dashboard.component

"\).then\(

\(m\) **=>** m.StudentDashboardComponent,

\),

\},

\{ path: "", redirectTo: "dashboard", pathMatch: "full" \},

\];

Let’s break this down:

 **path: 'dashboard'** When the browser URL is /dashboard, load this

component.

 **loadComponent: \(\) => import\(...\).then\(m =>**

**m.StudentDashboardComponent\)** This is called **lazy loading**. Instead of loading the dashboard code when the app starts, Angular loads it only when the user actually visits /dashboard. The .then\(m => m.StudentDashboardComponent\) part extracts the specific class from the imported file. This pattern looks complex, but you will copy it for every route the only things that change are the file path and the component name.

 **\{ path: '', redirectTo: 'dashboard', pathMatch: 'full' \}** If

someone visits the root URL \(/\), automatically send them to /dashboard.

Finally, open src/app/app.component.html. Delete everything the CLI generated \(the Angular welcome page with the logo\). Replace it with just: <**router-outlet** />

The <router-outlet> is a placeholder. It tells Angular: “Render the current route’s component here.” Without it, your routes are configured but there is nowhere to display them you get a blank page.

Step 7: Run It

ng serve

Open http://localhost:4200/dashboard in your browser. **What you should see:** Liya’s name, her credits \(45\), and her graduation status \(In Progress\). Click the button. The credits go to 48, then 51, and so on. The graduation status stays “In Progress” until credits reach 120, then it flips to “Eligible for Graduation.”

Troubleshooting

Problem Likely Cause Fix Blank page at Missing <router-outlet /> Add <router-outlet /> /dashboard in app.component.html

 

\}\} shows \[object \{\{ earnedCredits\(\) \}\} signal Object\] \{\{ earnedCredits Missing parentheses on the Change to

“Cannot find Wrong import path after Check the relative path in module” error generating app.routes.ts matches the

actual file location

Port 4200 already Another process is using it Run ng serve --port 4300 in use or kill the other process

Checkpoint 1

Before moving to Excercise 2, confirm:

☐ ng serve starts without errors

☐ The dashboard renders with dynamic signal data

☐ Clicking the button updates both the credits number and the graduation

status text



Module 8 Lab Session 2: Components, Lists, and Routing Spine

**Module** M8 Angular 22 Fundamentals **Session** 2 of 3

Excercises Excercises **2**, **3**, and **4**

 

Story thread

You move from a single dashboard into composable UI: course cards, lists with honest empty states, and a **real route** to course detail. **Do not skip Lab 4** before Session 3 forms and HTTP Excercises expect a coherent router tree. Excercise 2 Breaking the Monolith: Component Communication

Your dashboard should not contain 500 lines of HTML for every feature. You need a CourseCardComponent that knows how to display one course and tell the parent when the user clicks “Enroll.”

Step 1: Generate the Card Component

ng generate component ui/course-card

Step 2: Define the Component Contract

Open course-card.component.ts:

**import** \{ Component, input, output \} **from** "@angular/core"; **import** \{ Course \} **from** "../../models/course.model"; @Component\(\{

selector: "tms-course-card",

standalone: **true**,

templateUrl: "./course-card.component.html",

styleUrl: "./course-card.component.scss",

\}\)

**export class** CourseCardComponent \{

course = input.required<Course>\(\);

enrollClicked = output<Course>\(\);

\}

Two new concepts here:

 **input.required<Course>\(\)** This declares that the parent component *must*

pass a Course object to this child. The <Course> part is a TypeScript **generic** it tells Angular the type of data expected. If the parent forgets to pass the course, Angular throws a compile-time error, not a runtime crash. Think of it like a required parameter on a C\# method.

 **output<Course>\(\)** This declares that this child component can send a

Course event back up to the parent. The parent listens for this event the same way you listen for a button click. You will see how to wire it in Step 4.

Notice the selector: 'tms-course-card' this means you will use <tms-course-card> as the HTML tag in the parent’s template. You can name this anything, but prefixing with tms-makes it clear this is a TMS component, not a built-in HTML tag.

Step 3: Build the Card Template

Open course-card.component.html: <**div** class="card">

<**h3**>\{\{ course\(\).title \}\} \(\{\{ course\(\).code \}\}\)</**h3**>

<**p**>

Enrolled \{\{ course\(\).enrollmentCount \}\} of \{\{ course\(\).maxCapacity \}\} seats

</**p**>

<**span**

class="badge"

\[class.closed\]="course\(\).enrollmentCount >= course\(\).maxCapacity"

>

\{\{ course\(\).enrollmentCount >= course\(\).maxCapacity ? "Full" : "Acc epting

enrollments" \}\}

</**span**>

<**button**

\(click\)="enrollClicked.emit\(course\(\)\)"

\[disabled\]="course\(\).enrollmentCount >= course\(\).maxCapacity"

>

Enroll

</**button**>

</**div**>

New Angular syntax in this template:

 **course\(\).title** Because course is an input\(\) signal, you call it with \(\) to

get the value, then access .title on the resulting Course object.

 **\[class.closed\]="course\(\).enrollmentCount >= course\(\).maxCapacity"**

The square brackets \[ \] mean this is a **property binding**. When the class is full, Angular adds the CSS class closed. This mirrors the capacity fields on the **TMS API contract** list row \(enrollmentCount, maxCapacity\) that your catalogue API already exposes.

 **\(click\)="enrollClicked.emit\(course\(\)\)"** When the button is clicked,

this calls .emit\(\) on the output you defined, sending the current course object up to the parent.

 **\[disabled\]="course\(\).enrollmentCount >= course\(\).maxCapacity"**

Another property binding. When the course is full, the button becomes disabled \(greyed out, unclickable\).

Step 4: Use the Card in the Dashboard

Go back to student-dashboard.component.ts. You need to do three things: import the card component, add mock course data, and write a handler for the enroll event.

First, add these imports at the top of the file:

**import** \{ CourseCardComponent \} **from** "../../ui/course-card/course-card.c omponent";

**import** \{ Course \} **from** "../../models/course.model"; Then, update the @Component decorator to include the child component. Add CourseCardComponent to the imports array: @Component\(\{

selector: 'app-student-dashboard',

standalone: **true**,

imports: \[CourseCardComponent\], *// This tells Angular: "I use Course*

*CardComponent in my template"*

templateUrl: './student-dashboard.component.html',

styleUrl: './student-dashboard.component.scss'

\}\)

**Why is this needed?** Because standalone components manage their own dependencies. If you use <tms-course-card> in the template but do not add it to imports , Angular does not know what that tag is it silently renders it as text in the DOM. No error. No warning. Just nothing where the card should be. Now add a sample course and a handler to the class body: *// signal<Course | null>\(null\) means: "This signal holds either a Cours e or nothing."*

*// The | null syntax is TypeScript's way of saying a value can be absen t.*

selectedCourse = signal<Course | null>\(**null**\); *// A sample course to display \(we will switch to an array in Excercise 3\)*

sampleCourse: Course = \{

id: 1,

title: "Advanced Java Services",

code: "CSE-101",

maxCapacity: 30,

enrollmentCount: 12,

\};

handleEnroll\(course: Course\) \{

**this**.selectedCourse.set\(course\);

console.log\('Enrollment requested for:', course.title\);

\}

In student-dashboard.component.html, add below the existing dashboard content:

<**h2**>Available Courses</**h2**>

<**tms-course-card**

\[course\]="sampleCourse"

\(enrollClicked\)="handleEnroll\($event\)"

/>

Here is what each part of the HTML means:

 **\[course\]="sampleCourse"** Square brackets = **property binding**. This

passes the sampleCourse object from the parent into the child’s course input. It is like passing a parameter to a function.

 **\(enrollClicked\)="handleEnroll\($event\)"** Parentheses = **event binding**.

When the child component calls enrollClicked.emit\(...\), Angular runs handleEnroll\(...\) in the parent. The $event is a special Angular variable that contains whatever the child emitted in this case, the Course object.

Checkpoint 2

☐ The course card renders inside the dashboard

☐ Clicking “Enroll” logs the course title to the browser console

☐ The “Enroll” button is disabled when the course is full

\(enrollmentCount >= maxCapacity\)

**Note:** selectedCourse updates in memory in this lab, but the template does not show it yet that is intentional. **Excercise 3** adds a visible “Last enrollment request” line so Enroll clicks change the screen, not only the console.

Excercise 3 Loops, Conditionals, and the Empty State You have one card working. Now you need a catalog of courses, and you need to handle the case where there are no courses available. Step 1: Create the Catalog Data

In student-dashboard.component.ts, replace or add: availableCourses = signal<Course\[\]>\(\[

\{

id: 1,

title: "Advanced Java Services",

code: "CSE-101",

maxCapacity: 30,

enrollmentCount: 10,

\},

\{

id: 2,

title: "Angular UI Lab",

code: "CSE-210",

maxCapacity: 25,

enrollmentCount: 25,

\},

\{

id: 3,

title: "Database Design",

code: "CSE-305",

maxCapacity: 20,

enrollmentCount: 18,

\},

\{

id: 4,

title: "API Security Workshop",

code: "CSE-420",

maxCapacity: 40,

enrollmentCount: 15,

\},

\]\);

Step 2: Render the Loop

**Cleanup from Excercise 2 \(required\):** Excercise 2 left a **single** <tms-course-card> bound to sampleCourse under a heading like “Available Courses.” If you leave that in place, you will see **one hardcoded card plus four loop cards** that is not a bug in Angular; it is leftover markup. **Remove** the Excercise 2-only block: the extra <h2> and the single <tms-course-card \[course\]="sampleCourse" ... />. You can delete the sampleCourse property from the class once nothing references it; keep selectedCourse and handleEnroll for the loop’s \(enrollClicked\) bindings.

In the template, after cleanup, use something like:

<**h2**>Course Catalog</**h2**>

@if \(availableCourses\(\).length === 0\) \{

<**div** class="empty-state">

<**p**>No courses are available this term. Check back during registration.

</**p**>

</**div**>

\} @else \{

<**div** class="grid">

@for \(course of availableCourses\(\); track course.id\) \{

<**tms-course-card** \[course\]="course" \(enrollClicked\)="handleEnroll\($eve

nt\)" />

\} @empty \{

<**p**>No results match your search.</**p**>

\}

</**div**>

\} @if \(selectedCourse\(\); as picked\) \{

<**p** class="selection-hint" role="status">

Last enrollment request: <**strong**>\{\{ picked.title \}\}</**strong**> \(\{\{ pick

ed.code

\}\}\)

</**p**>

\}

Use **selectedCourse\(\)** in the template it is a **signal**, so the parentheses call the current value.

**Why this** **@if** **block:** In Excercise 2, handleEnroll only called console.log. Beginners often think the UI is “broken” because nothing on the page changes. This line proves the parent received the event no DevTools required. You can style .selection-hint in the component SCSS \(for example subtle border or background\).

Two things to pay attention to:

1. **track course.id** This is mandatory. It tells Angular which DOM node

belongs to which course. If you omit track, the application will not compile.

2. **@empty** This is a fallback that renders if the array is specifically empty *after*

the loop starts. It is different from the outer @if check. In practice you will usually use one or the other depending on whether you want to show

different messages for “no data loaded yet” versus “data loaded but filtered to zero results.”

Step 3: Test the Empty State

Temporarily set availableCourses to an empty array: signal<Course\[\]>\(\[\]\). The empty state message should appear. Then restore the data. Checkpoint 3

☐ Only the **catalog** cards appear \(no duplicate single sampleCourse card

above the grid\)

☐ Four course cards render in a grid

☐ The full course \(“Angular UI Lab”\) has a disabled Enroll button

☐ Clicking **Enroll** on a course updates the on-page “Last enrollment request”

line \(not only the console\)

☐ Setting the array to empty shows the empty state message

Excercise 4 Routing to a Course Detail Page When Liya clicks on a course title, she should navigate to /courses/1 and see proof that the **route parameter** arrived \(this Excercise is the **navigation spine**\). A rich detail screen \(title, seat counts, HATEOAS links from CourseDetailDto\) would use CourseService.getById\(id\(\)\) or data you already have; that layering is intentionally **out of scope** here so you are not debugging **HTTP and routing** in the same hour.

Step 1: Generate the Detail Component

ng generate component features/course-detail

Step 2: Add the Route

In app.routes.ts, add:

\{

path: 'courses/:id',

loadComponent: \(\) **=> import**\('./features/course-detail/course-detail.c

omponent'\)

.then\(m **=>** m.CourseDetailComponent\) \}

Step 3: Use Input Binding for the Route Parameter In course-detail.component.ts:

**import** \{ Component, input, effect \} **from** "@angular/core";

@Component\(\{

selector: "app-course-detail",

standalone: **true**,

templateUrl: "./course-detail.component.html",

\}\)

**export class** CourseDetailComponent \{

*// This automatically receives the :id from the URL /courses/:id*

*// Because we enabled withComponentInputBinding\(\) in app.config.ts \(S*

*tep 2 of Excercise 1\),*

*// Angular maps the URL parameter ":id" directly to this input.*

*// The name must match exactly: the route says ":id", so the input is*

*called "id".*

id = input.required<string>\(\);

*// The constructor runs when the component is created.*

*// effect\(\) watches any signals read inside it. Every time id\(\) chang*

*es*

*// \(e.g. navigating from /courses/1 to /courses/2\), this code runs ag*

*ain.*

**constructor**\(\) \{

effect\(\(\) **=>** \{

console.log\(\`Loading course detail for ID: $\{**this**.id\(\)\}\`\);

\}\);

\}

\}

In the template:

<**h1**>Course Detail</**h1**>

<**p**>Course ID: \{\{ id\(\) \}\}</**p**>

<**a** routerLink="/dashboard">Back to Dashboard</**a**> The routerLink attribute tells Angular: “When the user clicks this link, navigate to /dashboard without reloading the entire page.” This is different from a normal

HTML <a href="/dashboard"> a normal link would reload the whole app. Angular’s routerLink does a soft navigation, keeping the app state intact. To use routerLink, you must import RouterLink in the component: **import** \{ RouterLink \} **from** '@angular/router';

@Component\(\{

*// ... existing properties*

imports: \[RouterLink\] *// Add this*

\}\)

Step 4: Link from the Course Card

Back in course-card.component.html, wrap the course title in a link that navigates to the detail page:

<**h3**>

<**a** \[routerLink\]="\['/courses', course\(\).id\]"> \{\{ course\(\).title \}\} </

**a**>

\(\{\{ course\(\).code \}\}\)

</**h3**>

The \[routerLink\]="\['/courses', course\(\).id\]" builds a URL dynamically. If course\(\).id is 1, Angular navigates to /courses/1. The square brackets around routerLink mean this is a property binding Angular evaluates the expression, it is not a static string.

You also need to add RouterLink to CourseCardComponent’s imports array, just like you did for the detail component.

Troubleshooting

Problem Cause Fix Clicking the link RouterLink not imported Add RouterLink to the does nothing in the component imports array URL changes but Missing <router-outlet Ensure the outlet exists page is blank /> in app.component.html id\(\) returns Input name does not The route says :id, so the undefined match route parameter input must be called id

name

Checkpoint 4

☐ Clicking a course title navigates to /courses/1 \(or whatever the ID is\)

☐ The detail page displays the correct course ID

☐ The “Back to Dashboard” link works



Module 8 Lab Session 3: Reactive Forms and Live API

**Module** M8 Angular 22 Fundamentals **Session** 3 of 3

 

Excercise 5 The Enrollment Form

Liya wants to enroll. She needs a form that captures her Student ID, the term, and optional backup course choices. The form must validate inputs before they reach the .NET API.

Step 1: Generate the Form Component

ng generate component features/enrollment-form

Step 2: Build the Form Model

This step has the most new concepts in the entire workbook. Read the inline comments carefully:

In enrollment-form.component.ts:

**import** \{ Component, inject, signal \} **from** "@angular/core"; **import** \{

FormBuilder,

FormControl,

Validators,

ReactiveFormsModule,

FormArray,

\} **from** "@angular/forms";

@Component\(\{

selector: "app-enrollment-form",

standalone: **true**,

imports: \[ReactiveFormsModule\], *// Required without this, Angular do*

*es not recognize form directives*

templateUrl: "./enrollment-form.component.html",

\}\)

**export class** EnrollmentFormComponent \{

*// inject\(FormBuilder\) is Angular's way of requesting a service.*

*// It is similar to constructor injection in .NET \(like inject ILogge*

*r in a C\# class\).*

*// The "private" keyword means only this class can access it.*

**private** fb = inject\(FormBuilder\);

*// A signal to track whether the form was submitted \(for showing a su*

*ccess message\)*

submitted = signal\(**false**\);

*// fb.nonNullable.group\(\{...\}\) creates a form object in TypeScript co*

*de.*

*// "nonNullable" ensures that all values are typed as 'string' instea*

*d of 'string | null'*

*// this saves you from writing null-checking code everywhere.*

*//*

*// Each field is defined as: \[defaultValue, validators\]*

*// Validators are rules that the value must pass before the form is c*

*onsidered valid.*

form = **this**.fb.nonNullable.group\(\{

studentId: \[

"",

\[Validators.required, Validators.pattern\("^STU-\[0-9\]\{4\}$"\)\],

\],

*//* *^^ default value is empty string*

*//* *^^ two validators: the field is required AND must*

*match the pattern STU-1234*

courseId: \["", Validators.required\],

term: \["Fall 2026", Validators.required\], *// Pre-filled with a defa ult term*

notes: \[""\], *// No validators this field is optional*

backupCourses: **this**.fb.array<FormControl<string>>\(\[\]\), *// Starts em pty, user adds rows dynamically*

\}\);

*// "get backups\(\)" is a TypeScript property accessor it looks like a*

*variable but runs a function.*

*// This is a shortcut so you can write "this.backups" instead of "thi*

*s.form.controls.backupCourses"*

**get** backups\(\) \{

**return this**.form.controls.backupCourses;

\}

*// Adds a new empty text input to the backup courses array*

addBackup\(\) \{

**this** .backups.push\(

**this** .fb.control\("", \{

nonNullable: **true**,

validators: Validators.required,

\}\),

\);

\}

*// Removes a specific backup course row by its position in the array*

removeBackup\(index: number\) \{

**this** .backups.removeAt\(index\);

\}

submit\(\) \{

**if** \(**this**.form.valid\) \{

*// getRawValue\(\) extracts the full form data as a JSON object. // IMPORTANT: Do NOT use .value here. If any field is disabled, .*

*value silently*

*// drops that field from the object. getRawValue\(\) always include*

*s everything.*

**const** payload = **this**.form.getRawValue\(\); console.log\("Enrollment payload:", payload\); **this** .submitted.set\(**true**\);

\} **else** \{

*// markAllAsTouched\(\) forces Angular to show validation errors on*

*every field.*

*// Without this call, Angular only shows errors on fields the use*

*r has clicked on.*

**this** .form.markAllAsTouched\(\);

\}

\}

\}

Step 3: Build the Form Template

In enrollment-form.component.html: <**h2**>Course Enrollment</**h2**>

@if \(submitted\(\)\) \{

<**div** class="success">

Enrollment submitted. Check the console for the payload.

</**div**>

\} @else \{

*<\!-- \[formGroup\]="form" connects this <form> tag to the TypeScript form*

*object you built above. -->*

*<\!-- \(ngSubmit\)="submit\(\)" calls your submit\(\) method when the user pre sses Enter or clicks the submit button. -->*

<**form** \[formGroup\]="form" \(ngSubmit\)="submit\(\)">

<**label** for="studentId">Student ID</**label**>

*<\!-- formControlName="studentId" links this input to the 'studentId'*

*field in the form group. -->*

*<\!-- Angular keeps the TypeScript value and the on-screen input in sy*

*nc automatically. -->*

<**input**

id="studentId"

formControlName="studentId"

placeholder="e.g. STU-1234"

/>

*<\!-- Show the error ONLY when the user has clicked into and out of th*

*e field \(.touched\) -->*

*<\!-- AND the current value fails validation \(.invalid\). -->*

@if \(form.controls.studentId.touched **&&** form.controls.studentId.inval

id\) \{

<**span** class="error">Enter a valid Student ID \(format: STU-0000\)</**spa**

**n**>

\}

<**label** for="courseId">Course ID</**label**>

<**input**

id="courseId"

formControlName="courseId"

placeholder="e.g. 1 \(TMS course primary key\)"

/>

@if \(form.controls.courseId.touched **&&** form.controls.courseId.invalid\)

\{

<**span** class="error">Course ID is required</**span**>

\}

<**label** for="term">Term</**label**>

<**input** id="term" formControlName="term" />

<**label** for="notes">Notes \(optional\)</**label**>

<**textarea** id="notes" formControlName="notes"></**textarea**>

<**h3**>Backup Courses</**h3**>

*<\!-- $index is a built-in variable inside @for loops the current pos*

*ition \(0, 1, 2...\) -->*

@for \(backup of backups.controls; track $index\) \{

<**div** class="backup-row">

*<\!-- \[formControl\]="backup" connects this input to the specific For mControl object. -->*

*<\!-- This is different from formControlName formControlName looks up by string name, -->*

*<\!-- while \[formControl\] binds directly to the control object in th e array. -->*

<**input**

\[formControl\]="backup"

\[placeholder\]="'Backup course ' \+ \($index \+ 1\)"

/>

*<\!-- type="button" prevents this from submitting the form. -->*

*<\!-- Without type="button", any <button> inside a <form> defaults t o type="submit". -->*

<**button** type="button" \(click\)="removeBackup\($index\)">Remove</**butto n**>

</**div**>

\}

<**button** type="button" \(click\)="addBackup\(\)">Add Backup Course</**butto**

**n**>

<**hr** />

*<\!-- \[disabled\]="form.invalid" disables the button when ANY field fai*

*ls validation. -->*

<**button** type="submit" \[disabled\]="form.invalid">Confirm Enrollment</**b**

**utton**>

</**form**>

\}

Step 4: Route to the Form

Add a route in app.routes.ts:

\{

path: 'enroll',

loadComponent: \(\) **=> import**\('./features/enrollment-form/enrollment-fo

rm.component'\)

.then\(m **=>** m.EnrollmentFormComponent\) \}

Open the form at **http://localhost:4200/enroll**, or add a **routerLink** on the dashboard \(for example an “Enroll” link to /enroll\) so the page is easy to find the workbook adds the route but does not add that link for you. Things That Will Go Wrong \(and How to Fix Them\)

 **Validation messages do not appear:** You are probably

checking .invalid without checking .touched. Angular does not flag pristine \(never-clicked\) fields as errors. Call .markAllAsTouched\(\) on submit.

 **formGroup** **directive not recognized:** You forgot to add

ReactiveFormsModule to the component’s imports array.

 **Trying to use** **\[\(ngModel\)\]** **alongside** **\[formGroup\]****:** This throws an error.

Pick one form strategy per form element. For this form, use only Reactive Forms directives \(formControlName, \[formControl\]\).

Checkpoint 5

☐ The form renders with Student ID, Course ID, Term, and Notes fields

☐ Clicking “Add Backup Course” adds a new input row

☐ Clicking “Remove” removes that specific row

☐ Submitting with an empty Student ID shows the validation error

☐ A valid submission logs the complete payload to the console \(open

DevTools to verify\)

Excercise 6 Connecting to the .NET API

Mock data got you this far. Now you connect to the real backend. Before HTTP: Observable → async result \(read this once\) So far, **signal**, **computed**, **input**, and **output** behaved like **synchronous** values in the component: the template reads them, Angular re-renders when they change. **HttpClient.get\(\)** does **not** return a Course\[\] immediately. It returns an **Observable** a lazy stream that may emit **one value later**, then complete, or emit an **error**. Someone has to **subscribe** to start the request and receive the payload. If you subscribe by hand in a component and forget to tear down, you get the memory-leak.

**rxResource** \(from @angular/core/rxjs-interop\) is Angular’s bridge: you keep the **Observable** inside CourseService, but the component exposes **coursesResource.value\(\)**, **.isLoading\(\)**, and **.error\(\)** as **signals** the template already knows how to use. Same mental model as the rest of the workbook only the **data source** is asynchronous.

**Mental model:** Observable = “a future stream of results.” rxResource = “run that stream when the resource loads, surface outcomes as signals, and clean up when this component goes away.” You do **not** need to master all of RxJS for this lab; you need this **one** pattern.

Step 1: Verify Your API Is Running

Open a separate terminal and start the **same TMS Web API** you built in Module 6 \(and extended in Module 7 if your cohort ran those topics\): cd path/to/your/tms-api

dotnet run

Confirm you get **200** with a JSON body. Send **GET** with **page** and **pageSize** \(for

example https://localhost:5001/api/courses?page=1&pageSize=20\); adjust host/port if your API prints something else.

**Catalogue envelopes \(Angular mapping\) read once, verify once in Scalar:**

 **Never integrate** against a bare root JSON array **\[ \{...\}, \{...\} \]** for

TMS: the catalogue is always wrapped in an object.

 **GET /api/courses** \(M6 spine you still run in many cohorts\) and **GET**

**/api/v1/courses** \(once URL versioning is live\): **items\[\]** carries the rows. Paging totals usually live **alongside** **items** on that same envelope \(**totalCount**, **page**, **pageSize**, **totalPages**, **hasPrevious**, **hasNext**, camelCase JSON\).

 **GET /api/v2/courses** \(the programme M7 hardened catalogue envelope\):

**data\[\]** carries the rows; paging often nests under **meta**, hypermedia paths under **links**. Inspect your real payload, your service must map whatever field actually holds **Course\[\]**.

Every list row exposes **id**, **code**, **title**, **maxCapacity**, **enrollmentCount**. This Excercise 6 scaffold calls **GET /api/courses** and maps **p.items**; if your integration URL is **/api/v2/courses**, change the map to **p.data** \(and extend your TypeScript type to match **meta** / **links** if TypeScript starts complaining\). Use Scalar, Postman, curl, or Invoke-RestMethod. If it does not respond, fix the API before continuing, this Excercise is about the Angular side, not debugging .NET.

Step 2: Create the Course Service

ng g service services/course --type=service

This creates src/app/services/course.service.ts. The CLI generates a minimal skeleton. Replace it with:

**import** \{ Service, inject \} **from** "@angular/core"; **import** \{ HttpClient \} **from** "@angular/common/http"; **import** \{ map \} **from** "rxjs/operators"; **import** \{ Course, CourseDetail, PagedResponse \} **from** "../models/course.m odel";

*// @Service\(\) means Angular creates one instance of this service // and shares it across the entire app. This is the Angular 22 shorthan d replacing legacy @Injectable.*

*// This is similar to AddSingleton<T>\(\) in .NET's dependency injection.* @Service\(\)

**export class** CourseService \{

*// inject\(HttpClient\) requests Angular's HTTP client the same patter*

*n as inject\(FormBuilder\)*

**private** http = inject\(HttpClient\);

**private** baseUrl = "https://localhost:5001/api/courses";

getAll\(page=1, pageSize=50\) \{

*// This URL is GET /api/courses → map items\[\] \(M6 catalogue envelop e\). Never accept a bare root \[...\].*

*// Switch to map\(\(p\) => p.data\) if your base URL is GET /api/v2/cou rses; paging often nests under meta on that envelope \(Step 1\).*

**return this**.http

.get<PagedResponse<Course>>\(**this**.baseUrl, \{

params: \{ page: page.toString\(\), pageSize: pageSize.toString\(\)

\},

\}\)

.pipe\(map\(\(p\) **=>** p.items\)\);

\}

getById\(id: string\) \{

**return this**.http.get<CourseDetail>\(\`$\{**this**.baseUrl\}/$\{id\}\`\);

\}

\}

Step 3: Consume the Service with rxResource Open your **existing** student-dashboard.component.ts from Labs 1–3 do **not** start a second component file. **Merge** the changes below into the class you already have: add **inject** to your @angular/core import if it is not there yet \(alongside Component, signal, computed\), add the imports below, remove the **availableCourses** signal \(and any code that exists only to feed the old mock catalog\), add **coursesResource**, and keep **handleEnroll**, **selectedCourse**, and your @Component imports \(for example CourseCardComponent\) intact. If you paste a second export class StudentDashboardComponent, TypeScript will error edit the one class.

At the top, add these imports \(merge with existing lines, do not duplicate\): **import** \{ rxResource \} **from** "@angular/core/rxjs-interop"; **import** \{ CourseService \} **from** "../../services/course.service"; Now replace the hardcoded availableCourses signal with a live API call. The key change is using rxResource this is Angular 22’s recommended way to safely fetch data from an API, where it is fully stable:

**export class** StudentDashboardComponent \{

*// inject\(CourseService\) requests the service we just created.*

*// Angular finds the singleton instance and gives it to us.*

**private** api = inject\(CourseService\);

studentName = signal\("Liya Kebede"\);

earnedCredits = signal\(45\);

graduationStatus = computed\(\(\) **=>**

**this** .earnedCredits\(\) >= 120 ? "Eligible for Graduation" : "In Progr ess",

\);

*// rxResource wraps the HTTP call into three managed signals:*

*//* *- coursesResource.isLoading\(\) → true while waiting for the serve*

*r response*

*//* *- coursesResource.error\(\)* *→ the error object if the request*

*fails*

*//* *- coursesResource.value\(\)* *→ the Course\[\] array when the requ*

*est succeeds*

*//*

*// It handles subscribing \(starting the request\) and unsubscribing \(c*

*leaning up*

*// if the user navigates away before the response arrives\) automatica*

*lly.*

*// You never write .subscribe\(\) or .unsubscribe\(\) with rxResource.*

coursesResource = rxResource\(\{

stream: \(\) **=> this**.api.getAll\(\),

\}\);

*// ...rest of the component \(handleEnroll, etc.\)*

\}

Step 4: Update the Template

Replace the availableCourses\(\) references with coursesResource: <**h2**>Course Catalog</**h2**>

@if \(coursesResource.isLoading\(\)\) \{

<**div** class="spinner">Fetching courses from the server...</**div**> \} @else if \(coursesResource.error\(\)\) \{

<**div** class="error">

Could not load courses. Make sure your .NET API is running.

</**div**>

\} @else \{

<**div** class="grid">

@for \(course of coursesResource.value\(\)\!; track course.id\) \{

<**tms-course-card** \[course\]="course" \(enrollClicked\)="handleEnroll\($eve

nt\)" />

\} @empty \{

<**p**>No courses are available this term.</**p**>

\}

</**div**>

\}

One new thing here: the **\!** after coursesResource.value\(\)\!. This is TypeScript’s **non-null assertion operator**. It tells TypeScript: “I know this value is not null at this point.” We can safely use it here because the @else block only runs after isLoading\(\) and error\(\) are both false meaning the data has loaded successfully and value\(\) contains the actual array. Step 5: Handle CORS \(You Will Hit This\)

When the Angular app on localhost:4200 tries to call the .NET API on localhost:5001, the browser will block the request with a CORS error. This is the browser’s security policy it has nothing to do with Angular. **The fix is on the .NET side.** In your TMS API Program.cs, ensure you have a CORS policy that allows localhost:4200: builder.Services.AddCors\(options => \{

options.AddPolicy\("AllowAngular", policy =>

policy.WithOrigins\("http://localhost:4200"\)

.AllowAnyHeader\(\) .AllowAnyMethod\(\)\);

\}\);

*// ...*

app.UseCors\("AllowAngular"\);

Step 6: Verify in the Browser

Open http://localhost:4200/dashboard. Open DevTools \(F12\) and go to the Network tab.

You should see a GET request to the URL in your CourseService \(for example https://localhost:5001/api/courses?page=1&pageSize=50\). The response should be **200 OK** with a **wrapped catalogue** \(not a root-level array\): **items\[\]** carries rows on **/api/courses** and **/api/v1/courses**, while **GET /api/v2/courses** on the programme M7 envelope uses **data\[\]** \(often with **meta** / **links**\). Rows expose **id**, **code**, **title**, **maxCapacity**, **enrollmentCount** in camelCase. After mapping in the UI, course cards render with live rows, not the earlier mock array.

 

Checkpoint 6

☐ The Network tab shows a successful GET request to the .NET API

☐ Course cards render with real data \(not the hardcoded mock array\)

☐ The loading spinner appears briefly before data renders

☐ If you stop the .NET API and refresh, the error message appears instead

Module 9 Lab Session 1: Centralized State with SignalStore

Story Thread

It is Enrollment Week at CoTBE. Two thousand students are registering simultaneously, and Dawit is working through the queue from his instructor dashboard.

He opens the **Enrollment List** widget on the left panel and clicks **Approve** next to Liya’s enrollment. The status flips to “Approved” immediately. He then switches to the **Dashboard Summary** widget on the right panel to check remaining pending counts.

The summary still says **42 Pending**. He refreshes the entire page. Now it reads **41 Pending**. What happened? Both widgets called

this.http.get\('/api/enrollments'\).subscribe\(\) independently when they initialized. The Enrollment List updated its local signal\(\) when the approve call completed, but the Dashboard Summary was running on its own copy of the data , a completely separate signal that knew nothing about the approval. This is the core problem of **state drift**: when multiple components independently fetch and manage copies of the same mutable data, they inevitably fall out of sync.

The solution is a **single source of truth**. Instead of each widget holding its own signal, both read from one centralized store. When the store changes, every widget that reads from it updates automatically, no refresh, no extra API calls, no drift.

Prerequisites: Domain Model and API Service Before building the store, you need the enrollment data contract and the HTTP service that talks to the .NET API.

The Enrollment Model

Create src/app/models/enrollment.model.ts: **export interface** Enrollment \{

id: string;

studentId: number;

studentName: string;

courseId: number;

courseName: string;

status: 'Pending' | 'Approved' | 'Rejected';

enrolledAt: string;

\}

The Enrollment Service

Generate the service \(ng generate service services/enrollment\) and implement the API client:

**import** \{ Service, inject \} **from** '@angular/core'; **import** \{ HttpClient \} **from** '@angular/common/http'; **import** \{ Observable \} **from** 'rxjs'; **import** \{ Enrollment \} **from** '../models/enrollment.model'; @Service\(\)

**export class** EnrollmentService \{

**private** http = inject\(HttpClient\);

**private** baseUrl = 'http://localhost:5000/api/enrollments';

getAll\(\): Observable<Enrollment\[\]> \{

**return this**.http.get<Enrollment\[\]>\(**this**.baseUrl\);

\}

approve\(id: string\): Observable<void> \{

**return this**.http.post<void>\(\`$\{**this**.baseUrl\}/$\{id\}/approve\`, \{\}\);

\}

\}

*The base URL uses a relative path \(**/api/enrollments**\). In M10 Session 1, you will* *configure the Angular dev proxy and environment files to route this to your .NET* *API on port 5000. For now, this relative path keeps your service code deployment-**ready from day one.*

Exercise 1: Centralized State with NgRx SignalStore Why Local Signals Drift

In Module 8, you learned that signal\(\) creates a reactive value that components can read and write. But here is the catch: each signal\(\) call creates an **independent, isolated** reactive container. If two components each create their own enrollment signal or each call http.get\(\) separately; they hold separate copies of the data. Changing one copy does not affect the other. This is fine for UI-local state \(a sidebar toggle, a selected tab index\). But for shared mutable data like enrollment records that multiple widgets display and modify, you need a **singleton store ;** one instance in memory, shared by every component that injects it.

Install NgRx SignalStore

Open a terminal in your Angular project and install the package: npm install @ngrx/signals

Build the Enrollment Store

Create src/app/store/enrollment.store.ts. Read the inline comments carefully, each building block solves a specific architectural problem: **import** \{ computed, inject \} **from** '@angular/core'; **import** \{

signalStore,

withComputed,

withMethods,

patchState,

withState,

\} **from** '@ngrx/signals';

**import** \{

withEntities,

setAllEntities,

updateEntity,

\} **from** '@ngrx/signals/entities'; **import** \{ rxMethod \} **from** '@ngrx/signals/rxjs-interop'; **import** \{ pipe, concatMap, tap, catchError, EMPTY \} **from** 'rxjs'; **import** \{ EnrollmentService \} **from** '../services/enrollment.service'; **import** \{ Enrollment \} **from** '../models/enrollment.model';

**export const** EnrollmentStore = signalStore\(

\{ providedIn: 'root' \},

*// withState adds simple properties alongside the entity collection*

withState\(\{ isLoading: **false**, error: null **as** string | null \}\),

*// withEntities creates an O\(1\) ID-indexed dictionary for the enrollm*

*ent collection.*

*// Internally, it stores \{ ids: string\[\], entityMap: Record<string, E*

*nrollment> \}*

*// so lookups and updates by ID are instant — no array scanning.*

withEntities<Enrollment>\(\),

*// withComputed creates read-only derived signals that update automat*

*ically.*

*// pendingCount recalculates every time the entity collection changes.*

withComputed\(\(store\) **=>** \(\{

pendingCount: computed\(

\(\) **=>** store.entities\(\).filter\(e **=>** e.status === 'Pending'\).length

\),

\}\)\),

withMethods\(\(store, api = inject\(EnrollmentService\)\) **=>** \(\{

*// Loading Data*

*// Why concatMap here? Because concatMap processes one emission at a time*

*// in strict order. If something triggers loadEnrollments\(\) twice q uickly,*

*// concatMap waits for the first HTTP response before starting the second.*

*// switchMap would cancel the first request \(data loss risk\).*

*// mergeMap would run both in parallel \(race condition risk\).*

loadEnrollments: rxMethod<void>\(

pipe\(

tap\(\(\) **=>** patchState\(store, \{ isLoading: **true**, error: null \}\)\), concatMap\(\(\) **=>**

api.getAll\(\).pipe\(

tap\(rows **=>** patchState\(store, setAllEntities\(rows\), \{ isLoa

ding: **false** \}\)\),

catchError\(err **=>** \{

patchState\(store, \{ isLoading: **false**, error: err.message

\}\);

**return** EMPTY; *// EMPTY completes silently so the rxMethod*

*pipeline survives*

\}\)

\)

\)

\)

\),

*// Optimistic Approve*

*// Step 1: Instantly flip the status to "Approved" in the store.*

*//* *Every component reading from the store sees the change i*

*mmediately.*

*// Step 2: Send the approval to the server.*

*// Step 3: If the server rejects it, roll back the status to "Pendi ng."*

approveEnrollment: rxMethod<string>\(

pipe\(

tap\(id **=>** \{

*// Optimistic update — the UI reacts before the network round*

*-trip completes*

patchState\(store, updateEntity\(\{ id, changes: \{ status: 'Appr

oved' \} \}\)\);

\}\),

concatMap\(id **=>**

api.approve\(id\).pipe\(

catchError\(err **=>** \{

*// Server said no — restore the previous state* patchState\(store, updateEntity\(\{ id, changes: \{ status: '

Pending' \} \}\)\);

patchState\(store, \{ error: 'Server rejected the approval.

Check enrollment constraints.' \}\);

**return** EMPTY;

\}\)

\)

\)

\)

\),

\}\)\)

\);

Wire the Store into Your Component

Generate the enrollment list component \(ng generate component features/enrollment-list\), then connect it to the store: **import** \{ Component, inject, OnInit \} **from** '@angular/core'; **import** \{ EnrollmentStore \} **from** '../../store/enrollment.store'; @Component\(\{

selector: 'tms-enrollment-list',

standalone: **true**,

templateUrl: './enrollment-list.component.html'

\}\)

**export class** EnrollmentListComponent **implements** OnInit \{

store = inject\(EnrollmentStore\);

ngOnInit\(\) \{

**this** .store.loadEnrollments\(\);

\}

onApprove\(id: string\) \{

**this** .store.approveEnrollment\(id\);

\}

\}

In the template, bind to the store’s entity collection:

@if \(store.isLoading\(\)\) \{

<**p**>Loading enrollments...</**p**>

\}

@for \(enrollment of store.entities\(\); track enrollment.id\) \{

<**div** class="enrollment-card">

<**span**>\{\{ enrollment.studentName \}\} — \{\{ enrollment.courseName \}\}</**s pan**>

<**span** class="status">\{\{ enrollment.status \}\}</**span**>

@if \(enrollment.status === 'Pending'\) \{

<**button** \(click\)="onApprove\(enrollment.id\)">Approve</**button**>

\}

</**div**>

\}

@if \(store.error\(\)\) \{

<**p** class="error">\{\{ store.error\(\) \}\}</**p**>

\}

What You Just Built

To see the store in action, open two components that both read from EnrollmentStore the enrollment list and a dashboard summary widget showing store.pendingCount\(\) . Click **Approve** on Liya’s enrollment in the list. Without navigating away or refreshing, the dashboard widget’s pending count drops by one automatically.

This works because both components inject the same singleton store. The moment patchState fires, the store’s entity dictionary updates, and every component bound to store.entities\(\) or store.pendingCount\(\) re-renders automatically. No manual refresh, no duplicate API calls, no state drift. *Cross-tab sync \(two separate browser windows updating simultaneously\) requires a* *real-time push channel. You will build that in Session 3 using SignalR.* This is the architectural foundation for everything that follows in M9: performance optimization, enterprise grids, defensive RxJS, and real-time sync all build on top of this centralized store.

What’s Next

Keep this project running. In **Session 2**, you will optimize the Command Center for slow 3G connections using @defer blocks and replace the basic HTML list with a sortable, paginated Angular Material grid both wired directly to this same EnrollmentStore.

Module 9 Lab Session 2: Performance and Enterprise Grid

**Module** M9 Angular Advanced Concepts **Session** 2 of 3

**Exercises** Exercise 2: @defer, OnPush\), Exercise 3 : MatTable,

viewChild \)

 

Story Thread

Liya opens the Instructor Command Center on her phone while riding the campus shuttle. Her device is on a 3G cellular connection. She watches a completely blank white screen for 8 full seconds.

Why? The Command Center includes an analytics charting library that weighs 2MB of JavaScript. Even though Liya has no intention of scrolling down to see the chart right now , she just wants to check pending enrollment counts. Her browser must download, parse, and execute all 2MB before *any* content appears. Meanwhile, Dawit has 5,000 enrollment records loaded into a plain HTML <table> . He cannot sort by student name without writing custom JavaScript. He cannot paginate without building his own controls. When he presses Ctrl\+F to search for “Liya,” the browser’s native find highlights text but offers no filtering or column-level search.

In this session, you solve both problems:

-**Exercise 2:** Use Angular’s @defer blocks to physically split the heavy chart into a separate JavaScript chunk that only downloads when the user scrolls to it.-**Exercise 3:** Replace the raw HTML table with an Angular Material MatTable that provides built-in sorting, pagination, and accessibility, wired directly into your EnrollmentStore.

Before You Begin

Session 1 left you with a working EnrollmentStore, an EnrollmentListComponent displaying enrollments with @for, and the Enrollment model. Confirm these files exist before continuing:

 src/app/models/enrollment.model.ts \(the Enrollment interface\)

 src/app/store/enrollment.store.ts \(the EnrollmentStore with

withEntities, loadEnrollments, approveEnrollment\)

 src/app/services/enrollment.service.ts \(the EnrollmentService HTTP

client\)

 src/app/features/enrollment-list/ \(the basic list component from

Session 1\)

Add Angular Material

Exercise 3 requires Angular Material. Install it now so both exercises can use it: ng add @angular/material

When prompted, choose any pre-built theme \(for example, “Indigo/Pink” or “Custom”\). Accept the defaults for typography and animations. Verify Animation Support

Angular Material components: sort arrows, paginator transitions, expansion panels require the Angular animations module. The ng add @angular/material command should have registered it automatically, but confirm it is present.

 

Exercise 2: Performance with @defer Blocks How @defer Actually Works

When you wrap a component in @defer, the Angular compiler physically separates that component’s code into a **distinct JavaScript chunk file** during the build. The main application JavaScript loads instantly without the deferred chunk. The chunk only downloads when the trigger condition fires. This is not CSS display: none. The code literally does not exist in the browser’s memory until the trigger fires.

Step 1: Generate the Components

You need two new components, the instructor dashboard \(the parent page\) and the analytics chart \(the heavy child that gets deferred\): ng generate component features/instructor-dashboard ng generate component ui/analytics-chart

Step 2: Build the Analytics Chart Component

This component simulates a heavyweight charting library. In a production TMS, this would be a real chart \(Chart.js, ngx-charts, or similar\) rendering enrollment trends. For this exercise, a styled placeholder with enough internal logic is sufficient to produce a measurable separate chunk.

Open analytics-chart.component.ts and replace its contents: **import** \{ Component, computed, input \} **from** '@angular/core'; **import** \{ Enrollment \} **from** '../../models/enrollment.model';

@Component\(\{

selector: 'tms-analytics-chart',

standalone: **true**,

template: \`

<div class="chart-container">

<h3>Enrollment Analytics</h3>

<div class="chart-bars">

<div class="bar approved"

\[style.height.px\]="approvedHeight\(\)">

<span>Approved</span>

</div>

<div class="bar pending"

\[style.height.px\]="pendingHeight\(\)">

<span>Pending</span>

</div>

<div class="bar rejected"

\[style.height.px\]="rejectedHeight\(\)">

<span>Rejected</span>

</div>

</div>

<p class="chart-summary">

Total records: \{\{ data\(\).length \}\}

</p>

</div>

\`,

stylesUrl:\`analytics-chat.component.scss\` // check at the last pages

// of this file

\}\)

**export class** AnalyticsChartComponent \{

data = input.required<Enrollment\[\]>\(\);

*// computed\(\) memoizes the result — the filter only re-runs when data*

*\(\) changes,*

*// not on every change detection cycle. This is the signal-first patt*

*ern M9 teaches.*

approvedHeight = computed\(\(\) **=>** \{

**const** count = **this**.data\(\).filter\(e **=>** e.status === 'Approved'\).leng th;

**return** Math.max\(20, count \* 3\);

\}\);

pendingHeight = computed\(\(\) **=>** \{

**const** count = **this**.data\(\).filter\(e **=>** e.status === 'Pending'\).lengt h;

**return** Math.max\(20, count \* 3\);

\}\);

rejectedHeight = computed\(\(\) **=>** \{

**const** count = **this**.data\(\).filter\(e **=>** e.status === 'Rejected'\).leng th;

**return** Math.max\(20, count \* 3\);

\}\);

\}

Step 3: Build the Instructor Dashboard

Open instructor-dashboard.component.ts and replace its contents. The critical UI enrollment count, pending approvals, action buttons renders immediately. The heavy chart defers:

**import** \{ Component, inject, OnInit \} **from** '@angular/core'; **import** \{ EnrollmentStore \} **from** '../../store/enrollment.store'; **import** \{ AnalyticsChartComponent \} **from** '../../ui/analytics-chart/analy tics-chart.component';

@Component\(\{

selector: 'tms-instructor-dashboard',

standalone: **true**,

imports: \[AnalyticsChartComponent\],

templateUrl: './instructor-dashboard.component.html',

styleUrl: './instructor-dashboard.component.scss' //check at the last

// pages of this file

\}\)

**export class** InstructorDashboardComponent **implements** OnInit \{

store = inject\(EnrollmentStore\);

ngOnInit\(\) \{

**this** .store.loadEnrollments\(\);

\}

\}

**Why is** **AnalyticsChartComponent** **in the** **imports** **array if we want it deferred?** Angular’s compiler needs the import to validate the <tms-analytics-chart> selector in the template. But because the component is used *only* inside a @defer block, Angular is smart enough to split it into a separate JavaScript chunk automatically. You get compile-time safety and runtime code splitting, the import does not pull the chart into the main bundle.

Step 4: Build the Dashboard Template

Open instructor-dashboard.component.html and add the following. Read the inline comments, they explain exactly which parts render immediately and which parts defer:

*<\!-- Renders instantly on any connection speed -->* <**div** class="dashboard-header">

<**h1**>Instructor Command Center</**h1**>

<**div** class="kpi-row">

<**div** class="kpi-card">

<**span** class="kpi-value">\{\{ store.entities\(\).length \}\}</**span**> <**span** class="kpi-label">Total Enrollments</**span**>

</**div**>

<**div** class="kpi-card pending">

<**span** class="kpi-value">\{\{ store.pendingCount\(\) \}\}</**span**> <**span** class="kpi-label">Pending Approval</**span**>

</**div**>

</**div**>

</**div**>

*<\!-- DEFERRED UI: The chart code lives in a separate .js chunk file -->* <**div** class="chart-section">

@defer \(on viewport; prefetch on idle\(500\)\) \{

<**tms-analytics-chart** \[data\]="store.entities\(\)" />

\} @placeholder \{

<**div** class="skeleton-chart">Scroll down to view analytics...</**div**>

\} @loading \(minimum 500ms\) \{

<**div** class="spinner">Downloading chart engine...</**div**>

\} @error \{

<**p**>Failed to load chart. Check your connection.</**p**>

\}

</**div**>

Step 5: Wire the Dashboard into Your App

The dashboard needs a route so learners can navigate to it. Open src/app/app.routes.ts and add a route for the dashboard: **import** \{ Routes \} **from** '@angular/router'; **export const** routes: Routes = \[

\{

path: 'dashboard',

loadComponent: \(\) **=>**

**import** \('./features/instructor-dashboard/instructor-dashboard.comp

onent'\)

.then\(m **=>** m.InstructorDashboardComponent\)

\},

*// ... your existing routes \(enrollment-list, etc.\)*

\{ path: '', redirectTo: 'dashboard', pathMatch: 'full' \}

\];

Notice the loadComponent syntax, this is lazy-loaded route-level code splitting. Combined with @defer inside the dashboard template, you now have *two layers* of lazy loading: the route chunk loads the dashboard, and then the viewport trigger loads the chart chunk only when needed.

What Each Piece Does

 **on viewport**: The chunk download triggers when the <div class="chart-

section"> scrolls into the browser’s visible area. Internally, Angular uses IntersectionObserver the same browser API that powers lazy-loaded images.

 **prefetch on idle\(500\)** : Even before the user scrolls, Angular begins

downloading the chunk file during browser idle time. The \(500\) is a timeout in milliseconds: if the browser never reaches a true idle state \(common on busy tablets\), the prefetch fires after 500ms anyway. Without this timeout, requestIdleCallback can wait indefinitely on overloaded devices.

 **@placeholder**: Renders instantly on page load. This is what Liya sees while

she reads enrollment counts, no blank screen. The min-height: 250px keeps it below the fold on mobile viewports.

 **@loading \(minimum 500ms\)**: Shows a spinner while the chunk downloads.

The minimum 500ms prevents a jarring flash if the download completes in 50ms.

 **@error**: Renders if the chunk download fails \(e.g., the device goes offline

mid-download\).

A Note on OnPush \(Angular 22 Default\)

Open the generated instructor-dashboard.component.ts. Notice that Angular 22’s CLI generates every component with changeDetection: ChangeDetectionStrategy.OnPush by default. In older versions, Angular re-checked *every* component in the tree on *any* browser event, a strategy now renamed Eager and deprecated. With OnPush, Angular only re-checks a component when its signal-bound inputs change reference or when a signal\(\) it reads emits a new value.

Because every component in M9 reads from the EnrollmentStore \(which is signal-based\), OnPush works naturally. You do not need to do anything special, just know that this is why your UI stays fast even with 5,000 rows. Verify Exercise 2

Follow these steps in order. Each one confirms a specific aspect of the code-splitting architecture.

1. **Build and check for chunk files.** Run:

ng build

Examine the terminal output. You should see a separate chunk file listed \(something like chunk-XXXX.js\). That is the analytics chart code living in its own file, physically separated from the main bundle.

2. **Test on a slow connection.** Start the dev server \(ng serve\), then open

Chrome DevTools -> Network tab. Set the throttling dropdown to **Slow** **3G**. Navigate to http://localhost:4200/dashboard.

3. **Confirm the critical UI loads first.** The dashboard header “Instructor

Command Center,” total enrollment count, pending count should appear within 1-2 seconds. The chart section should show the skeleton-chart placeholder \(“Scroll down to view analytics…”\).

4. **Scroll down to trigger the deferred chunk.** As you scroll the chart

section into view, watch the Network tab. A new JavaScript file \(chunk-XXXX.js \) should appear in the request list. The placeholder swaps to the “Downloading chart engine…” spinner, then the chart renders.

5. **Confirm the chunk did NOT load on page load.** Scroll back to the top of

the Network tab request list. The chart chunk should not appear among the initial page load requests only after you scrolled.

Exercise 3: Enterprise Data Grid with Angular Material Why MatTable Over Raw HTML

In Session 1, you built the enrollment list with @for it rendered cards, and that was all it did.

For an enterprise application managing thousands of records, you need: --**Column sorting** \(click a header to sort alphabetically or numerically\)-**Pagination** \(navigate 50 rows at a time instead of rendering 5,000 DOM nodes\)-**Accessibility** \(screen readers, keyboard navigation, ARIA attributes\) Angular Material’s MatTable provides all of this out of the box. The key architectural bridge is MatTableDataSource it wraps your data array and provides sorting, pagination, and filtering logic that Material’s directives consume. Step 1: Refactor the Enrollment List Component You are going to transform the EnrollmentListComponent from Session 1. Instead of the @for card layout, you will wire it into a Material data grid. Open enrollment-list.component.ts and replace the entire file with the following. Read the inline comments they explain every architectural choice: **import** \{ Component, viewChild, effect, inject \} **from** '@angular/core'; **import** \{ MatTableModule, MatTableDataSource \} **from** '@angular/material/t able';

**import** \{ MatPaginatorModule, MatPaginator \} **from** '@angular/material/pag inator';

**import** \{ MatSortModule, MatSort \} **from** '@angular/material/sort'; **import** \{ EnrollmentStore \} **from** '../../store/enrollment.store'; **import** \{ Enrollment \} **from** '../../models/enrollment.model'; @Component\(\{

selector: 'tms-enrollment-list',

standalone: **true**,

imports: \[MatTableModule, MatPaginatorModule, MatSortModule\],

templateUrl: './enrollment-list.component.html',

styleUrl: './enrollment-list.component.scss'//check at the last pages

// of this file

\}\)

**export class** EnrollmentListComponent \{

store = inject\(EnrollmentStore\);

displayedColumns = \['studentName', 'courseName', 'status', 'actions'\];

*// MatTableDataSource bridges our store data into Material's renderin*

*g pipeline*

dataSource = **new** MatTableDataSource<Enrollment>\(\);

*// viewChild.required\(\) is Angular 22's signal-based replacement for*

*@ViewChild.*

*// Unlike the legacy decorator, these are signals — they update react*

*ively when*

*// Angular resolves the template queries. No ngAfterViewInit lifecycl*

*e hook needed.*

**readonly** paginator = viewChild.required\(MatPaginator\);

**readonly** sort = viewChild.required\(MatSort\);

**constructor**\(\) \{

*// Effect 1: Push store entities into the Material data source when ever they change.*

*// Every time the store updates \(approve, load, rollback\), this eff ect fires*

*// and the table re-renders with fresh data.*

effect\(\(\) **=>** \{

**this** .dataSource.data = **this**.store.entities\(\);

\}\);

*// Effect 2: Wire paginator and sort controls once Angular resolves the view queries.*

*// Because viewChild returns a signal, this effect re-runs when the paginator*

*// or sort directives become available — no manual lifecycle hook n eeded.*

effect\(\(\) **=>** \{

**this** .dataSource.paginator = **this**.paginator\(\); **this** .dataSource.sort = **this**.sort\(\);

\}\);

*// Load enrollments on component creation*

**this** .store.loadEnrollments\(\);

\}

\}

**What changed from Session 1?** You removed the OnInit lifecycle hook and the @for template binding. The effect\(\) blocks now handle all data flow reactively. The loadEnrollments\(\) call moved into the constructor because there are no lifecycle dependencies the store is a singleton that can load immediately. Step 2: Build the Grid Template

Open enrollment-list.component.html and replace its entire contents: <**h2**>Enrollment Records</**h2**>

@if \(store.isLoading\(\)\) \{

<**p**>Loading enrollments...</**p**>

\}

@if \(store.error\(\)\) \{

<**p** class="error">\{\{ store.error\(\) \}\}</**p**>

\}

<**table** mat-table \[dataSource\]="dataSource" matSort class="mat-elevation-z8">

*<\!-- Student Name Column -->*

<**ng-container** matColumnDef="studentName">

<**th** mat-header-cell \*matHeaderCellDef mat-sort-header>Student</**th**>

<**td** mat-cell \*matCellDef="let row">\{\{ row.studentName \}\}</**td**>

</**ng-container**>

*<\!-- Course Name Column -->*

<**ng-container** matColumnDef="courseName">

<**th** mat-header-cell \*matHeaderCellDef mat-sort-header>Course</**th**>

<**td** mat-cell \*matCellDef="let row">\{\{ row.courseName \}\}</**td**>

</**ng-container**>

*<\!-- Status Column -->*

<**ng-container** matColumnDef="status">

<**th** mat-header-cell \*matHeaderCellDef mat-sort-header>Status</**th**>

<**td** mat-cell \*matCellDef="let row">

<**span** class="status-badge" \[class\]="row.status.toLowerCase\(\)">

\{\{ row.status \}\}

</**span**>

</**td**>

</**ng-container**>

*<\!-- Actions Column -->*

<**ng-container** matColumnDef="actions">

<**th** mat-header-cell \*matHeaderCellDef>Actions</**th**>

<**td** mat-cell \*matCellDef="let row">

@if \(row.status === 'Pending'\) \{

<**button** \(click\)="store.approveEnrollment\(row.id\)">Approve</**butt**

**on**>

\}

</**td**>

</**ng-container**>

*<\!-- Row definitions -->*

<**tr** mat-header-row \*matHeaderRowDef="displayedColumns"></**tr**>

<**tr** mat-row \*matRowDef="let row; columns: displayedColumns;"></**tr**>

</**table**>

<**mat-paginator**

\[pageSizeOptions\]="\[10, 25, 50\]"

showFirstLastButtons>

</**mat-paginator**>

Why \*matCellDef Uses Structural Directives You may notice that MatTable still uses \*matHeaderCellDef and \*matCellDef

structural directives with the asterisk syntax rather than the new @for control

flow. This is because Material’s table rendering pipeline is template-driven: each

ng-container defines a column *template* that Material instantiates per row. The

@for syntax works for simple loops, but Material’s table needs these template

references to manage virtual scrolling, sorting, and accessibility internally.

Step 3: Add the Enrollment List Route

If the enrollment list does not yet have a route, add one in src/app/app.routes.ts :

**export const** routes: Routes = \[

\{

path: 'dashboard',

loadComponent: \(\) **=>**

**import** \('./features/instructor-dashboard/instructor-dashboard.comp

onent'\)

.then\(m **=>** m.InstructorDashboardComponent\)

\},

\{

path: 'enrollments',

loadComponent: \(\) **=>**

**import** \('./features/enrollment-list/enrollment-list.component'\)

.then\(m **=>** m.EnrollmentListComponent\)

\},

\{ path: '', redirectTo: 'dashboard', pathMatch: 'full' \}

\];

Verify Exercise 3

1. **Start the dev server** \(ng serve\) and navigate to

http://localhost:4200/enrollments.

2. **Test sorting.** Click the **Student** column header. The rows should sort

alphabetically \(A-Z\). Click again for reverse order \(Z-A\). Repeat with the **Course** and **Status** columns.

3. **Test pagination.** If you have more than 10 enrollment records, the

paginator at the bottom should show page controls. Change the page size dropdown to 25 or 50. Click the forward/back arrows to navigate between pages.

4. **Test the Approve action.** Find a row with “Pending” status. Click the

**Approve** button. The status badge should flip to “Approved” instantly \(optimistic update from Session 1’s store\). If you also have the dashboard open in another tab, the pending count drops by one, both views read from the same EnrollmentStore.

5. **Confirm accessibility.** Press Tab to move focus into the table. Use Enter

on a column header to trigger sorting. Screen readers should announce “Student, sort button” when the header receives focus.

What You Just Built

Open the dashboard on a throttled Slow 3G connection. The KPI cards total enrollments and pending count appear within 1-2 seconds. The chart section shows a dashed skeleton placeholder. Only when you scroll down does the chart chunk download and render. Navigate to the enrollment list. Click the **Student** column header 5,000 rows sort instantly. Use the paginator to browse 50 records at a time.

Here is what you can show a teammate or facilitator to demonstrate competency:

 **Network tab proof:** On Slow 3G, the initial page load does not include

the chart chunk. Scrolling triggers a separate chunk-XXXX.js download.

 **Build output proof:** The ng build terminal output lists the deferred

chunk as a separate file with its own size.

 **Grid interaction proof:** Clicking any sortable column header reorders

rows. Changing the page size in the paginator updates the visible row count.

 **Store integration proof:** Approving an enrollment in the grid updates the

dashboard’s pending count without a page refresh, both components read the same EnrollmentStore.

What’s Next

Your Command Center now loads instantly on slow connections and displays data in a professional, sortable grid. In **Lab Session 3**, you will defend the application against rage-clicking instructors using exhaustMap and connect real-time server events via SignalR so all open dashboards stay synchronized.



## **SCSS REFERENCES**

**analytics-chart.scss**

.chart-container \{

padding: 1.5rem;

border: 1px solid \#334155;

border-radius: 8px;

background: \#0f172a;

color: \#e2e8f0;

\}

.chart-bars \{

display: flex;

gap: 2rem;

align-items: flex-end;

height: 200px;

padding: 1rem 0;

\}

.bar \{

width: 80px;

border-radius: 4px 4px 0 0;

display: flex;

align-items: flex-end;

justify-content: center;

padding-bottom: 0.5rem;

font-size: 0.85rem;

font-weight: 600;

min-height: 20px;

\}

.bar.approved \{ background: \#059669; \}

.bar.pending \{ background: \#d97706; \}

.bar.rejected \{ background: \#dc2626; \}

.chart-summary \{

margin-top: 1rem;

font-size: 0.9rem;

color: \#94a3b8;

\}



**instructor-dashboard.component.scss**

.dashboard-header \{

**padding**: 1.5rem;

\}

.kpi-row \{

**display**: flex;

**gap**: 1.5rem;

**margin-top**: 1rem;

\}

.kpi-card \{

**padding**: 1rem 1.5rem;

**border-radius**: 8px;

**background**: \#1e293b;

**color**: \#e2e8f0;

**display**: flex;

**flex-direction**: column;

**min-width**: 160px;

\}

.kpi-card.pending \{

**border-left**: 4px solid \#d97706;

\}

.kpi-value \{

**font-size**: 2rem;

**font-weight**: 700;

\}

.kpi-label \{

**font-size**: 0.85rem;

**color**: \#94a3b8;

**margin-top**: 0.25rem;

\}

.chart-section \{

**margin-top**: 2rem;

**padding**: 0 1.5rem;

\}

*/\**

*\* NOTE: This min-height prevents a CLS \(Cumulative Layout Shift\) bug.*

*\**

*\* The @defer \(on viewport\) trigger uses the browser's IntersectionObse*

*rver.*

*\* If .skeleton-chart renders at 0px height, the observer immediately r eports*

*\* it as "in viewport" — and the deferred chunk downloads on page load, \* defeating the entire purpose of code splitting.*

*\**

*\* Setting min-height reserves visual space, ensures the element starts \* off-screen on most devices, and keeps the page layout stable when \* the real chart swaps in.*

*\*/*

.skeleton-chart \{

**min-height**: 250px;

**display**: flex;

**align-items**: center;

**justify-content**: center;

**background**: \#1e293b;

**border** : 2px dashed \#334155;

**border-radius**: 8px;

**color**: \#64748b;

**font-size**: 1rem;

\}

.spinner \{

**min-height**: 250px;

**display**: flex;

**align-items**: center;

**justify-content**: center;

**color**: \#94a3b8;

**font-size**: 1rem;

\}

 

**enrollment-list.component.scss**

table \{

**width**: 100%;

\}

.status-badge \{

**padding**: 0.25rem 0.75rem;

**border-radius**: 12px;

**font-size**: 0.8rem;

**font-weight**: 600;

**text-transform**: uppercase;

\}

.status-badge.approved \{

**background**: \#065f46;

**color**: \#a7f3d0;

\}

.status-badge.pending \{

**background**: \#78350f;

**color**: \#fde68a;

\}

.status-badge.rejected \{

**background**: \#7f1d1d;

**color**: \#fca5a5;

\}

button \{

**padding**: 0.4rem 1rem;

**border** : none;

**border-radius**: 6px;

**background**: \#4f46e5;

**color**: white;

**cursor** : pointer;

**font-size**: 0.85rem;

\}

button***:hover*** \{

**background**: \#4338ca;

\}



Module 9 Lab Session 3: Defensive RxJS and Real-Time Sync

**Module** M9 Angular Advanced Concepts **Session** 3 of 3

**Exercises** Exercise 4: exhaustMap, takeUntilDestroyed, Exercise 5: SignalR

live sync

 

Story Thread

It is the last day of midterm grading. Dawit has 120 final grades to submit before the 5 PM deadline. He fills in the grade form for a student and clicks **Submit** **Final Grades**. The button gives no visual feedback no spinner, no disabled state. After waiting two seconds with nothing happening on his slow connection, he clicks again. And again. And again.

He opens the browser console and discovers the Network tab shows **five** identical POST requests to /api/grades. The database now contains five duplicate grade records for the same student.

The root cause: each button click called

this.api.postGrade\(payload\).subscribe\(\) independently. Every call launched a separate HTTP request in parallel. The server had no way to know the client was rage-clicking; it processed all five faithfully.

This is not a server bug. This is a **client architecture failure**. And the fix lives in how we manage the RxJS event stream.



Exercise 4: The Rage-Click Defender \(exhaustMap\) The Three Flattening Operators - When Each One Matters Before writing code, understand the three RxJS operators that handle “a new event arrives while a previous HTTP request is still in flight”: Operator What it does with the old Best use case

request

**switchMap** **Cancels** the old request, **Search boxes:** cancel the slow

starts the new one “Smi” search when the user types

“Smith”

**exhaustMap** **Ignores** the new emission, **Submit buttons:** drop rage-clicks

finishes the old one while the first request is pending

**concatMap** **Queues** the new one after **Sequential data syncs:** process

the old one finishes actions in strict order

For Dawit’s grade submission, the correct choice is exhaustMap. While the first POST is in flight, any subsequent clicks are silently dropped. The first request completes, the grade is saved once, and Dawit moves to the next student. Using switchMap here would be dangerous: it cancels the in-flight request on the client, but the server may have already processed and committed the grade before the cancellation frame arrives leading to a saved grade with no client confirmation.

Step 1: Generate the Component and Service Generate the grade submission feature component and its supporting HTTP service:

ng generate service services/grade --type=service ng generate component features/grade-submission --type=component

Step 2: Implement the Grade Service

Open src/app/services/grade.service.ts and implement the HTTP client interface using Angular 22’s @Service\(\) decorator:

 

**export interface** GradePayload \{

studentId: number;

courseId: number;

score: number;

\}

@Service\(\)

**export class** GradeService \{

**private** http = inject\(HttpClient\);

postGrade\(payload: GradePayload\): Observable<\{ id: string; success: b

oolean \}> \{

**return this**.http.post<\{ id: string; success: boolean \}>\('/api/grade s', payload\);

\}

\}

Step 3: Implement the Guarded Component Class \(Reactive Form\) Open src/app/features/grade-submission/grade-submission.component.ts. Import ReactiveFormsModule, FormBuilder, and Validators alongside Angular Material components. Construct an explicit gradeForm group and set up the Subject-based event stream protected by exhaustMap: @Component\(\{

selector: 'tms-grade-submission',

standalone: **true**,

imports: \[

*// Do import the necessary modules and components*

\],

templateUrl: './grade-submission.component.html'

\}\)

**export class** GradeSubmissionComponent \{

**private** api = inject\(GradeService\);

**private** fb = inject\(FormBuilder\);

*// Reactive Form definition with initial model values and validators*

gradeForm = **this**.fb.group\(\{

studentId: \[101, \[Validators.required, Validators.min\(1\)\]\],

courseId: \[302, \[Validators.required, Validators.min\(1\)\]\],

score: \[88, \[Validators.required, Validators.min\(0\), Validators.max\(100\)\]\]

\}\);

isSubmitting = **false**;

submissionStatus = '';

*// A Subject is a manual event stream — template clicks push payloads*

*into it*

**private** submitClick$ = **new** Subject<GradePayload>\(\);

**constructor**\(\) \{

**this** .submitClick$

.pipe\(

*// exhaustMap: while the inner HTTP observable is active, // ALL new emissions from submitClick$ are silently dropped. // Dawit can click 50 times — only ONE POST request fires.* exhaustMap\(payload **=>** \{

**this**.isSubmitting = **true**; **this**.submissionStatus = 'Submitting grade to server...'; **return this** .api.postGrade\(payload\);

\}\),

*// takeUntilDestroyed: automatically unsubscribes when Angular // destroys this component, preventing memory leaks. // Placed inside constructor to inherit the active injection*

*context.*

takeUntilDestroyed\(\)

\)

.subscribe\(\{

next: result **=>** \{

**this**.isSubmitting = **false**; **this**.submissionStatus = \`Grade saved successfully\! Record ID:

$\{result.id\}\`;

\},

error: err **=>** \{

**this**.isSubmitting = **false**; **this**.submissionStatus = \`Submission failed: $\{err.message ||

'Server error'\}\`;

\}

\}\);

\}

*// The template form submit handler pushes valid values into the*

*protected stream*

onSubmit\(\) \{

**if** \(**this**.gradeForm.valid\) \{

**const** rawValue = **this**.gradeForm.getRawValue\(\); **this** .submitClick$.next\(\{

studentId: Number\(rawValue.studentId\), courseId: Number\(rawValue.courseId\), score: Number\(rawValue.score\)

\}\);

\}

\}

\}

Step 4: Build the Grade Submission Template \(Reactive Form \+ Material \+ Tailwind\)

Open src/app/features/grade-submission/grade-submission.component.html and bind the \[formGroup\]="gradeForm" with formControlName bindings, validation error messages \(<mat-error>\), and button disability states: <**div** class="max-w-md mx-auto my-8">

<**mat-card** class="shadow-xl rounded-2xl bg-slate-900 border border-sla

te-800 text-slate-100 p-6">

<**mat-card-header** class="mb-4">

<**mat-card-title** class="text-xl font-bold text-slate-100">Grade Su

bmission Form</**mat-card-title**>

<**mat-card-subtitle** class="text-slate-400 text-sm">Instructor Midt

erm Grading</**mat-card-subtitle**>

</**mat-card-header**>

<**form** \[formGroup\]="gradeForm" \(ngSubmit\)="onSubmit\(\)">

<**mat-card-content** class="space-y-4">

<**mat-form-field** appearance="outline" class="w-full">

<**mat-label**>Student ID</**mat-label**> <**input** matInput type="number" formControlName="studentId" /> @if \(gradeForm.controls.studentId.hasError\('required'\)\) \{

<**mat-error**>Student ID is required</**mat-error**>

\}

</**mat-form-field**>

<**mat-form-field** appearance="outline" class="w-full">

<**mat-label**>Course ID</**mat-label**> <**input** matInput type="number" formControlName="courseId" /> @if \(gradeForm.controls.courseId.hasError\('required'\)\) \{

<**mat-error**>Course ID is required</**mat-error**>

\}

</**mat-form-field**>

<**mat-form-field** appearance="outline" class="w-full">

<**mat-label**>Score \(0-100\)</**mat-label**> <**input** matInput type="number" formControlName="score" /> @if \(gradeForm.controls.score.hasError\('min'\) || gradeForm.co

ntrols.score.hasError\('max'\)\) \{

<**mat-error**>Score must be between 0 and 100</**mat-error**>

\}

</**mat-form-field**>

@if \(isSubmitting\) \{

<**div** class="flex justify-center py-3">

<**mat-spinner** diameter="32"></**mat-spinner**>

</**div**>

\}

@if \(submissionStatus\) \{

<**div** class="mt-4 p-3 rounded-lg bg-slate-800 text-sky-400 tex

t-sm font-medium border border-slate-700">

\{\{ submissionStatus \}\}

</**div**>

\}

</**mat-card-content**>

<**mat-card-actions** class="mt-4">

<**button**

mat-raised-button

color="primary"

type="submit"

\[disabled\]="gradeForm.invalid || isSubmitting" class="w-full py-3 text-base font-semibold"> Submit Final Grade

</**button**>

</**mat-card-actions**>

</**form**>

</**mat-card**>

</**div**>

Step 5: Add Route Registration

Open src/app/app.routes.ts and add the lazy-loaded route: \{

path: 'grade-submission',

loadComponent: \(\) **=>**

**import** \('./features/grade-submission/grade-submission.component'\)

.then\(m **=>** m.GradeSubmissionComponent\)

\}

Verify Exercise 4

1. Start the dev server \(ng serve\) and navigate to

http://localhost:4200/grade-submission.

2. Try submitting invalid values \(e.g. score set to 150 or empty student ID\) —

observe reactive <mat-error> messages and the disabled Submit button.

3. Set the Chrome DevTools network throttling dropdown to **Slow 3G**

\(simulating slow server response times\).

4. Click the **Submit Final Grade** button rapidly **10 times in a row**.

5. Inspect the Network tab. You will observe exactly **one** POST request to

/api/grades, while the Material spinner provides visual loading feedback.

The subsequent 9 clicks were silently ignored by exhaustMap while the first request was in flight.

Exercise 5: Real-Time Sync with SignalR

The Problem with Polling

Right now, if another instructor approves an enrollment on a different computer, Dawit’s dashboard does not update until he manually refreshes the page. One approach would be to poll the API every few seconds with setInterval. But polling wastes bandwidth, delays updates, and does not scale when you have hundreds of connected clients.

**SignalR** solves this by opening a persistent WebSocket connection between the browser and the server. When a server-side event occurs \(enrollment approved, grade submitted, course status changed\), the server pushes the event directly to every connected client in real time.

In M7 Session 3, you built the backend TmsHub with a strongly-typed client interface \(ITmsHubClient\) that already sends ReceiveTranscriptReady and ReceiveGradePosted. Now you will extend that same hub contract with enrollment status broadcasts, wire the enrollment approval endpoint to push events, and build the Angular client that subscribes to them. Step 1: Extend the Backend Hub Client Interface In M7 Session 3, you created TmsApi.Application/Hubs/ITmsHubClient.cs with three events \(ReceiveTranscriptReady, ReceiveCourseUpdate, ReceiveGradePosted\). Open that file and add the enrollment status event: *// File: TmsApi.Application/Hubs/ITmsHubClient.cs* **public interface** ITmsHubClient

\{

*// New: broadcast enrollment status changes to all connected clients* Task ReceiveEnrollmentStatusUpdated\(string enrollmentId, string status\); \}

Because TmsHub extends Hub<ITmsHubClient>, this new method is immediately available on Clients.All, Clients.Group\(...\), and every other SignalR target with full compile-time type safety. No magic strings.

Step 2: Broadcast from the Enrollment Approval Endpoint Open your enrollment controller \(for example,

TmsApi.Api/Controllers/V2/EnrollmentsController.cs\). Inject IHubContext<TmsHub, ITmsHubClient> and broadcast the status change after the database commit succeeds:

*// File: TmsApi.Api/Controllers/V2/EnrollmentsController.cs* **public class** EnrollmentsController\(

*/\* your existing dependencies \*/*

IHubContext<TmsHub, ITmsHubClient> hubContext\) : ControllerBase \{

\[HttpPost\("\{id\}/approve"\)\]

**public** async Task<IActionResult> Approve\(string id, CancellationTok en ct\)

\{

*// Your existing approval logic ...*

*// After the database commit succeeds, broadcast to all connect*

*ed Angular clients*

await hubContext.Clients.All

.ReceiveEnrollmentStatusUpdated\(id, "Approved"\);

**return** NoContent\(\);

\}

Notice the call uses hubContext.Clients.All not Clients.Group\(...\). For enrollment status changes visible to all instructors on the dashboard, broadcasting to all connected clients is the correct choice. Step 3: Install SignalR Client Package \(Angular\) Install the official Microsoft SignalR client library in your Angular project: npm install @microsoft/signalr

Step 4: Configure the Dev Server Proxy

The Angular dev server runs on localhost:4200. Your .NET API runs on localhost:5000 \(or 5001 for HTTPS\). Without a proxy, relative URLs like /hubs/tms and /api/enrollments will hit localhost:4200 which returns 404 because there is no backend there.

Create proxy.conf.json in your Angular project root \(next to angular.json\): \{

"/api": \{

"target": "http://localhost:5000",

"secure": **false**,

"changeOrigin": **true**

\},

"/hubs": \{

"target": "http://localhost:5000",

"secure": **false**,

"ws": **true**

\}

\}

The "ws": true flag on the /hubs entry is critical, it tells the proxy to upgrade the connection to WebSocket protocol. Without it, SignalR’s WebSocket handshake fails silently and falls back to long polling \(much slower, higher server load\). Wire the proxy into angular.json under the serve options: "serve"**:** \{

"options": \{

"proxyConfig": "proxy.conf.json"

\}

\}

Restart ng serve after this change \(proxy config is only read at startup\). *The proxy makes your Angular dev server forward all* */api* *and* */hubs* *traffic to* *the .NET backend so both servers appear as the same origin to the browser.*

Step 5: Build the Live Sync Service

Generate the service \(ng generate service services/live-sync\). Open src/app/services/live-sync.service.ts and implement the production SignalR connection manager .

**export interface** EnrollmentStatusEvent \{

id: string;

status: 'Pending' | 'Approved' | 'Rejected';

\}

@Service\(\)

**export class** LiveSyncService \{

**private** platformId = inject\(PLATFORM\_ID\);

**private** connection: HubConnection | null = **null**;

**private** eventsSubject = **new** Subject<EnrollmentStatusEvent>\(\);

*// Expose events as an observable — the store will subscribe to this*

events$ = **this**.eventsSubject.asObservable\(\);

*// Connection state signal for UI status feedback*

connectionState = signal<'connected' | 'reconnecting' |

'disconnected'>\('disconnected'\);

connect\(\) \{

*// Guard against duplicate connections if called more than once*

**if** \(**this**.connection\) **return**;

*// SignalR uses WebSocket which only exists in browsers, not on the Node.js server.*

*// If SSR is enabled \(Extension 1\), this method runs during server render — skip it.*

**if** \(\!isPlatformBrowser\(**this**.platformId\)\) **return**;

*// Same hub URL and reconnect strategy you tested in M7 Session 3 browser DevTools*

**this** .connection = **new** HubConnectionBuilder\(\)

.withUrl\('/hubs/tms'\)

.withAutomaticReconnect\(\[0, 2000, 10000, 30000\]\) .build\(\);

*// The event name matches the ITmsHubClient method you just added on the backend.*

*// SignalR strongly-typed hubs send the method name as the event name automatically.*

**this** .connection.on\(

'ReceiveEnrollmentStatusUpdated', \(enrollmentId: string, status: 'Pending' | 'Approved' |

'Rejected'\) **=>** \{

**this**.eventsSubject.next\(\{ id: enrollmentId, status \}\);

\}

\);

**this** .connection.onreconnecting\(\(\) **=> this**.connectionState.set\('reconnecting'\)\);

**this** .connection.onreconnected\(\(\) **=> this**.connectionState.set\('connected'\)\);

**this** .connection.onclose\(\(\) **=> this**.connectionState.set\('disconnected'\)\);

**this** .connection

.start\(\)

.then\(\(\) **=> this**.connectionState.set\('connected'\)\) .catch\(err **=>** console.error\('SignalR connection error:', err\)\);

\}

\}

The event name 'ReceiveEnrollmentStatusUpdated' maps exactly to the ITmsHubClient.ReceiveEnrollmentStatusUpdated method you added in Step 1. When the backend calls

hubContext.Clients.All.ReceiveEnrollmentStatusUpdated\(id, "Approved"\), SignalR serialises the method name as the event identifier. The Angular client listens for that same string. This is why strongly-typed hubs matter, the C\# compiler catches typos before they become silent runtime mismatches. Step 6: Bridge Live Events into the SignalStore LiveSyncService exposes events as an observable \(events$\) rather than directly mutating state. This is a deliberate architectural choice: the **hub service** manages connection transport, while the **store** manages state mutations. Open src/app/store/enrollment.store.ts. Update the imports and add listenForLiveUpdates inside withMethods: **export const** EnrollmentStore = signalStore\(

\{ providedIn: 'root' \},

withEntities<Enrollment>\(\),

withMethods\(\(

store,

api = inject\(EnrollmentService\),

sync = inject\(LiveSyncService\)

\) **=>** \(\{

*// Listens to SignalR live sync stream and updates store state automatically*

listenForLiveUpdates: rxMethod<void>\(

pipe\(

tap\(\(\) **=>** sync.connect\(\)\), switchMap\(\(\) **=>** sync.events$\), tap\(event **=>** \{

patchState\(

store,

updateEntity\(\{ id: event.id, changes: \{ status:

event.status \} \}\)

\);

\}\)

\)

\),

*// ... your existing loadEnrollments\(\) and approveEnrollment\(\) methods*

\}\)\)

\);

Step 7: Activate Live Sync on App Startup

Open src/app/app.component.ts \(or instructor-dashboard.component.ts\) and trigger the live listener:

**export class** AppComponent **implements** OnInit \{

**private** store = inject\(EnrollmentStore\);

ngOnInit\(\) \{

**this** .store.loadEnrollments\(\);

**this** .store.listenForLiveUpdates\(\);

\}

\}

Verify Exercise 5 \(Real-Time Live Sync\)

1. Start both the .NET backend \(dotnet run --project TmsApi.Api\) and the

Angular dev server \(ng serve\).

2. Open two separate browser tabs side by side

\(http://localhost:4200/enrollments in Tab 1, and http://localhost:4200/dashboard in Tab 2\).

3. In Tab 1, approve a pending enrollment in the table grid.

4. Watch Tab 2: the pending count on the dashboard updates instantly

without a page refresh or manual API poll. The .NET

EnrollmentsController.Approve called hubContext.Clients.All.ReceiveEnrollmentStatusUpdated\(id, "Approved"\), which pushed the event through the WebSocket to every connected Angular LiveSyncService → EnrollmentStore → component binding.

5. \(Optional\) Open Chrome DevTools → Console and verify the connection

state: you should see no errors and the connectionState signal reads 'connected'. Stop the .NET backend briefly — the console logs “Reconnecting…” then “Reconnected” when you restart it, proving withAutomaticReconnect works.

 

What You Just Built

Open http://localhost:4200/grade-submission on Slow 3G throttling. Rapidly click **Submit Final Grade** 10 times. Exactly **one** HTTP request fires to /api/grades, accompanied by a Reactive Form, Tailwind styling, and a Material progress spinner. Open two browser windows side by side. Approve an enrollment in Tab 1 — Tab 2 updates instantly via SignalR push without polling. Here is what you can show a teammate or facilitator to demonstrate competency:

 **Reactive Form \+ Material UI proof:** Form utilizes ReactiveFormsModule,

FormBuilder, Validators, <mat-error> messages, and disabled button state.

 **RxJS** **exhaustMap** **proof:** On Slow 3G network throttling, rapid button

double-clicks result in exactly one network request in the DevTools Network tab.

 **RxJS Memory Safety proof:** Code uses takeUntilDestroyed\(\) in

constructor streams, preventing dangling subscription leaks.

 **SignalR Full-Stack Proof:** LiveSyncService connects to the M7 TmsHub

\(/hubs/tms\) and listens for ReceiveEnrollmentStatusUpdated; the strongly-typed ITmsHubClient method you extended on the backend. Status changes broadcast from the .NET EnrollmentsController update all open Angular clients instantly.

 **Architecture proof:** Separate concerns between transport

\(LiveSyncService\) and state management \(EnrollmentStore\).

 

Module 9 Complete

You have built a high-performance, real-time Angular 22 single-page application:

 **Session 1:** Centralized state management with NgRx SignalStore

eliminating data drift between components.

 **Session 2:** Performance optimization with @defer code splitting and

enterprise Material grids \(MatTable, MatSort, MatPaginator\).

 **Session 3:** Defensive RxJS with exhaustMap to prevent duplicate

submissions, and real-time push events via SignalR \(LiveSyncService\).

**Next module:** In **M10 Full-Stack Integration**, you will connect this Angular client to the .NET API under real browser security rules configuring CORS policies, HttpOnly authentication cookies, XSRF protection, functional HTTP interceptors, route guards, and optimistic UI rollback. Everything built here is your frontend foundation.



Module 10 Lab Session 1: Breaking the CORS Lock

**Module** M10 Full-Stack Integration **Session** 1 of 3

**Exercises** Exercise **1 :** Named CORS policy, environment driven API base

URLs, CourseService wiring

 

Story Thread

Liya is testing the new TMS enrollment page. On her phone and browser, she clicks **Enroll in Web Architecture**. Nothing happens. No loading spinner, no error banner just silence.

She opens Chrome Developer Tools \(F12\), switches to the Console tab, and sees a bright red error message: Access to XMLHttpRequest at 'http://localhost:5000/api/v1/courses' from origin 'http://localhost:4200' has been blocked by CORS policy. Yet, earlier that morning, Dawit tested the exact same endpoint using Scalar and Postman, and both received clean 200 OK responses. Why did desktop API tools succeed while Chrome blocked Liya’s browser? Scalar and Postman are backend utilities; they do not enforce the browser’s **Same-Origin Policy \(SOP\)**. Chrome, on the other hand, detects that your Angular application runs on localhost:4200 while your .NET API listens on localhost:5000. Because the port numbers differ, the browser treats them as two entirely separate websites and blocks JavaScript from reading the API’s response. In this lab, you will configure your .NET API to grant explicit permission to your Angular client and organize environment driven configuration URLs so your app is ready for production.

Exercise 1: Breaking the CORS Lock \(Policy and Environments\) Part A: Diagnose the Lock in DevTools

Before changing a single line of code, let’s observe the browser’s security enforcement firsthand:

1. Open your TmsApi project and comment out

**app.UseCors\("AllowAngular"\);** regisered by previous sessions;

2. Launch your .NET Web API project in one terminal window:

dotnet run

3. Launch your Angular application in a second terminal window:

ng serve

4. Open your browser to http://localhost:4200 and open **DevTools** \(F12 or

right click -> Inspect\).

5. Navigate to the Network tab and clear the log.

6. Trigger an HTTP request from Angular to your backend API.

7. Notice that the Network request appears red or fails to complete, and the

Console displays the CORS blockage message.

Notice something subtle: the server actually processed the request and returned a response\! But Chrome intercepted that response before your Angular TypeScript code could touch it.

Part B: Configure a Named CORS Policy in .NET 10 To tell Chrome that our Angular frontend is a trusted partner, we must declare a CORS policy on the .NET server.

1. Open appsettings.Development.json in your Web API project. Add the

allowed origin list so we do not hardcode URLs in C\# source code: \{

"AllowedOrigins": \[

"http://localhost:4200"

\]

\}

2. Open Program.cs in your Web API project.

3. Locate the service registration section \(before builder.Build\(\)\) and

define a named CORS policy called "TmsClient": *// Load allowed origins from appsettings.Development.json* var allowedOrigins = builder.Configuration

.GetSection\("AllowedOrigins"\).Get<string\[\]>\(\) ?? \["http://localhost:4200"\];

*// Register the CORS policy in the Dependency Injection container* builder.Services.AddCors\(options => \{

options.AddPolicy\("TmsClient", policy => \{

policy.WithOrigins\(allowedOrigins\)

.AllowAnyHeader\(\) .AllowAnyMethod\(\) .AllowCredentials\(\) *// Vital for HttpOnly auth cook*

*ies in Session 2*

.SetPreflightMaxAge\(TimeSpan.FromMinutes\(10\)\);

\}\);

\}\);

4. Scroll down to the HTTP request pipeline configuration section \(after

builder.Build\(\)\). Enable the policy using app.UseCors\(\): *// CRITICAL: Middleware order matters\!*

*// UseRouting -> UseCors -> UseAuthentication -> UseAuthorization* app.UseCors\("TmsClient"\);

**A Crucial Security Trap to Avoid:** Never combine .AllowAnyOrigin\(\)

with .AllowCredentials\(\). If you attempt to do so, ASP.NET Core will

throw an InvalidOperationException at server startup. The browser

specification strictly forbids wildcard origins when sending

authenticated credentials like cookies or auth headers, because doing so

would allow any malicious site on the web to make credentialed calls

against your user’s session.

5. Save Program.cs.

6. Stop your API terminal \(Ctrl\+C\) and restart it \(dotnet run\). Configuration

changes in Program.cs require a full process restart to take effect. Part C: Clean Environment Configurations and Domain Models Hardcoding URLs like http://localhost:5000 inside your services makes deploying to production painful. Let’s create Angular environment configurations to handle API base routes cleanly.

1. Angular 22 does not generate environment files by default. Run the

generator in your Angular project terminal:

ng generate environments

This command creates src/environments/environment.ts and src/environments/environment.development.ts, while updating angular.json automatically.

2. Open src/environments/environment.development.ts and set your local

API endpoint:

**export const** environment = \{

production: **false**,

apiUrl: '/api/v1'

\};

3. Open src/environments/environment.ts \(used for production builds\):

**export const** environment = \{

production: **true**,

apiUrl: '/api/v1'

\};

4. Verify your Course interface and PagedResponse<T> wrapper in

src/app/models/course.model.ts: **export interface** Course \{

id: number;

code: string;

title: string;

maxCapacity: number;

enrollmentCount: number; status?: string;

\}

**export interface** PagedResponse<T> \{

items: T\[\];

totalCount: number;

page: number;

pageSize: number;

totalPages: number;

hasPrevious: boolean;

hasNext: boolean;

\}

5. Update your CourseService \(src/app/services/course.service.ts\) to

use the environment config and modern Angular @Service\(\) injection patterns:

**import** \{ Service, inject \} **from** '@angular/core'; **import** \{ HttpClient \} **from** '@angular/common/http'; **import** \{ map \} **from** 'rxjs/operators'; **import** \{ environment \} **from** '../../environments/environment'; **import** \{ Course, PagedResponse \} **from** '../models/course.model'; @Service\(\)

**export class** CourseService \{

**private** http = inject\(HttpClient\); **private readonly** base = \`$\{environment.apiUrl\}/courses\`; getAll\(\) \{

**return this**.http

.get<PagedResponse<Course>>\(**this**.base, \{

params: \{ page: '1', pageSize: '50' \}

\}\)

.pipe\(map\(response **=>** response.items\)\);

\}

\}

Verifying Your Work

Switch back to your browser at http://localhost:4200 and open DevTools to inspect your request:

1. **Successful Communication:** The network request returns HTTP status 200

OK.

2. **Clean Console:** The browser console displays no red CORS error messages.

3. **Header Verification:** Click a network request to your API. In the Response

Headers you will see Access-Control-Allow-Origin: http://localhost:4200 proof that the named CORS policy is working.

 

What’s Next?

With the CORS lock broken and API communication established, your Angular app can read data from your backend. But what happens when Liya tries to perform an action that requires logging in?

Module 10 Lab Session 2: The Identity Handshake

**Module** M10 Full-Stack Integration **Session** 2 of 3

**Exercises** **Exercise 2:** HttpOnly auth cookie, XSRF double-submit

protection, credentials interceptor, AuthService

 

Story Thread

Abeba, a senior TMS administrator, logs in to review student enrollment records. In many basic tutorials, developers save JWT access tokens inside browser localStorage.

Why is that a major security hazard in enterprise applications? If an attacker manages to inject a Cross-Site Scripting \(XSS\) payload through a forum post, profile bio, or compromised npm dependency, a single line of malicious JavaScript localStorage.getItem\('auth\_token'\) can exfiltrate Abeba’s admin token to an external server. With that token, the attacker now owns the system.

In this lab, we build **The Identity Handshake**: 1. **HttpOnly Authentication Cookies:** The .NET API writes the auth token into an HttpOnly cookie. Because it is marked HttpOnly, client-side JavaScript \(including malicious XSS scripts\) is physically incapable of reading it. The browser attaches it automatically to outgoing requests.

2. **XSRF Double-Submit Protection:** Because browsers automatically send

cookies on cross-site requests, we must guard against Cross-Site Request Forgery \(CSRF\). We issue a secondary, readable cookie \(XSRF-TOKEN\). Angular reads this cookie and echoes it back in an X-XSRF-TOKEN HTTP header on mutating requests \(POST, PUT, DELETE\). Since malicious external sites cannot read cookies across origins under SOP rules, they cannot supply the matching header. 3. **Client Session State Wrapper \(****AuthService****\):** Angular’s AuthService drives the login sequence and tracks current user session signals without exposing raw tokens to DOM scripts.

Exercise 2: The Identity Handshake \(HttpOnly Cookie \+ XSRF\) Part A: Server-Side Cookie Issuance in .NET

First, let’s create \(or update\) Controllers/AuthController.cs in our Web API project to issue secure HttpOnly cookies upon login instead of returning raw tokens in the response body.

**Note:** In Module 10, we build the **browser cookie transport layer**. The

full database-backed ASP.NET Core Identity system, BCrypt password

hashing, and JWT signing keys will be built in **Module 12 \(Security &**

**Authentication\)**.

1. Create Auth DTOs in Application layer LoginRequest.cs and

UserProfile.cs

**public** record LoginRequest\(string Username, string Password\); **public** record UserProfileDto\(string DisplayName, string Role\);

2. Create a new file Controllers/AuthController.cs in your Web API project:

**namespace** TmsApi.Api.Controllers; \[ApiController\]

\[Route\("api/\{version:apiVersion\}/auth"\)\] **public class** AuthController : ControllerBase \{

\[HttpPost\("login"\)\]

**public** IActionResult Login\(

\[FromBody\] LoginRequest request, \[FromServices\] IWebHostEnvironment env\)

\{

*// Validate credentials \(demo account for M10 transport*

*testing\)*

**if** \(request.Username == "admin" && request.Password ==

"Password123\!"\)

\{

var dummyJwt = "header.payload.signature-demo-token"; *// Append HttpOnly authentication cookie — JavaScript*

*CANNOT read this token*

Response.Cookies.Append\("tms\_auth", dummyJwt, **new**

CookieOptions

\{

HttpOnly = **true**, Secure = \!env.IsDevelopment\(\), *// HTTPS in prod;*

*HTTP permitted locally over dev*

SameSite = SameSiteMode.Strict,

Expires = DateTimeOffset.UtcNow.AddHours\(2\)

\}\);

**return** Ok\(**new** UserProfileDto\("System Admin",

"Admin"\)\);

\}

**return** Unauthorized\(**new** \{ detail = "Invalid username or

password." \}\);

\}

\[HttpGet\("me"\)\]

**public** IActionResult GetCurrentUser\(\) \{

*// Inspect cookie attached automatically by the browser*

*on cross-origin requests*

**if** \(Request.Cookies.TryGetValue\("tms\_auth", **out** \_\)\) \{

**return** Ok\(**new** UserProfileDto\("System Admin",

"Admin"\)\);

\}

**return** Unauthorized\(**new** \{ detail = "Session expired or

missing authentication cookie." \}\);

\}

\}

Notice Secure = \!env.IsDevelopment\(\). In local development over HTTP \(http://localhost:5000\), setting Secure = true will cause browser security policies to reject the cookie immediately.

Part B: Configure Antiforgery Middleware in .NET Next, let’s configure ASP.NET Core to generate anti-forgery tokens for state-changing operations.

1. Open Program.cs in your Web API project.

2. Register the Antiforgery service with a header name matching Angular’s

default convention:

**using** Microsoft.AspNetCore.Antiforgery;

builder.Services.AddAntiforgery\(options => \{

options.HeaderName = "X-XSRF-TOKEN";

\}\);

3. Add middleware to issue the readable XSRF-TOKEN cookie whenever an

authenticated user communicates with the API. Place this middleware *after* app.UseAuthentication\(\) and app.UseAuthorization\(\): app.Use\(async \(context, next\) => \{

**if** \(context.User.Identity?.IsAuthenticated == **true** || context.

Request.Cookies.ContainsKey\("tms\_auth"\)\)

\{

var antiforgery = context.RequestServices

.GetRequiredService<IAntiforgery>\(\);

var tokens = antiforgery.GetAndStoreTokens\(context\); context.Response.Cookies.Append\("XSRF-TOKEN", tokens.Requ

estToken\!,

**new** CookieOptions \{

HttpOnly = **false**, *// MUST be false so Angular Jav*

*aScript can read it\!*

Secure = \!builder.Environment.IsDevelopment\(\), SameSite = SameSiteMode.Strict

\}\);

\}

await next\(context\);

\}\);

Part C: Configure Angular Credentials Interceptor and XSRF Handshake

By default, Angular’s HttpClient omits cookies when making HTTP requests across origins. We will write an HttpInterceptor to attach credentials automatically, and configure Angular’s built-in XSRF protection.

1. Create a file named src/app/interceptors/credentials.interceptor.ts:

**import** \{ HttpInterceptorFn \} **from** '@angular/common/http'; **export const** credentialsInterceptor: HttpInterceptorFn = \(req, ne xt\) **=>** \{

**return** next\(req.clone\(\{ withCredentials: **true** \}\)\);

\};

2. Open src/app/app.config.ts and register the interceptor along with

withXsrfConfiguration :

**export const** appConfig: ApplicationConfig = \{

providers: \[

provideHttpClient\(

withInterceptors\(\[credentialsInterceptor\]\), withXsrfConfiguration\(\{

cookieName: 'XSRF-TOKEN', *// Cookie name set by .NET*

*server*

headerName: 'X-XSRF-TOKEN', *// Header expected by .NET*

*server*

\}\)

\)

\]

\};

Now, every HTTP request emitted by HttpClient automatically includes withCredentials: true . Furthermore, Angular detects the XSRF-TOKEN cookie and automatically adds the X-XSRF-TOKEN header to all POST, PUT, and DELETE requests\!

Part D: Build AuthService for Cookie-Backed Sessions Now let’s build AuthService to manage current user session state in Angular using Signals.

1. Open src/app/services/auth.service.ts and implement cookie-backed

authentication state management:

**export interface** TmsUser \{

displayName: string;

role: string;

\}

**export interface** LoginRequest \{

username: string;

password: string;

\}

@Service\(\)

**export class** AuthService \{

**private** http = inject\(HttpClient\); currentUser = signal<TmsUser | null>\(**null**\); hasRole\(role: string\): boolean \{

**const** user = **this**.currentUser\(\); **return** user?.role === role || user?.role === 'Admin';

\}

**async** login\(credentials: LoginRequest\) \{

*// Server sets the HttpOnly cookie in the Set-Cookie response*

*header*

**await** firstValueFrom\(

**this** .http.post<void>\('/api/auth/login', credentials\)

\);

*// Fetch authenticated profile — browser automatically sends*

*the cookie*

**const** user = **await** firstValueFrom\(

**this** .http.get<TmsUser>\('/api/auth/me'\)

\);

**this** .currentUser.set\(user\);

\}

\}

**Transport vs. Role Authorization:** AuthService tracks the current

session state on the client. In **Module 12 \(Security & Authentication\)**,

you will pair this service with production functional route guards

\(roleGuard\) and ASP.NET Core Identity claims policies. Verifying Your Work

Launch your app, log in as an administrator or student, and inspect the state in Chrome DevTools:

1. **Inspect Cookies:** Open DevTools -> **Application** tab -> **Cookies**

\(http://localhost:4200 or localhost:5000\).

o Find tms\_auth: Check that the **HttpOnly** column has a checkmark. o Find XSRF-TOKEN: Check that the **HttpOnly** column is **blank**

\(allowing Angular JavaScript access\).

2. **Verify Storage is Clean:** Click **Local Storage** in DevTools. Ensure no

tokens or sensitive session data remain stored there.

3. **Verify Header Insertion:** In the DevTools **Network** tab, trigger a POST

action \(e.g., submitting an enrollment\). Inspect the Request Headers you will see X-XSRF-TOKEN populated with the value matching your XSRF-TOKEN cookie\!

What’s Next?

You now have a secure, cookie-backed identity handshake operating cleanly between Angular and .NET.

Module 10 Lab Session 3: Full-Stack Integration, Error Handling, and Optimistic Rollback **Module** M10 Full-Stack Integration **Session** 3 of 3

**Exercises** Exercises **3** and **4**

 

Story Thread

Liya tries to enroll in *Database Internals*, but the course reaches maximum capacity right as her request hits the server. The .NET API returns HTTP status 400 Bad Request containing an RFC 7807 ProblemDetails JSON body with a specific message: *“Course DB-401 is at maximum capacity.”* Without structured error interceptors, Angular might show a generic "Http failure response for \(unknown url\): 0 Unknown Error", leaving Liya guessing what went wrong.

Meanwhile, Abeba deletes an old course from the catalog. In modern applications, users expect instant UI feedback—they do not want to wait 800ms for a network round trip. But if Abeba deletes a course that still has active enrollments, the backend rejects the deletion \(409 Conflict\). How do we give users instant UI feedback while reliably restoring the original state if the server rejects the action?

In this final integration session, you complete the full-stack loop: 1. **Server Side** **ProblemDetails & Error Interceptor:** Configure .NET 10 to emit RFC 7807 standard error payloads, parse them centrally in Angular, and handle expired sessions \(401 Unauthorized\) seamlessly. 2. **Optimistic UI Deletion with** **Automatic Rollback:** Instantly mutate local store state for instant visual feedback, while snapshotting state to roll back cleanly if server validation fails. 3. **End-to-****End Integration Sprint:** Bring CORS policies, cookies, XSRF headers, error interceptors, and real-time SignalR updates together into a complete, working full-stack system\!

Exercise 3: Full-Stack Integration, Error Handling, and Optimistic Rollback

Part A: Server-Side ProblemDetails & Angular Error Interceptor First, let’s ensure our .NET API formats all exception and validation errors as RFC 7807 ProblemDetails JSON objects, then write an Angular functional HttpInterceptor to catch HTTP errors globally.

1. Open Program.cs in your Web API project. Register ProblemDetails

middleware services:

*// Add RFC 7807 ProblemDetails support to the DI container* builder.Services.AddProblemDetails\(\);

2. Scroll down to the middleware pipeline \(after builder.Build\(\)\) and

enable the status code pages middleware:

app.UseStatusCodePages\(\); *// Converts 4xx/5xx responses into stan dard ProblemDetails payloads*

3. Next, create src/app/interceptors/error.interceptor.ts in your

Angular project:

**export const** errorInterceptor: HttpInterceptorFn = \(req, next\) **=>**

\{

**const** router = inject\(Router\);

**return** next\(req\).pipe\(

catchError\(\(err: HttpErrorResponse\) **=>** \{

*// Extract C\# RFC 7807 ProblemDetails detail property* **const** detailMessage = err.error?.detail ?? 'A system error

occurred. Please try again.';

**if** \(err.status === 401\) \{

*// Redirect expired or unauthenticated sessions back to l*

*ogin*

router.navigate\(\['/login'\]\);

\} **else** \{

*// Surface structured error to developer console / UI not*

*ification*

console.error\('API Error Response:', detailMessage\);

\}

**return** throwError\(\(\) **=>** err\);

\}\)

\);

\};

4. Register errorInterceptor in src/app/app.config.ts alongside your

credentialsInterceptor:

**import** \{ credentialsInterceptor \} **from** './interceptors/credential s.interceptor';

**import** \{ errorInterceptor \} **from** './interceptors/error.intercepto r';

**export const** appConfig: ApplicationConfig = \{

providers: \[

provideHttpClient\(

withInterceptors\(\[credentialsInterceptor, errorIntercepto

r\]\),

withXsrfConfiguration\(\{

cookieName: 'XSRF-TOKEN', headerName: 'X-XSRF-TOKEN',

\}\)

\)

\]

\};

 

Part B: Optimistic UI Deletion with Automatic Snapshot Rollback Now, let’s implement optimistic UI updates for course deletion. When Abeba clicks **Delete Course**, we remove the entity from our SignalStore immediately. If the server rejects the call \(for instance, if active student enrollments exist\), we restore the previous snapshot seamlessly.

1. Open src/app/store/course.store.ts.

2. Implement deleteCourse inside withMethods:

 

**export const** CourseStore = signalStore\(

\{ providedIn: 'root' \},

withMethods\(\(store, svc = inject\(CourseService\)\) **=>** \(\{

deleteCourse\(id: number\) \{

*// 1. Take snapshot of current entities BEFORE mutating local*

*state*

**const** previousSnapshot = store.entities\(\); *// 2. Instant visual feedback — remove entity immediately from*

*local UI*

patchState\(store, removeEntity\(id\)\); *// 3. Dispatch API call to backend server*

svc.delete\(id\).pipe\(

catchError\(err **=>** \{

*// 4. Server rejected request — restore previous snapshot*

*and set error message*

patchState\(store, setAllEntities\(previousSnapshot\)\); patchState\(store, \{

error: 'Cannot delete course: active student enrollments

exist.'

\}\);

**return** EMPTY;

\}\)

\).subscribe\(\);

\}

\}\)\)

\);

**Execution Order Rule:** The store.entities\(\) snapshot MUST be

captured *before* calling patchState\(store, removeEntity\(id\)\). If you

snapshot after patchState, your rollback snapshot will already be

missing the deleted item\!

Part C: SignalR CORS Verification

In **Module 9 Lab Session 3**, you built the LiveSyncService and EnrollmentStore.listenForLiveUpdates to stream real-time status updates over WebSockets. Now that your backend enforces a named CORS policy, let’s verify that your SignalR hub allows cross-origin connections.

1. Open Program.cs in your Web API project.

2. Locate the SignalR hub mapping and ensure .RequireCors\("TmsClient"\)

is chained:

app.MapHub<TmsHub>\("/hubs/tms"\).RequireCors\("TmsClient"\);

3. Restart your API \(dotnet run\). SignalR WebSockets will now successfully

negotiate cross-origin handshakes from http://localhost:4200. Exercise 4: End-to-End Integration Sprint \(Verification\) Bring all integration layers together and verify your complete full-stack application:

1. **Launch Both Servers:**

o Start the ASP.NET Core API \(dotnet run on port 5000\). o Start the Angular dev server \(ng serve on port 4200\).

2. **Execute Identity Handshake:** Log in via Angular. Open DevTools ->

Application -> Cookies and verify tms\_auth is marked **HttpOnly**.

3. **Fetch Data with Environment Config:** Browse the catalog. Confirm

CourseService retrieves data directly using environment.apiUrl.

4. **Mutate Data with XSRF Guard:** Perform an action \(e.g. updating a

record\). Verify in the Network tab that the request carries the automatic X-XSRF-TOKEN header.

5. **Verify Real-Time SignalR Stream:** Open two browser windows. Trigger

an update in Window 1 and observe Window 2 update automatically over WebSockets.

6. **Verify Error Interception:** Attempt an invalid action \(e.g., enrolling in a

full course\). Confirm the browser console displays the C\# ProblemDetails detail text.

7. **Test Optimistic Rollback:** Delete a course that has active enrollments.

Confirm the card disappears instantly from the screen, then reappears a moment later when the server returns 409 Conflict.

Integration Complete\!

Congratulations\! You have successfully fused an enterprise Angular 22 frontend with an ASP.NET Core 10 backend.

Your application now operates under real browser security rules with named CORS policies, secure HttpOnly session cookies, XSRF protection, functional HTTP interceptors, rate-limited SignalR WebSocket updates, and robust optimistic UI mutations with automatic server rollback.

Module 11 Lab Session 1: Password Hashing Mechanics & ASP.NET Core Identity Setup **Session** 1 of 3

**Exercises** Exercise **1** & Exercise **2** **Integrity Lab** **Tier 1** Salting & hashing mechanics, IdentityUser extension, **tier** IdentityDbContext, password policy, lockout config

 

Story thread

Before configuring ASP.NET Core Identity, you will use BCrypt.Net-Next to observe how salts prevent rainbow table lookups and slow down brute-force attacks. Next, you will integrate **ASP.NET Core Identity** into the TMS API to manage TmsUser accounts, enforce enterprise password policies, handle brute-force lockouts, and run database migrations.

Exercise 1: The Mechanics of Password Hashing **Context:** Plain-text passwords and unsalted hashes \(like raw MD5 or SHA256\) are major security vulnerabilities. BCrypt uses a salt and work factor to slow down brute-force attacks.

Step 1: Install BCrypt Package

Open a terminal in your Web API project directory \(Tms.Api\): Dotnet add TmsApi.Infrastructure/TmsApi.Infrastructure.csproj package B Crypt.Net-Next

Step 2: Create Cryptography Service

Create TmsApi.Infrastructure/Services/CryptoDemoService.cs: **namespace** TmsApi.Infrastructure.Services; **public class** CryptoDemoService

\{

**public** string HashUserPassword\(string plainText\)

\{

*// BCrypt automatically generates a unique salt and prepends it t*

*o the hash.*

*// workFactor: 12 means 2^12 key expansion iterations.*

**return** BCrypt.Net.BCrypt.HashPassword\(plainText, workFactor: 12\);

\}

**public** bool VerifyUserPassword\(string plainText, string hashedDbPass

word\)

\{

**return** BCrypt.Net.BCrypt.Verify\(plainText, hashedDbPassword\);

\}

\}

Step 3: Test Salt Uniqueness

Inspect in an endpoint or test method:

var service = **new** CryptoDemoService\(\); string hash1 = service.HashUserPassword\("Password123\!"\); string hash2 = service.HashUserPassword\("Password123\!"\); *// hash1 and hash2 are completely different strings because of unique ra ndom salts\!*

Console.WriteLine\($"Hash 1: \{hash1\}"\); Console.WriteLine\($"Hash 2: \{hash2\}"\); *// Both verify to true against the same plain text:* bool match1 = service.VerifyUserPassword\("Password123\!", hash1\);*// true* bool match2 = service.VerifyUserPassword\("Password123\!", hash2\);*// true*

**Decision Rule:** Never write custom hashing algorithms in production. In

Exercise 2 onwards, you will use UserManager provided by ASP.NET Core

Identity.

 

Exercise 2: ASP.NET Core Identity Setup & AuthController Step 1: Extend IdentityUser

Install the necessary packages to Infrastructure layer: **Cd TmsApi.Infrastructure/TmsApi.Infrastructure dotnet add package Microsoft.Extensions.Identity.Stores dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore** Create TmsApi.Infrastructure/Identity/TmsUser.cs in your API project: **using** Microsoft.AspNetCore.Identity; **namespace** TmsApi.Infrastructure.Identity; **public class** TmsUser : IdentityUser \{

**public** string FirstName \{ **get**; **set**; \} = string.Empty;

**public** string LastName \{ **get**; **set**; \} = string.Empty;

**public** string? Department \{ **get**; **set**; \}

\}

Step 2: Update DbContext

In TmsApi.Infrastructure/Persistence/TmsDbContext.cs, update your context to inherit from IdentityDbContext<TmsUser>: **namespace** TTmsApi.Infrastructure.Persistence; **public class** TmsDbContext : IdentityDbContext<TmsUser> \{

**public** TmsDbContext\(DbContextOptions<TmsDbContext> options\) :

**base**\(options\) \{ \}

\}

Step 3: Configure Identity Services in Program.cs Open Program.cs and configure Identity Core with enterprise password and lockout policies:

builder.Services.AddIdentityCore<TmsUser>\(options => \{

*// Enterprise Password Policy*

options.Password.RequiredLength = 12;

options.Password.RequireUppercase = **true**;

options.Password.RequireDigit = **true**;

options.Password.RequireNonAlphanumeric = **true**;

*// Brute-Force Lockout Protection*

options.Lockout.MaxFailedAccessAttempts = 5;

options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes\(15\);

options.Lockout.AllowedForNewUsers = **true**;

\}\)

.AddRoles<IdentityRole>\(\)

.AddEntityFrameworkStores<TmsDbContext>\(\);

Step 4: Replace the Demo AuthController with Identity Replace the M10 demo **AuthController** with the Identity-backed version below. The M10 version used hardcoded credentials for transport testing this version uses **UserManager** for real accounts. Update TmsApi.Api/Controllers/AuthController.cs: **namespace** TmsApi.Api.Controllers; \[ApiController\]

\[Route\("api/\[controller\]"\)\]

**public class** AuthController : ControllerBase \{

**private readonly** UserManager<TmsUser> \_userManager;

**private readonly** RoleManager<IdentityRole> \_roleManager;

**public** AuthController\(

UserManager<TmsUser> userManager, RoleManager<IdentityRole> roleManager\)

\{

\_userManager = userManager; \_roleManager = roleManager;

\}

**public** record RegisterRequest\(

string Email,

string Password,

string FirstName,

string LastName,

string Role\);

\[HttpPost\("register"\)\]

**public** async Task<IActionResult> Register\(\[FromBody\]

RegisterRequest request\)

\{

var existingUser = await

\_userManager.FindByEmailAsync\(request.Email\);

**if** \(existingUser \!= **null**\) \{

*// Prevent account enumeration by returning a generic*

*response*

**return** Ok\(**new** \{ message = "Registration request

received." \}\);

\}

var user = **new** TmsUser

\{

UserName = request.Email,

Email = request.Email, FirstName = request.FirstName, LastName = request.LastName

\};

var result = await \_userManager.CreateAsync\(user,

request.Password\);

**if** \(\!result.Succeeded\)

\{

var errors = result.Errors.Select\(e => e.Description\); **return** BadRequest\(**new** \{ errors \}\);

\}

*// Ensure requested role exists*

**if** \(\!await \_roleManager.RoleExistsAsync\(request.Role\)\) \{

await \_roleManager.CreateAsync\(**new**

IdentityRole\(request.Role\)\);

\}

await \_userManager.AddToRoleAsync\(user, request.Role\); **return** Ok\(**new** \{ message = "Registration successful." \}\);

\}

**public** record LoginRequest\(string Email, string Password\);

\[HttpPost\("login"\)\]

**public** async Task<IActionResult> Login\(\[FromBody\] LoginRequest

request\)

\{

var user = await \_userManager.FindByEmailAsync\(request.Email\); **if** \(user == **null**\)

\{

**return** Unauthorized\(**new** \{ detail = "Invalid credentials." \}\);

\}

**if** \(await \_userManager.IsLockedOutAsync\(user\)\) \{

**return** StatusCode\(423, **new** \{ detail = "Account locked due to

multiple failed login attempts. Try again in 15 minutes." \}\);

\}

var validPassword = await \_userManager.CheckPasswordAsync\(user,

request.Password\);

**if** \(\!validPassword\)

\{

await \_userManager.AccessFailedAsync\(user\); **return** Unauthorized\(**new** \{ detail = "Invalid credentials." \}\);

\}

*// Reset failed attempt counter on successful login* await \_userManager.ResetAccessFailedCountAsync\(user\); **return** Ok\(**new**

\{

userId = user.Id,

email = user.Email,

firstName = user.FirstName, lastName = user.LastName

\}\);

\}

\}

Step 5: Run Database Migration

Run migrations from your solution root targeting the Infrastructure and Api projects:

dotnet ef migrations add AddIdentitySupport --project TmsApi.Infrastruc ture/TmsApi.Infrastructure.csproj --startup-project TmsApi.Api/TmsApi. Api.csproj

dotnet ef database update --project TmsApi.Infrastructure/TmsApi.Infras tructure.csproj --startup-project TmsApi.Api/TmsApi.Api.csproj **Expected result:** The migration creates seven standard AspNet... tables \(AspNetUsers, AspNetRoles, AspNetUserClaims, etc.\) in your database.

Verification Checkpoint \(Session 1\)

1. **Scalar / HTTP Verification:**

o Open OpenAPI / Scalar at https://localhost:5001/scalar/v1. o Send POST /api/auth/register with body:

\{

"email": "leul.instructor@cotbe.edu.et", "password": "SecurePass123\!", "firstName": "Leul", "lastName": "Gebre", "role": "Instructor"

\}

o Confirm response 200 OK.

2. **Password Validation Check:**

o Attempt registration with password "short". Confirm 400 Bad

Request listing password policy errors.

3. **Lockout Check:**

o Attempt POST /api/auth/login with wrong password 5 times. On

6th attempt, confirm status 423 Locked returned.



Module 11 Lab Session 2: JWT Bearer Authentication & Refresh Token Rotation **Session** 2 of 3

**Exercises** Exercise **3** & Exercise **4** **Integrity Lab** **Tier 2** — JWT claim claims, HMAC-SHA256 signature, **tier** AddJwtBearer authentication, RefreshToken entity, rotation,

single-use validation

 

Story thread

When a user submits valid credentials to /api/auth/login, the API must issue a cryptographically signed JSON Web Token \(JWT\) containing user identity claims \(sub, email, role\). Short-lived access tokens \(15 minutes\) reduce the impact of token theft. To maintain continuous user sessions, you will implement **Refresh** **Token Rotation**: every refresh call invalidates the old token \(IsUsed = true\) and issues a brand-new pair. Submitting an already-used refresh token triggers **token** **theft detection** and revokes all active sessions for that user.

Exercise 3: JWT Generation & Bearer Authentication Pipeline Step 1: Configure Jwt Settings & User Secrets In appsettings.Development.json:

\{

"Jwt": \{

"Issuer": "https://localhost:5001",

"Audience": "tms-client",

"ExpiryMinutes": 15

\}

\}

Store the cryptographic signing key safely in User Secrets: dotnet user-secrets set "Jwt:Key" "A-Very-Long-Secret-Key-For-TMS-Auth-Stored-Safely-2026"

Step 2: Implement TokenService

Create TmsApi.Infrastructure/Services/TokenService.cs: **namespace** TmsApi.Infrastructure.Services; **public class** TokenService

\{

**private readonly** IConfiguration \_config;

**public** TokenService\(IConfiguration config\)

\{

\_config = config;

\}

**public** string GenerateJwt\(TmsUser user, IList<string> roles\)

\{

var claims = **new** List<Claim> \{

**new** Claim\(ClaimTypes.NameIdentifier, user.Id\), **new** Claim\(ClaimTypes.Email, user.Email ?? string.Empty\), **new** Claim\("FirstName", user.FirstName\)

\};

**foreach** \(var role **in** roles\) \{

claims.Add\(**new** Claim\(ClaimTypes.Role, role\)\);

\}

var key = **new**

SymmetricSecurityKey\(Encoding.UTF8.GetBytes\(\_config\["Jwt:Key"\]\!\)\);

var creds = **new** SigningCredentials\(key,

SecurityAlgorithms.HmacSha256\);

var token = **new** JwtSecurityToken\(

issuer: \_config\["Jwt:Issuer"\], audience: \_config\["Jwt:Audience"\], claims: claims,

expires:

DateTime.UtcNow.AddMinutes\(int.Parse\(\_config\["Jwt:ExpiryMinutes"\]\!\)\),

signingCredentials: creds

\);

**return new** JwtSecurityTokenHandler\(\).WriteToken\(token\);

\}

\}

Step 3: Configure Authentication Pipeline in Program.cs In Program.cs, add JWT Bearer authentication and register TokenService: builder.Services.AddScoped<TokenService>\(\); builder.Services.AddAuthentication\(options => \{

options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; \}\)

.AddJwtBearer\(options =>

\{

options.TokenValidationParameters = **new** TokenValidationParameters

\{

ValidateIssuer = **true**,

ValidateAudience = **true**, ValidateLifetime = **true**, ValidateIssuerSigningKey = **true**, ValidIssuer = builder.Configuration\["Jwt:Issuer"\], ValidAudience = builder.Configuration\["Jwt:Audience"\], IssuerSigningKey = **new** SymmetricSecurityKey\(

Encoding.UTF8.GetBytes\(builder.Configuration\["Jwt:Key"\]\!\)\)

\};

\}\);

 

Exercise 4: Refresh Token Rotation & Theft Detection Step 1: Create RefreshToken Entity

Create TmsApi.Domain.Entities/RefreshToken.cs: **namespace** TmsApi.Domain.Entities; **public class** RefreshToken

\{

**public** int Id \{ **get**; **set**; \}

**public** string Token \{ **get**; **set**; \} = string.Empty;

**public** string UserId \{ **get**; **set**; \} = string.Empty;

**public** DateTime ExpiresAt \{ **get**; **set**; \}

**public** bool IsUsed \{ **get**; **set**; \}

**public** bool IsRevoked \{ **get**; **set**; \} \}

Add DbSet<RefreshToken> RefreshTokens \{ get; set; \} to TmsDbContext.cs and run migration:

dotnet ef migrations add AddRefreshTokens --project TmsApi.Infrastructu re/TmsApi.Infrastructure.csproj --startup-project TmsApi.Api/TmsApi.Api. csproj

dotnet ef database update --project TmsApi.Infrastructure/TmsApi.Infras tructure.csproj --startup-project TmsApi.Api/TmsApi.Api.csproj

Step 2: Update AuthController with Token Generation & Rotation Update Controllers/AuthController.cs to inject TmsDbContext and TokenService, and add POST /api/auth/refresh: **namespace** TmsApi.Api.Controllers; \[ApiController\]

\[Route\("api/\[controller\]"\)\]

**public class** AuthController : ControllerBase \{

**private readonly** UserManager<TmsUser> \_userManager;

**private readonly** RoleManager<IdentityRole> \_roleManager;

**private readonly** TmsDbContext \_context;

**private readonly** TokenService \_tokenService;

**public** AuthController\(

UserManager<TmsUser> userManager, RoleManager<IdentityRole> roleManager, TmsDbContext context,

TokenService tokenService\)

\{

\_userManager = userManager; \_roleManager = roleManager; \_context = context;

\_tokenService = tokenService;

\}

\[HttpPost\("login"\)\]

**public** async Task<IActionResult> Login\(\[FromBody\] LoginRequest request\)

\{

var user = await \_userManager.FindByEmailAsync\(request.Email\); **if** \(user == **null**\) **return** Unauthorized\(**new** \{ detail = "Invalid

credentials." \}\);

**if** \(await \_userManager.IsLockedOutAsync\(user\)\) \{

**return** StatusCode\(423, **new** \{ detail = "Account locked due

to multiple failed login attempts." \}\);

\}

var validPassword = await \_userManager.CheckPasswordAsync\(user,

request.Password\);

**if** \(\!validPassword\)

\{

await \_userManager.AccessFailedAsync\(user\); **return** Unauthorized\(**new** \{ detail = "Invalid

credentials." \}\);

\}

await \_userManager.ResetAccessFailedCountAsync\(user\); var roles = await \_userManager.GetRolesAsync\(user\); var accessToken = \_tokenService.GenerateJwt\(user, roles\); *// Issue initial Refresh Token*

var refreshToken = **new** RefreshToken \{

Token = Guid.NewGuid\(\).ToString\("N"\), UserId = user.Id,

ExpiresAt = DateTime.UtcNow.AddDays\(7\), IsUsed = **false**,

IsRevoked = **false**

\};

\_context.RefreshTokens.Add\(refreshToken\); await \_context.SaveChangesAsync\(\); **return** Ok\(**new**

\{

accessToken,

refreshToken = refreshToken.Token

\}\);

\}

**public** record RefreshRequest\(string RefreshToken\);

\[HttpPost\("refresh"\)\]

**public** async Task<IActionResult> Refresh\(\[FromBody\] RefreshRequest request\)

\{

var storedToken = await \_context.RefreshTokens

.FirstOrDefaultAsync\(rt => rt.Token ==

request.RefreshToken\);

**if** \(storedToken == **null**\) \{

**return** Unauthorized\(**new** \{ detail = "Invalid refresh

token." \}\);

\}

*// Theft Detection: If an ALREADY-USED token is submitted,*

*revoke ALL tokens for this user\!*

**if** \(storedToken.IsUsed\) \{

var userTokens = await \_context.RefreshTokens

.Where\(rt => rt.UserId == storedToken.UserId\) .ToListAsync\(\);

**foreach** \(var t **in** userTokens\) \{

t.IsRevoked = **true**;

\}

await \_context.SaveChangesAsync\(\); **return** Unauthorized\(**new** \{ detail = "Token theft detected.

All user sessions revoked." \}\);

\}

**if** \(storedToken.IsRevoked || storedToken.ExpiresAt <

DateTime.UtcNow\)

\{

**return** Unauthorized\(**new** \{ detail = "Refresh token expired

or revoked." \}\);

\}

*// Mark current token as used*

storedToken.IsUsed = **true**; *// Issue brand-new Refresh Token pair*

var newRefreshToken = **new** RefreshToken \{

Token = Guid.NewGuid\(\).ToString\("N"\), UserId = storedToken.UserId, ExpiresAt = DateTime.UtcNow.AddDays\(7\), IsUsed = **false**,

IsRevoked = **false**

\};

\_context.RefreshTokens.Add\(newRefreshToken\); await \_context.SaveChangesAsync\(\); var user = await \_userManager.FindByIdAsync\(storedToken.UserId\);

var roles = await \_userManager.GetRolesAsync\(user\!\); var newAccessToken = \_tokenService.GenerateJwt\(user\!, roles\); **return** Ok\(**new**

\{

accessToken = newAccessToken, refreshToken = newRefreshToken.Token

\}\);

\}

\}

**Client Integration Note \(Token Migration from M10\):** Unlike M10’s

cookie-only transport where the browser implicitly received Set-Cookie,

POST /api/auth/login and POST /api/auth/refresh now return

\{ accessToken, refreshToken \} explicitly in the JSON response

payload. In **Session 3 \(Exercise 6\)**, we update Angular’s AuthService to

hold accessToken in memory and configure jwtInterceptor to attach it

as Authorization: Bearer <token> to outgoing API requests.

 

Verification Checkpoint \(Session 2\)

1. **Login & Claims Inspection:**

o Send POST /api/auth/login. Confirm response returns

accessToken and refreshToken.

o Copy accessToken to https://jwt.ms. Verify claims: sub, email,

role, and exp \(15 min\).

2. **Refresh Token Rotation Check:**

o Send POST /api/auth/refresh with refreshToken. Confirm a new

accessToken and new refreshToken are returned.

3. **Theft Revocation Test:**

o Resend the *old* \(now used\) refresh token to POST

/api/auth/refresh.

o Confirm 401 Unauthorized \(“Token theft detected. All user sessions

revoked.”\).

o Verify in database that all RefreshTokens for that user have

IsRevoked = 1.

Module 11 Lab Session 3: Policy Authorization, Angular Guards & Security Hardening **Session** 3 of 3

**Exercises** Exercises **5**, **6**, and **7** **Integrity Lab tier Tier 3** — Policy authorization, custom AuthorizationHandler,

Angular role guards, rate limiting, security headers, final audit

 

Story thread

Role-based authorization \(\[Authorize\(Roles = "Instructor"\)\]\) is insufficient when an instructor attempts to modify a course taught by someone else. You will build a **Resource-Based Authorization Handler** ensuring instructors can only modify courses where they are assigned as lead instructor. Then, you will implement Angular roleGuard and structural template checks for clean client UX, and finish by hardening the API against DDoS and browser attacks using rate limiting and security headers middleware.

Exercise 5: Resource-Based Authorization \(Course Ownership\) Step 1: Define Custom Authorization Requirement Create Authorization/CourseInstructorRequirement.cs: **using** Microsoft.AspNetCore.Authorization; **namespace** Tms.Api.Authorization; **public class** CourseInstructorRequirement : IAuthorizationRequirement \{ \}

 

Step 2: Implement Authorization Handler

Create Authorization/CourseInstructorHandler.cs: **namespace** Tms.Api.Authorization; **public class** CourseInstructorHandler : AuthorizationHandler<CourseInstructorRequirement, Course> \{

**protected override** Task HandleRequirementAsync\(

AuthorizationHandlerContext context, CourseInstructorRequirement requirement, Course resource\)

\{

var userId =

context.User.FindFirstValue\(ClaimTypes.NameIdentifier\);

var isInstructor = context.User.IsInRole\("Instructor"\); var isAdmin = context.User.IsInRole\("Admin"\); *// Admins can manage any course*

**if** \(isAdmin\)

\{

context.Succeed\(requirement\); **return** Task.CompletedTask;

\}

*// Instructors can only manage courses where InstructorId*

*matches their User ID*

**if** \(isInstructor && resource.InstructorId == userId\) \{

context.Succeed\(requirement\);

\}

**return** Task.CompletedTask;

\}

\}

 

Step 3: Register Policy in Program.cs

In Program.cs:

builder.Services.AddAuthorizationBuilder\(\)

.AddPolicy\("CanEditCourse", policy =>

policy.Requirements.Add\(**new** CourseInstructorRequirement\(\)\)\);

builder.Services.AddSingleton<IAuthorizationHandler, CourseInstructorHa ndler>\(\);

Step 4: Enforce Policy & Roles in Controller

In Controllers/CourseController.cs, inject IAuthorizationService and enforce the policy:

**namespace** Tms.Api.Controllers;

\[Authorize\(Roles = "Instructor,Admin"\)\]

\[ApiController\]

\[Route\("api/\[controller\]"\)\]

**public class** CourseController : ControllerBase \{

**private readonly** TmsDbContext \_context;

**private readonly** IAuthorizationService \_authorizationService;

**public** CourseController\(TmsDbContext context, IAuthorizationService authorizationService\)

\{

\_context = context;

\_authorizationService = authorizationService;

\}

\[HttpPut\("\{id\}"\)\]

**public** async Task<IActionResult> UpdateCourse\(int id, \[FromBody\] UpdateCourseDto dto\)

\{

var course = await \_context.Courses.FindAsync\(id\); **if** \(course == **null**\) **return** NotFound\(\); var authResult = await

\_authorizationService.AuthorizeAsync\(User, course, "CanEditCourse"\);

**if** \(\!authResult.Succeeded\) \{

**return** Forbid\(\); *// 403 Forbidden when caller doesn't own*

*the resource*

\}

course.Title = dto.Title; await \_context.SaveChangesAsync\(\); **return** NoContent\(\); \}

\}

 

Exercise 6: Angular Route Protection & Interceptors Step 1: Update AuthService for Bearer Tokens Update src/app/services/auth.service.ts \(first built in M10 Session 2\) to store the JWT access token in memory, decode user profile claims, and expose getAccessToken\(\):

**export interface** TmsUser \{

email: string;

displayName: string;

role: string;

\}

**export interface** LoginRequest \{

email: string;

password: string;

\}

**export interface** AuthResponse \{

accessToken: string;

refreshToken: string;

\}

@Injectable\(\{ providedIn: 'root' \}\) **export class** AuthService \{

**private** http = inject\(HttpClient\);

**private** accessToken = signal<string | null>\(**null**\);

currentUser = signal<TmsUser | null>\(**null**\);

getAccessToken\(\): string | null \{

**return this**.accessToken\(\);

\}

hasRole\(role: string\): boolean \{

**const** user = **this**.currentUser\(\);

**return** user?.role === role || user?.role === 'Admin';

\}

**async** login\(credentials: LoginRequest\): Promise<void> \{

**const** res = **await** firstValueFrom\(

**this** .http.post<AuthResponse>\('/api/auth/login', credentials\)

\);

**this** .accessToken.set\(res.accessToken\);

*// Decode user payload from JWT \(or fetch /api/auth/me\)*

**const** payload = JSON.parse\(atob\(res.accessToken.split\('.'\)\[1\]\)\);

**this** .currentUser.set\(\{

email: payload.email || payload.sub, displayName: payload.name || payload.email || 'User', role:

payload\['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'\] || payload.role || 'Student'

\}\);

\}

logout\(\): void \{

**this** .accessToken.set\(**null**\);

**this** .currentUser.set\(**null**\);\}\}

Step 2: Functional Role Guard

Create src/app/guards/role.guard.ts: **export const** roleGuard = \(requiredRole: string\): CanActivateFn **=>** \{

**return** \(\) **=>** \{

**const** auth = inject\(AuthService\);

**const** router = inject\(Router\);

**if** \(auth.hasRole\(requiredRole\)\) \{

**return true**;

\}

**return** router.createUrlTree\(\["/unauthorized"\]\);

\};

\};

Apply in app.routes.ts:

\{

path: 'admin/courses',

component: AdminCourseListComponent,

canActivate: \[roleGuard\('Admin'\)\]

\}

 

Step 3: Conditional UI in Templates

Hide administrative buttons from non-admin users:

@if \(auth.hasRole\('Admin'\)\) \{

<**button** \(click\)="deleteCourse\(course.id\)" class="btn-danger">

Delete Course

</**button**>

\}

 

Step 4: HTTP Auth Interceptor

Create src/app/interceptors/jwt.interceptor.ts: **export const** jwtInterceptor: HttpInterceptorFn = \(req, next\) **=>** \{

**const** auth = inject\(AuthService\);

**const** token = auth.getAccessToken\(\);

**if** \(token\) \{

**const** cloned = req.clone\(\{

setHeaders: \{ Authorization: \`Bearer $\{token\}\` \}

\}\);

**return** next\(cloned\);

\}

**return** next\(req\);

\};

 

Exercise 7: Security Hardening \(Rate Limiting & Headers\) Step 1: Rate Limiting in .NET API

In Program.cs:

builder.Services.AddRateLimiter\(options => \{

options.AddFixedWindowLimiter\("AuthLimiter", opt =>

\{

opt.PermitLimit = 5;

opt.Window = TimeSpan.FromMinutes\(1\); opt.QueueLimit = 0;

\}\);

\}\);

*// After UseRouting: if you don’t have it already* app.UseRateLimiter\(\);

Apply to AuthController.cs:

\[EnableRateLimiting\("AuthLimiter"\)\] \[HttpPost\("login"\)\]

**public** async Task<IActionResult> Login\(\[FromBody\] LoginRequest request\)

 

Step 2: Security Response Headers Middleware Add security headers to responses in Program.cs: app.Use\(async \(context, next\) => \{

context.Response.Headers.Append\("X-Content-Type-Options", "nosniff"\);

context.Response.Headers.Append\("X-Frame-Options", "DENY"\);

context.Response.Headers.Append\("Referrer-Policy", "strict-origin-when-cross-origin"\);

context.Response.Headers.Append\(

"Content-Security-Policy", "default-src 'self'; script-src 'self'; style-src 'self'

'unsafe-inline';"\);

await next\(\);

\}\);

 

Final Security Sprint Verification

Verify your hardened TMS pipeline:

☐ **Auth & Lockout:** 5 failed logins triggers 423 Locked.

☐ **JWT Bearer:** API requires Authorization: Bearer <token> header on

protected routes.

☐ **Resource Policy:** Instructor A receives 403 Forbidden attempting to edit

Instructor B’s course.

☐ **Angular Guard & UI:** Unauthorized user attempting /admin/courses

routes to /unauthorized.

☐ **Rate Limiting:** Exceeding 5 login attempts/min triggers 429 Too Many

Requests.

☐ **Security Headers:** Response headers include X-Frame-Options: DENY and

X-Content-Type-Options: nosniff.



Module 12 Lab Session 2: Frontend Vitest, Playwright E2E & Business Rule Sprint **Session** 2 of 2

**Exercises** Exercises **4**, **5**, and **6**

**Integrity** **Tier 2** Vitest Angular specs, Playwright E2E with auth storageState, **Lab tier** MaxEnrollmentsPerStudent business rule

 

Story thread

Session 1 pinned the .NET logic and the HTTP pipeline. Session 2 turns the safety net toward the user-facing surface: Angular components and stores, full-browser user journeys, and a new business rule \(MaxEnrollmentsPerStudent\) added with a guiding test.

Exercise 4: Angular Component and Store Tests \(Vitest\) **Context:** Your Angular components and SignalStores form the UI layer of the TMS. A broken component template or wrong computed signal calculation breaks the user experience even if the C\# API is perfect. In this exercise, you will write Vitest specs for Angular components and NgRx SignalStores. Part A: Component Spec \(course-card.component.spec.ts\) CourseCardComponent \(M8\) uses routerLink in its template. Components that touch the router need provideRouter\(\[\]\) so the ActivatedRoute and RouterLink directives resolve. Without it, the test fails with NG0201: No provider found for ActivatedRoute.

Create src/app/ui/course-card/course-card.component.spec.ts: describe\("CourseCardComponent", \(\) **=>** \{

beforeEach\(\(\) **=>** \{

TestBed.configureTestingModule\(\{

providers: \[provideRouter\(\[\]\)\],

\}\);

\}\);

it\("should display the course title", **async** \(\) **=>** \{

**const** fixture = TestBed.createComponent\(CourseCardComponent\); Part B: HTTP Mock Spec \(enrollment.service.spec.ts\) M9’s EnrollmentService issues real HTTP calls. Use provideHttpClientTesting\(\) \+ HttpTestingController instead of mocking HttpClient directly. Create src/app/services/enrollment.service.spec.ts: describe\("EnrollmentService", \(\) **=>** \{

*// Set signal-based required input*

fixture.componentRef.setInput\("course", \{

id: 1,

code: "CSE-101",

title: "Advanced Web Dev", maxCapacity: 30,

enrollmentCount: 12,

\}\);

**await** fixture.whenStable\(\);

**const** el = fixture.nativeElement **as** HTMLElement;

expect\(el.textContent\).toContain\("Advanced Web Dev"\);

\}\);

it\("should emit enrollClicked event when button is clicked", **async** \(\)

**=>** \{

**const** fixture = TestBed.createComponent\(CourseCardComponent\);

**const** component = fixture.componentInstance;

fixture.componentRef.setInput\("course", \{

id: 1,

code: "CSE-101",

title: "Advanced Web Dev", maxCapacity: 30,

enrollmentCount: 12,

\}\);

**await** fixture.whenStable\(\);

**let** emittedCourse: any = **null**;

component.enrollClicked.subscribe\(\(c: any\) **=>** \(emittedCourse = c\)\);

**const** button = fixture.nativeElement.querySelector\(

"button",

\) **as** HTMLButtonElement;

button.click\(\);

**await** fixture.whenStable\(\);

expect\(emittedCourse\).toBeTruthy\(\);

expect\(emittedCourse.title\).toBe\("Advanced Web Dev"\);

\}\);

\}\);

**let** httpMock: HttpTestingController;

**let** service: EnrollmentService;

beforeEach\(\(\) **=>** \{

TestBed.configureTestingModule\(\{

providers: \[provideHttpClient\(\), provideHttpClientTesting\(\)\],

\}\);

httpMock = TestBed.inject\(HttpTestingController\);

service = TestBed.inject\(EnrollmentService\);

\}\);

afterEach\(\(\) **=>** httpMock.verify\(\)\);

it\("getAll\(\) issues GET /api/enrollments and maps the response",

**async** \(\) **=>** \{

**const** result = firstValueFrom\(service.getAll\(\)\);

**const** req = httpMock.expectOne\(\(r\) **=>** r.url.endsWith\("/api/enrollments"\)\);

expect\(req.request.method\).toBe\("GET"\);

req.flush\(\[

\{ id: 1, studentId: 11, studentName: "Abeba", courseId: 101,

courseName: "Intro to CS", status: "Pending", enrolledAt: "2026-08-12T10:00:00Z" \},

\{ id: 2, studentId: 12, studentName: "Kebede", courseId: 102,

courseName: "Data Structures", status: "Approved", enrolledAt: "2026-08-12T10:05:00Z" \},

\]\);

**const** enrollments = **await** result;

expect\(enrollments\).toHaveLength\(2\);

expect\(enrollments\[0\].courseName\).toBe\("Intro to CS"\);

\}\);

it\("approve\(id\) issues POT /api/enrollments/\{id\}/approve", **async** \(\)

**=>** \{

**const** result = firstValueFrom\(service.approve\(42\)\);

**const** req = httpMock.expectOne\(\(r\) **=>** r.url.endsWith\("/api/enrollments/42/approve"\)\);

expect\(req.request.method\).toBe\("POT"\);

req.flush\(\{

id: 42,

studentId: 11,

studentName: "Abeba",

courseId: 101,

courseName: "Intro to CS", status: "Approved",

enrolledAt: "2026-08-12T10:00:00Z",

\}\);

**const** approved = **await** result;

expect\(approved.status\).toBe\("Approved"\);

\}\);

\}\);

Run Frontend Tests

Run npm test and confirm both specs pass. Verification Checkpoint \(Exercise 4\)

☐ Component spec sets signal inputs using

fixture.componentRef.setInput\(\)

☐ Spec waits for DOM stability using await fixture.whenStable\(\)

☐ Output event subscription captures emitted course object

☐ EnrollmentStore spec verifies entities\(\) length \+ first row’s courseName

after store.seed\(...\)

☐ EnrollmentStore spec verifies pendingCount\(\) computed signal returns

the right count

☐ EnrollmentService.getAll\(\) spec asserts GET /api/enrollments against

HttpTestingController

☐ EnrollmentService.approve\(id\) spec asserts PUT

/api/enrollments/\{id\}/approve



Exercise 6: Playwright E2E

Unit and integration tests verify logic and the HTTP pipeline. This exercise verifies the **user facing surface** in a real browser, including semantic locators, auth reuse, and a failure-path story. This work subsumes the original retired Sessions 3 and 4. Step 1: Install Playwright

npm init playwright@latest --legacy-peer-deps

Step 2: Auth Setup \(reuse across tests\)

The auth setup below navigates to /login, fills inputs by their accessible

labels \(Username, Password\), clicks a button named Sign In, and waits

for a heading whose name matches /command center/i. *// app.routes.ts \(routes table that M9 left empty\)* **export const** routes: Routes = \[

\{ path: "login", component: LoginComponent \},

\{ path: "command-center", component: InstructorDashboardComponent, ca

nActivate: \[authGuard\] \},

\{ path: "\*\*", redirectTo: "login" \},

\];

*<\!-- login.component.html minimal shape labels and button name MUST ma tch -->*

<**form** \(submit\)="submit\($event\)">

<**label**>Username <**input** name="username" /></**label**>

<**label**>Password <**input** name="password" type="password" /></**label**>

<**button** type="submit">Sign In</**button**>

</**form**>

The auth.setup heading regex /command center/i matches the

<h1>Instructor Command Center</h1> that M9’s

InstructorDashboardComponent already renders. If your cohort

customised the heading text, update the regex in auth.setup.ts \(and

the workbook / essentials copies\) to match. Without a /login route,

page.goto\("/login"\) 404s and the setup hangs on expect.toBeVisible,

but the auth API may still return 200, so the test is correctly catching a

routing gap, not a flaky runner.

Create e2e/auth.setup.ts:

**import** \{ test **as** setup, expect \} **from** "@playwright/test"; setup\("authenticate as admin", **async** \(\{ page \}\) **=>** \{

**await** page.goto\("/login"\);

*// M10 & M12 baseline LoginRequest uses Email \(with Username supporte d as fallback\).*

**await** page.getByLabel\(/email|username/i\).fill\(process.env.TMS\_ADMIN\_E

MAIL ?? process.env.TMS\_ADMIN\_USER\!\);

**await** page.getByLabel\("Password"\).fill\(process.env.TMS\_ADMIN\_PASS\!\);

**await** page.getByRole\("button", \{ name: "Sign In" \}\).click\(\);

*// M9's InstructorDashboardComponent renders <h1>Instructor Command C*

*enter</h1>; the*

*// regex matches that heading so we know post-login navigation resolv*

*ed the protected route.*

**await** expect\(page.getByRole\("heading", \{ name: /command center/i \}\)\).

toBeVisible\(\);

**await** page.context\(\).storageState\(\{ path: "playwright/.auth/admin.jso

n" \}\);

\}\);

Wire it up in playwright.config.ts: projects: \[

\{ name: "setup", testMatch: /.\*\\.setup\\.ts/ \},

\{

name: "tests",

dependencies: \["setup"\],

use: \{ storageState: "playwright/.auth/admin.json" \},

\},

\],

**Security rule:** playwright/.auth/ contains session cookies. Confirm it is in .gitignore \(it should already be from the pre-work checklist\). Never commit storageState to version control.

Step 3: Happy-Path E2E Spec

Create e2e/admin-approve-enrollment.spec.ts: **import** \{ test, expect \} **from** "@playwright/test"; test\("admin approves a pending enrollment", **async** \(\{ page \}\) **=>** \{

**await** page.goto\("/dashboard"\);

*// The dashboard heading text comes from M9's InstructorDashboardComp*

*onent template*

*// \("Instructor Command Center"\); the spec's regex matches that exact*

*ly so a future*

*// copy edit doesn't silently break the auth-setup handoff.*

**await** expect\(page.getByRole\("heading", \{ name: /command center/i \}\)\).

toBeVisible\(\);

*// M9's EnrollmentListComponent renders a per-row "Approve" button on ly when the*

*// enrollment is still Pending. We click the first one and assert the*

*optimistic*

*// status flip from M9's EnrollmentStore shows up in the row's badge.*

**const** firstApprove = page.getByRole\("button", \{ name: "Approve" \}\).fi

rst\(\);

**await** firstApprove.click\(\);

*// The row's status badge flips to "Approved" instantly no navigation*

*needed.*

**await** expect\(page.getByText\("Approved"\).first\(\)\).toBeVisible\(\);

\}\);

 

Run the E2E Suite

npx playwright test

Final QA Verification Checklist

Before completing Module 12 , run all test suites across the solution to ensure full safety net coverage:

1. **.**NET Backend Unit & Integration Tests:

dotnet test

2. Angular Vitest Suite:

npm test

3. Playwright E2E Suite:

npx playwright test

Verification Checkpoint \(Exercise 6\)

☐ Playwright installed and configured

☐ Setup project executes and saves storageState

☐ auth.setup.ts uses getByLabel\(/email|username/i\) \(matching M10

LoginRequest\)

☐ auth.setup.ts reads TMS\_ADMIN\_USER \(or TMS\_ADMIN\_EMAIL alias\) and waits

for the /command center/i heading from M9’s dashboard

☐ Happy-path E2E spec clicks the M9 Approve button \(Pending row\) and

asserts the row flips to Approved



Module 12 Lab Session 1: .NET Unit, Mocking, and

Integration Testing

**Session** 1 of 2

 

**Exercises** Exercises **1**, **2**, and **3**

 

**Integrity** **Tier 1** xUnit \[Fact\]/\[Theory\] boundary testing, NSubstitute mocks, **Lab tier** WebApplicationFactory API contract assertions

 

Story

A student double-clicked **Submit Enrollment** and created duplicate rows. No test failed because no automated test existed. You will build the unit and integration test safety net that catches bugs like this in C\# domain logic and the HTTP pipeline before they reach production.

Exercise 1: The Pure Logic Test \(xUnit\)

The TMS stores per-enrollment grades as a decimal \(M5’s Enrollment.Grade\) and per-assessment maximums as decimal MaxScore \(M5’s Assessment\). Letter grades do not exist yet, you will add a small GradingService to TmsApi.Application that maps a \(score, maxScore\) pair to a GradeLevel. Unit tests pin those rules, so future refactoring cannot break grading logic silently. This is a deliberate TMS project push during M12: by the end of Exercise 1 you ship one new service plus its tests.

Step 1: Create the Test Project

Open a terminal in your TMS API solution directory:

dotnet new xunit-n TmsApi.Tests

dotnet sln add TmsApi.Tests

cd TmsApi.Tests

dotnet add reference ../TmsApi.Application/TmsApi.Application.csproj \(Add TmsApi.Application as a project reference. TmsApi.Application is where IEnrollmentService already lives per the M7 Session 0 refactor; the new GradingService belongs here too.\)

Step 2: Add GradingService to the TMS project

Create TmsApi.Application/Grading/GradeLevel.cs: **namespace** TmsApi.Application.Grading;

**public enum** GradeLevel

\{

Distinction,

Pass,

Fail,

Invalid,

\}

Create TmsApi.Application/Grading/GradingService.cs: **namespace** TmsApi.Application.Grading;

**public class** GradingService

\{

**public** const decimal DistinctionThreshold = 70m; **public** const decimal PassThreshold = 50m;

*// Pure mapping: one score against one maximum. // Uses M5's Assessment.MaxScore and the decimal part of*

*//Enrollment.Grade.*

**public** GradeLevel CalculateLetterGrade\(decimal score, decimal maxScore\)

\{

**if** \(maxScore <= 0m || score < 0m || score > maxScore\)

**return** GradeLevel.Invalid;

var pct = score / maxScore \* 100m; **return** pct >= DistinctionThreshold ? GradeLevel.Distinction

: pct >= PassThreshold ? GradeLevel.Pass : GradeLevel.Fail;

\}

*// Single-decimal path: maps an Enrollment.Grade percentage to a*

*GradeLevel.*

*// Enrollment.Grade is nullable per the M5 entity; null => Invalid.* **public** GradeLevel CalculateFromEnrollmentGrade\(decimal?

enrollmentGradePercent\)

\{

**if** \(enrollmentGradePercent **is null**\) **return** GradeLevel.Invalid; **return** CalculateLetterGrade\(enrollmentGradePercent.Value, maxScore:

100m\);

\}

\}

**What these exercises:** CalculateLetterGrade consumes the existing

Assessment shape from M5 \(decimal MaxScore, decimal Weight\).

CalculateFromEnrollmentGrade consumes the existing Enrollment.Grade

decimal. No new domain model, only a new application layer service that

interprets already stored data.

Step 3: Write the First Fact Test

Create GradingServiceTests.cs in TmsApi.Tests: **using** TmsApi.Application.Grading;

**namespace** TmsApi.Tests;

**public class** GradingServiceTests

\{

\[Fact\]

**public** void CalculateLetterGrade\_HighScore\_ReturnsDistinction\(\)

\{

*// Arrange*

var service = **new** GradingService\(\); *// Act*

var result = service.CalculateLetterGrade\(score: 85m, maxScore: 100m\); *// Assert*

Assert.Equal\(GradeLevel.Distinction, result\);

\}

\}

Step 4: Add Parameterized Theory Tests

Add boundary cases to GradingServiceTests.cs: \[Theory\]

\[InlineData\(0, 100, GradeLevel.Fail\)\] *// Boundary: zero score* \[InlineData\(70, 100, GradeLevel.Distinction\)\] *// Boundary: at distinction //threshold*

\[InlineData\(50, 100, GradeLevel.Pass\)\] *// Boundary: at pass threshold*

\[InlineData\(-1, 100, GradeLevel.Invalid\)\] *// Boundary: negative score*

\[InlineData\(101, 100, GradeLevel.Invalid\)\] *// Boundary: score exceeds max*

\[InlineData\(50, 0, GradeLevel.Invalid\)\] *// Boundary: zero max score //\(undefined percentage\)*

**public** void CalculateLetterGrade\_VariousInputs\_ReturnsExpectedLevel\(

decimal score, decimal maxScore, GradeLevel expected\)

\{

var service = **new** GradingService\(\); var result = service.CalculateLetterGrade\(score, maxScore\); Assert.Equal\(expected, result\);

\}

Step 5: Run the Tests

dotnet test

**Expected result:** All tests pass. If the grading logic has bugs, one or more tests fail. Fix the production code, never dilute the test assertions.

Exercise 2: Mocking External Boundaries \(NSubstitute\)

**Context:** Your M7 handler EnrollStudentHandler depends on the application-layer interface IEnrollmentService. To test the handler’s behaviour \(the place where route logic, validation, and authorization live\) without hitting the database, you mock IEnrollmentService and feed the handler the fake.

Step 1: Install NSubstitute

In your TmsApi.Tests directory:

dotnet add package NSubstitute

Step 2: Write the Mock Test

Create EnrollStudentHandlerTests.cs: **using** NSubstitute;

**using** TmsApi.Application.Interfaces; **using** TmsApi.Application.Enrollments.Commands; **using** TmsApi.Application.Common;

**using** TmsApi.Domain.Entities;

**namespace** TmsApi.Tests;

**public class** EnrollStudentHandlerTests \{

\[Fact\]

**public** async Task Handle\_WhenAlreadyEnrolled\_ReturnsDuplicateError\(\)

\{

*// Arrange: create a mock IEnrollmentService \(Application-layer interface\)*

var enrollmentService = Substitute.For<IEnrollmentService>\(\); var courseService = Substitute.For<ICourseService>\(\);

enrollmentService

.ExistsAsync\(99, "CS-401", Arg.Any<CancellationToken>\(\)\) .Returns\(Task.FromResult\(**true**\)\);

*// Course lookup runs first in the handler; return any non-null*

*// course so the duplicate check is the branch under test.*

var course = **new** Course

\{

Id = 1,

Code = "CS-401",

Title = "Advanced Web Dev", MaxCapacity = 30,

Enrollments = **new** List<Enrollment>\(\),

\};

courseService

.GetByCodeAsync\("CS-401", Arg.Any<CancellationToken>\(\)\) .Returns\(Task.FromResult<Course?>\(course\)\);

var handler = **new** EnrollStudentHandler\(enrollmentService, courseService\); var command=**new** EnrollStudentCommand\(StudentId: 99, CourseCode: "CS-401"\);

*// Act*

var result = await handler.Handle\(command, CancellationToken.None\);

*// Assert: handler surfaces the duplicate without touching the database. // Assert on the machine-readable Code \(the contract\) plus full record // equality, NOT on the human-readable Message,see M7 sealed-record pattern.*

Assert.False\(result.IsSuccess\); Assert.Equal\("already\_enrolled", result.Error.Code\); Assert.Equal\(EnrollmentError.AlreadyEnrolled\(99, "CS-401"\),

result.Error\);

*// The duplicate branch never writes — prove it.* await enrollmentService

.DidNotReceive\(\)

.AddAsync\(Arg.Any<Enrollment>\(\), Arg.Any<CancellationToken>\(\)\);

\}

\[Fact\]

**public** async Task Handle\_WhenCourseFull\_ReturnsCapacityError\(\)

\{

*// Arrange: course is at capacity \(Enrollments.Count >= MaxCapacity\).*

*// M7's handler checks capacity against the course object, not via service*

*// calls.*

var enrollmentService = Substitute.For<IEnrollmentService>\(\); var courseService = Substitute.For<ICourseService>\(\);

var course = **new** Course

\{

Id = 1,

Code = "CS-401",

Title = "Advanced Web Dev", MaxCapacity = 35,

Enrollments = Enumerable.Range\(1, 35\)

.Select\(i => **new** Enrollment \{ Id = i, CourseId = 1, Status =

"Pending"\}\)

.ToList\(\)\};

courseService

.GetByCodeAsync\("CS-401", Arg.Any<CancellationToken>\(\)\) .Returns\(Task.FromResult<Course?>\(course\)\); var handler= **new** EnrollStudentHandler\(enrollmentService, courseService\); var command = **new** EnrollStudentCommand\(StudentId:100, CourseCode: "CS-401"\);

*// Act*

var result = await handler.Handle\(command, CancellationToken.None\);

*// Assert: typed error matches the M7 sealed-record factory* Assert.False\(result.IsSuccess\); Assert.Equal\("course\_full", result.Error.Code\); Assert.Equal\(EnrollmentError.CourseFull\("Advanced Web Dev", 35\),

result.Error\);

await enrollmentService

.DidNotReceive\(\)

.AddAsync\(Arg.Any<Enrollment>\(\), Arg.Any<CancellationToken>\(\)\);

\}

\[Fact\]

**public** async Task Handle\_SuccessfulPath\_AddsEnrollmentOnce\(\)

\{

*// Arrange: course has room, student is not already enrolled; expect one*

*// AddAsync call.*

var enrollmentService = Substitute.For<IEnrollmentService>\(\); var courseService = Substitute.For<ICourseService>\(\);

var course = **new** Course

\{

Id = 1,

Code = "CS-401",

Title = "Advanced Web Dev", MaxCapacity = 35,

Enrollments = Enumerable.Range\(1, 20\)

.Select\(i => **new** Enrollment \{ Id = i, CourseId = 1, Status=

"Pending", \}\)

.ToList\(\),

\};

courseService

.GetByCodeAsync\("CS-401", Arg.Any<CancellationToken>\(\)\) .Returns\(Task.FromResult<Course?>\(course\)\);

enrollmentService

.ExistsAsync\(100, "CS-401", Arg.Any<CancellationToken>\(\)\) .Returns\(Task.FromResult\(**false**\)\);

var handler = **new** EnrollStudentHandler\(enrollmentService, courseService\);

var command =**new** EnrollStudentCommand\(StudentId: 100, CourseCode: "CS-401"\);

*// Act*

var result = await handler.Handle\(command, CancellationToken.None\);

*// Assert: handler produced a typed success payload with the right IDs*

Assert.True\(result.IsSuccess\); Assert.Equal\(100, result.Value.StudentId\); Assert.Equal\("CS-401", result.Value.CourseCode\);

*// The interaction: AddAsync called exactly once with a row that points at // the student and course*

await enrollmentService

.Received\(1\)

.AddAsync\(

Arg.Is<Enrollment>\(e => e.StudentId == 100 && e.CourseId ==

1\),

Arg.Any<CancellationToken>\(\)\);

\}

\}

**Why this version:** EnrollStudentHandler \(M7’s MediatR handler\) takes IEnrollmentService and ICourseService , both Application-layer interfaces. Mocking both keeps the handler testable without spinning up an EF in-memory provider. The handler’s real branches map cleanly to three tests: duplicate enrollment, course-full, and successful write each test asserts the typed Result plus a Received\(\) / DidNotReceive\(\) interaction so a future refactor cannot silently break the boundary.

Exercise 3: Integration Testing \(WebApplicationFactory\)

Unit tests verify C\# logic in isolation. Integration tests verify that the entire ASP.NET Core pipeline, routing, middleware, controllers, model binding, and JSON serialization, operates correctly together under HTTP requests.

Step 1: Install Integration Testing Packages

In your TmsApi.Tests directory, install Microsoft.AspNetCore.Mvc.Testing, the EF Core in-memory provider, and reference the API project:

dotnet add package Microsoft.AspNetCore.Mvc.Testing dotnet add package Microsoft.EntityFrameworkCore.InMemory dotnet add reference ../TmsApi.Api/TmsApi.Api.csproj

**Why** **InMemory****?** WebApplicationFactory boots the **real** ASP.NET Core pipeline.

Your API’s TmsDbContext needs a database — but the test host has no

connection string. Swapping in UseInMemoryDatabase gives the pipeline a

lightweight store without external infrastructure.

Step 2: Make Program Accessible to Test Project

Open TmsApi.Api/TmsApi.Api.csproj \(in your API startup project\) and add InternalsVisibleTo or expose Program: <**ItemGroup**>

<**AssemblyAttribute**

Include="System.Runtime.CompilerServices.InternalsVisibleTo">

<**\_Parameter1**>TmsApi.Tests</**\_Parameter1**>

</**AssemblyAttribute**>

</**ItemGroup**>

Alternatively, at the bottom of Program.cs in TmsApi.Api: **public partial class** Program \{ \}

Step 3: Create a Custom WebApplicationFactory

Create CustomWebApplicationFactory.cs in TmsApi.Tests: **using** Microsoft.AspNetCore.Hosting; **using** Microsoft.AspNetCore.Mvc.Testing; **using** Microsoft.EntityFrameworkCore; **using** Microsoft.Extensions.Configuration; **using** Microsoft.Extensions.DependencyInjection; **using** Microsoft.Extensions.DependencyInjection.Extensions; **using** TmsApi.Infrastructure.Persistence;

**namespace** TmsApi.Tests;

**public class** CustomWebApplicationFactory : WebApplicationFactory<Program> \{

**protected override** void ConfigureWebHost\(IWebHostBuilder builder\)

\{

*// 1. Supply required test configuration \(JWT secret, etc.\)*

builder.ConfigureAppConfiguration\(\(context, config\) => \{

config.AddInMemoryCollection\(**new** Dictionary<string, string?> \{

\["Jwt:Key"\] = "ThisIsASecretKeyForTestingPurposesOnly123456\!", \["Jwt:Secret"\] =

"ThisIsASecretKeyForTestingPurposesOnly123456\!",

\["Jwt:Issuer"\] = "TmsTestIssuer", \["Jwt:Audience"\] = "TmsTestAudience"

\}\);

\}\);

*// 2. Remove production DbContext and register InMemory with isolated internal provider*

builder.ConfigureServices\(services => \{

services.RemoveAll<DbContextOptions<TmsDbContext>>\(\); services.RemoveAll<DbContextOptions>\(\); services.RemoveAll<TmsDbContext>\(\);

var inMemoryProvider = **new** ServiceCollection\(\)

.AddEntityFrameworkInMemoryDatabase\(\) .BuildServiceProvider\(\);

services.AddDbContext<TmsDbContext>\(options => \{

options.UseInMemoryDatabase\("TmsTestDb"\); options.UseInternalServiceProvider\(inMemoryProvider\);

\}\);

\}\);

\}

\}

**What this does:**

1. Injects in-memory configuration keys for Jwt:Key so authentication middleware doesn’t throw ArgumentNullException when reading missing appsettings/user secrets.

2. Uses .AddEntityFrameworkInMemoryDatabase\(\) with .UseInternalServiceProvider\(\) to isolate EF Core’s in-memory services from production database provider registrations \(such as PostgreSQL/Npgsql\).

Step 5: Write the API Integration Test

Create CoursesApiTests.cs in TmsApi.Tests: **using** System.Net;

**using** System.Net.Http.Json;

**namespace** Tms.Tests;

**public class** CoursesApiTests : IClassFixture<CustomWebApplicationFactory> \{

**private readonly** HttpClient \_client;

**public** CoursesApiTests\(CustomWebApplicationFactory factory\)

\{

\_client = factory.CreateClient\(\);

\}

\[Fact\]

**public** async Task GetCourses\_ReturnsOkAndPagedJson\(\)

\{

*// Act — pin the V2 URL \(see Versioning callout below\)* var response = await

\_client.GetAsync\("/api/v2.0/courses?page=1&pageSize=10"\);

*// Assert — check HTTP status 200 OK*

response.EnsureSuccessStatusCode\(\);

*// TMS API contract check: PagedResponse<T> with items array* var page = await

response.Content.ReadFromJsonAsync<PagedCoursesJson>\(\);

Assert.NotNull\(page?.Items\);

\}

\[Fact\]

**public** async Task CreateCourse\_InvalidCode\_ReturnsValidationError\(\)

\{

*// Act — post invalid payload \(empty code\) to the V2 controller* var response = await \_client.PostAsJsonAsync\("/api/v2.0/courses", **new** \{

code = "",

title = "Intro to TMS Security", maxCapacity = 30

\}\);

*// Assert — validation failure returns 400 Bad Request or 422*

*Unprocessable Entity*

Assert.True\(

response.StatusCode **is** HttpStatusCode.BadRequest or

HttpStatusCode.UnprocessableEntity\);

\}

**private sealed class** PagedCoursesJson

\{

**public** List<CourseRowJson> Items \{ **get**; **set**; \} = **default**\!; **public** int TotalCount \{ **get**; **set**; \}

\}

**private sealed class** CourseRowJson

\{

**public** int Id \{ **get**; **set**; \} **public** string Code \{ **get**; **set**; \} = ""; **public** string Title \{ **get**; **set**; \} = ""; **public** int MaxCapacity \{ **get**; **set**; \} **public** int EnrollmentCount \{ **get**; **set**; \}

\}

\}

Step 6: Run the Suite

dotnet test

**Expected result:** CustomWebApplicationFactory launches the API in memory with an in-memory database. Both integration tests pass: GET returns 200 OK with a paged response, and POST returns 400 Bad Request when Code is empty.

**Versioning : target the URL, assert on the contract \(M7 V1/V2\).** The TMS

API ships two controller trees, TmsApi.Api.Controllers.V1 and

TmsApi.Api.Controllers.V2. Their response envelopes differ: V1 returns a flat

\{ "error": "..." \} JSON, V2 returns RFC 9457 application/problem\+json

with type, title, detail, and extensions\["code"\]. Two rules keep your

integration tests from breaking every time someone bumps a version:

1. **Pin the URL.** Use /api/v2.0/courses?page=1&pageSize=10 \(not

/api/courses\) when the controller you care about lives in V2. Tests that hit

the unversioned root are at the mercy of the route table.

2. **Assert on the stable** **error.Code****, not the envelope shape.** The

EnrollmentError sealed record \(M7, TmsApi.Application.Common\) carries a

machine-readable Code \("already\_enrolled", "course\_full",

"course\_not\_found"\). The handler maps that code into whatever envelope

the controller version returns. Your assertion stays stable:

o *// Stable across V1 \(flat JSON\) and V2 \(ProblemDetails\)*

var body = await response.Content.ReadFromJsonAsync<JsonElement>\(\);

Assert.Equal\("already\_enrolled",

body.GetProperty\("extensions"\).GetProperty\("code"\).GetString\(\)\); *// V2*

*// — or for V1 —*

Assert.Equal\("already\_enrolled", body.GetProperty\("code"\).GetString\(\)\);

o Don’t assert on the human Message, the detail text, or the type URI. Those

are presentation, not contract.



Verification Checkpoint \(Session 1\)

☐ TmsApi.Tests project created and added to the solution, referencing

TmsApi.Application and TmsApi.Api

☐ TmsApi.Application/Grading/GradingService.cs and GradeLevel.cs added ☐ At least one \[Fact\] test for a specific grade scenario \(CalculateLetterGrade\(85,

100\) → Distinction\)

☐ At least one \[Theory\] with boundary values \(zero, distinction threshold, pass

threshold, negative, exceeds-max, zero-max\)

☐ IEnrollmentService NSubstitute installed and used to mock ☐ Microsoft.AspNetCore.Mvc.Testing installed ☐ WebApplicationFactory<Program> starts the API in memory ☐ 200 OK GET test verifies and items\[\] array in PagedResponse ☐ POST test verifies validation failure \(400 or 422\) on invalid payloads ☐ dotnet test runs 100% green



