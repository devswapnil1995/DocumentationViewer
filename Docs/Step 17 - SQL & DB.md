## What is SQL?

> SQL (Structured Query Language) is a programming language used to communicate with and manage relational databases.

With SQL, you can store, retrieve, update, and delete data from databases such as SQL Server, PostgreSQL, and MySQL.

For example, if you have an application that stores employee information, SQL allows you to find employees, add new employees, update their salaries, or remove records.

**SQL vs SQL Server**

These two terms are related but different:

`SQL`

A language used to query and manage relational databases.

`SQL Server`

A database management system developed by Microsoft that uses SQL and its own extensions, such as T-SQL.

Other popular relational database systems include:

* PostgreSQL — Open-source relational database.

* MySQL — Widely used relational database.

* Oracle Database — Enterprise relational database.
----------------
---------------

## Constraint

> A **constraint** in SQL is a rule applied to a table or column to maintain data accuracy, consistency, and integrity in a database.

For example, constraints can ensure that:

* Every employee has a unique ID.
* An employee's name cannot be NULL.
* An employee's salary cannot be negative.
* Every employee belongs to a valid department.

SQL constraints enforce these rules automatically.

**Types of SQL Constraints**

There are six commonly used types of constraints:

1. PRIMARY KEY
2. FOREIGN KEY
3. NOT NULL
4. UNIQUE
5. CHECK
6. DEFAULT

---

**`1. PRIMARY KEY Constraint`**

A **PRIMARY KEY** constraint uniquely identifies each row in a table.

### Example

```sql
CREATE TABLE Employees (
    EmployeeId INT PRIMARY KEY,
    Name VARCHAR(100),
    Salary DECIMAL(10, 2)
);
```

**Rules**

* A primary key cannot contain duplicate values.
* A primary key cannot contain NULL values.
* A table can have only one primary key constraint.
* A primary key can consist of multiple columns. This is called a composite primary key.

**Example Data**

| EmployeeId | Name    |
| ---------- | ------- |
| 1          | Swapnil |
| 2          | Rahul   |
| 1          | Priya   |

The third row violates the PRIMARY KEY constraint because `EmployeeId = 1` already exists.

---

**`2. FOREIGN KEY Constraint`**

A **FOREIGN KEY** constraint establishes a relationship between two tables.

It ensures that a value in one table refers to a valid key in another table.

**Example**

```sql
CREATE TABLE Departments (
    DepartmentId INT PRIMARY KEY,
    DepartmentName VARCHAR(100)
);

CREATE TABLE Employees (
    EmployeeId INT PRIMARY KEY,
    Name VARCHAR(100),
    DepartmentId INT,

    FOREIGN KEY (DepartmentId)
        REFERENCES Departments(DepartmentId)
);
```

Here:

* `Departments.DepartmentId` is the primary key.
* `Employees.DepartmentId` is the foreign key.
* The foreign key references `Departments.DepartmentId`.

If the department table contains IDs `10` and `20`, an employee cannot be inserted with `DepartmentId = 99`, because that department does not exist.

**Purpose**

A FOREIGN KEY maintains **referential integrity** and prevents invalid relationships between tables.

---

**`3. NOT NULL Constraint`**

A **NOT NULL** constraint ensures that a column cannot contain NULL values.

**Example**

```sql
CREATE TABLE Employees (
    EmployeeId INT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Salary DECIMAL(10, 2)
);
```

The following statement fails because `Name` cannot be NULL:

```sql
INSERT INTO Employees (EmployeeId, Name, Salary)
VALUES (1, NULL, 80000);
```

**Important Note**

`NOT NULL` prevents NULL values, but it does not prevent empty strings such as `''`.

For example:

```sql
INSERT INTO Employees (EmployeeId, Name, Salary)
VALUES (1, '', 80000);
```

The empty string is not NULL and may be accepted.

---

**`4. UNIQUE Constraint`**

A **UNIQUE** constraint ensures that values in a column or combination of columns are not duplicated.

**Example**

```sql
CREATE TABLE Employees (
    EmployeeId INT PRIMARY KEY,
    Email VARCHAR(150) UNIQUE
);
```

The following statements violate the UNIQUE constraint:

```sql
INSERT INTO Employees (EmployeeId, Email)
VALUES (1, 'swapnil@example.com');

INSERT INTO Employees (EmployeeId, Email)
VALUES (2, 'swapnil@example.com');
```

The second insert fails because the email address already exists.

**PRIMARY KEY vs UNIQUE**

| PRIMARY KEY                               | UNIQUE                                |
| ----------------------------------------- | ------------------------------------- |
| Uniquely identifies a row                 | Enforces uniqueness                   |
| Does not allow NULL values                | NULL handling depends on the database |
| Only one primary key constraint per table | Multiple UNIQUE constraints can exist |
| Used to identify records                  | Used to prevent duplicate values      |

**Important SQL Server Note**

In SQL Server, a UNIQUE constraint allows a single NULL value in a nullable column. The behavior of NULL values in UNIQUE constraints can differ across database systems.

---

**`5. CHECK Constraint`**

A **CHECK** constraint ensures that a value satisfies a specified condition.

**Example**

```sql
CREATE TABLE Employees (
    EmployeeId INT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Salary DECIMAL(10, 2),
    Age INT CHECK (Age >= 18),
    CHECK (Salary >= 0)
);
```

The rules are:

* Age must be at least 18.
* Salary must be greater than or equal to 0.

**Example**

```sql
INSERT INTO Employees (EmployeeId, Name, Salary, Age)
VALUES (1, 'Swapnil', -5000, 25);
```

The insert fails because the salary violates the CHECK constraint.

---

**`6. DEFAULT Constraint`**

A **DEFAULT** constraint provides an automatic value when an INSERT statement does not specify a value for a column.

**Example**

```sql
CREATE TABLE Employees (
    EmployeeId INT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);
```

**Insert Example**

```sql
INSERT INTO Employees (EmployeeId, Name)
VALUES (1, 'Swapnil');
```

The database automatically assigns:

* `IsActive = 1`
* `CreatedDate =` current date and time

**Important Note**

A DEFAULT value is generally used when a column is omitted from the INSERT statement. It does not normally replace an explicitly supplied NULL.

---

**Quick Comparison**

| Constraint  | Main Purpose                           |
| ----------- | -------------------------------------- |
| PRIMARY KEY | Uniquely identifies each row           |
| FOREIGN KEY | Maintains relationships between tables |
| NOT NULL    | Prevents NULL values                   |
| UNIQUE      | Prevents duplicate values              |
| CHECK       | Enforces a condition                   |
| DEFAULT     | Provides an automatic value            |

---

**Example Using Multiple Constraints**

```sql
CREATE TABLE Departments (
    DepartmentId INT PRIMARY KEY,
    DepartmentName VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE Employees (
    EmployeeId INT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Email VARCHAR(150) UNIQUE,
    Salary DECIMAL(10, 2) CHECK (Salary >= 0),
    Age INT CHECK (Age >= 18),
    DepartmentId INT,
    IsActive BIT DEFAULT 1,

    FOREIGN KEY (DepartmentId)
        REFERENCES Departments(DepartmentId)
);
```

This table uses multiple constraints:

* `PRIMARY KEY` — Identifies each employee uniquely.
* `NOT NULL` — Ensures the employee name is provided.
* `UNIQUE` — Prevents duplicate email addresses.
* `CHECK` — Ensures salary and age meet the defined rules.
* `DEFAULT` — Sets `IsActive` to 1 when omitted.
* `FOREIGN KEY` — Ensures the department exists.

> A primary key uniquely identifies each record in a table, whereas a unique key ensures that other important columns do not contain duplicate values. A table can have only one primary key but multiple unique constraints. A foreign key can reference either a primary key or a suitable unique key.

-------
-------

## Key in SQL

A key is a column or combination of columns used to:

* Identify a record uniquely.
* Prevent duplicate records.
* Establish relationships between tables.
* Maintain data integrity.

**`Super Key`**

A Super Key is a column or combination of columns that can uniquely identify every row in a table.

A super key may contain extra columns that are not required for uniqueness.

**Example**

Suppose we have the following employee data:

```
EmployeeId    Name       Email
-----------   --------   ----------------------
101           Swapnil    swapnil@example.com
102           Rahul      rahul@example.com
103           Priya      priya@example.com
```

Assume that `EmployeeId` and `Email` are unique.

The following are super keys:

```
EmployeeId
Email
EmployeeId + Name
EmployeeId + Department
Email + Name
EmployeeId + Email + Name
```

Why is `EmployeeId + Name` a super key?

Because `EmployeeId` alone is already unique.

Adding `Name` does not destroy uniqueness.

**Important point**

A super key can contain unnecessary columns.

There is no specific SQL command called `CREATE SUPER KEY`.

Super keys are a database theory concept. In SQL, uniqueness can be enforced using `PRIMARY KEY` or `UNIQUE` constraints.

--------


**`Candidate Key`**

A Candidate Key is a minimal super key.

It uniquely identifies every row, and no column can be removed from it without losing uniqueness.

In simple words:

> A candidate key is a possible choice for the primary key.

**Example**

Assume the following columns are unique:

```
EmployeeId
Email
EmployeeCode
```

Then all three can be candidate keys.

```
EmployeeId
Email
EmployeeCode
```

But the following is not a candidate key:

```
EmployeeId + Name
```

Why?

Because `EmployeeId` alone is already unique.

Therefore, `Name` is unnecessary.

So:

```
EmployeeId + Name
```

is a super key, but not a candidate key.

**Important point**

A candidate key must be:

* Unique.
* Minimal.
* Capable of identifying one row.

There is no specific SQL command called `CREATE CANDIDATE KEY`.

You implement candidate keys using a primary key or suitable unique constraints.

---------------
**`Alternate Key`**

An Alternate Key is a candidate key that was not selected as the primary key.

```
CREATE TABLE Employees (
    EmployeeId INT PRIMARY KEY,
    Email VARCHAR(150) UNIQUE,
    EmployeeCode VARCHAR(20) UNIQUE,
    Name VARCHAR(100)
);
```

Assume:

```
EmployeeId  -> Primary Key
Email       -> Candidate Key
EmployeeCode -> Candidate Key
```

Since `EmployeeId` was selected as the primary key:

```
Email
EmployeeCode
```

are alternate keys.

An alternate key is usually implemented using a `UNIQUE` constraint.

-------------------
**`Composite Key`**

A Composite Key is a key made up of two or more columns.

The combination of these columns uniquely identifies a row.

The individual columns do not necessarily have to be unique.

This is one of the most important SQL interview concepts.

**Real-World Example of a Composite Key**

Imagine a college with students and courses.

* One student can enroll in multiple courses.
* One course can have multiple students.

We need a table to store student enrollments.

Example data:

```
StudentId    CourseId    EnrollmentDate
---------    --------    --------------
1            101         2026-09-01
1            102         2026-09-01
2            101         2026-09-02
2            103         2026-09-02
```

Let's analyze this data.

**Is StudentId unique?**

No.

Student 1 appears in two courses:

```
StudentId = 1
```

**Is CourseId unique?**

No.

Course 101 has two students:

```
CourseId = 101
```

**Is StudentId + CourseId unique?**

Yes.

The combination identifies a particular enrollment.

```
StudentId = 1, CourseId = 101
```

means:

```
Student 1 is enrolled in Course 101.
```

Similarly:

```
StudentId = 1, CourseId = 102
```

means:

```
Student 1 is enrolled in Course 102.
```

Therefore:

```
StudentId + CourseId
```

is a composite key.

**Why is StudentId + CourseId a Candidate Key?**

A candidate key must be minimal.

Let's check:

```
StudentId
```

alone is not unique.

```
CourseId
```

alone is not unique.

But:

```
StudentId + CourseId
```

together are unique.

If we remove either column, uniqueness is lost.

Therefore:

```
(StudentId, CourseId)
```

is a minimal super key.

So it is a:

```
Composite Candidate Key
```

**How to Create a Composite Primary Key**

In SQL Server, you can create a composite primary key by listing multiple columns inside the `PRIMARY KEY` constraint.

SQL

```
CREATE TABLE StudentCourses (
    StudentId INT,
    CourseId INT,
    EnrollmentDate DATE,

    PRIMARY KEY (StudentId, CourseId)
);
```

Here:

```
(StudentId, CourseId)
```

is the composite primary key.

**What does this enforce?**

The database will not allow duplicate combinations of:

```
StudentId + CourseId
```
-----------
**`Composite UNIQUE Key`**

A composite key does not have to be a primary key.

You can also create a composite `UNIQUE` constraint.

**Example**

Suppose a company wants to prevent duplicate employee names within the same department.

SQL

```
CREATE TABLE Employees (
    EmployeeId INT PRIMARY KEY,
    FirstName VARCHAR(50),
    LastName VARCHAR(50),
    DepartmentId INT,

    UNIQUE (FirstName, LastName, DepartmentId)
);
```

This ensures that the combination:

```
FirstName + LastName + DepartmentId
```

cannot be duplicated, subject to the database's NULL handling.

**Valid data**

```
EmployeeId    FirstName    LastName    DepartmentId
----------    ---------    ---------   ------------
1             Swapnil      Patil       10
2             Swapnil      Patil       20
3             Rahul        Sharma      10
```

This is valid because the combinations are different.

For example:

```
Swapnil + Patil + 10
Swapnil + Patil + 20
```

are two different combinations.

**Invalid data**

If this record already exists:

```
Swapnil + Patil + 10
```

then inserting another record with the same combination violates the composite unique constraint.

**Important point**

A composite key can be:

* A composite primary key.

* A composite candidate key.

* A composite alternate key.

* A composite unique constraint.

----------
**Composite Key vs Composite Primary Key**

These terms are related but not identical.

`Composite Key`

A key containing two or more columns.

Example:

```
StudentId + CourseId
```
--------------
`Composite Primary Key`

A composite key that has been selected as the primary key of a table.

Example:

SQL

```
CREATE TABLE StudentCourses (
    StudentId INT,
    CourseId INT,

    PRIMARY KEY (StudentId, CourseId)
);
```

**`Composite Candidate Key`**

A minimal combination of multiple columns that uniquely identifies a row.

Example:

```
(StudentId, CourseId)
```

If neither column is unique individually but the combination is unique, then it is a composite candidate key.

--------------
--------------

## Index

> An index in SQL is a database object that helps the database find rows faster.

An index works similarly to the index of a book:

* Without an index, you may need to scan every page to find a topic.

* With an index, you can directly locate the required page.

In the same way:

* Without an SQL index, the database may scan many or all rows.

* With an SQL index, the database can locate matching rows more efficiently.

**Example Without an Index**

Suppose we have an `Employees` table:

```SQL
CREATE TABLE Employees
(
    EmployeeId INT PRIMARY KEY,
    EmployeeName VARCHAR(100),
    DepartmentId INT,
    Salary DECIMAL(10, 2)
);
```

Now execute:

``` SQL
SELECT *
FROM Employees
WHERE DepartmentId = 10;
```

If there is no index on `DepartmentId`, the database may need to check each row:

```
Row 1 -> DepartmentId = 10?
Row 2 -> DepartmentId = 10?
Row 3 -> DepartmentId = 10?
Row 4 -> DepartmentId = 10?
...
```

This is called a table scan when the entire table is scanned.

For a table containing millions of rows, this can be slow.

**Example With an Index**

Create an index on `DepartmentId`:
```
CREATE INDEX IX_Employees_DepartmentId
ON Employees (DepartmentId);
```

Now the database has an additional structure that helps it find employees belonging to department `10`.

```SQL
SELECT *
FROM Employees
WHERE DepartmentId = 10;
```

The database can use the index to locate matching rows instead of checking every row in the table.

The actual index usage depends on factors such as:

* Number of rows in the table

* Number of matching rows

* Query structure

* Available indexes

* Statistics

* Database optimizer decisions

----------
**Why Do We Use Indexes?**

**1. Faster Data Retrieval**: Indexes improve the performance of queries that use:

**2. Faster Searching**: Indexes are useful when frequently searching by a particular column.

**3. Faster Sorting**: Indexes can help queries that use `ORDER BY`

**4. Faster Joins**: Indexes can improve queries that join tables using a particular column.

**5. Faster Grouping**: Indexes may help queries using `GROUP BY`

----------
**How to Create an Index**

The basic syntax is:

```
CREATE INDEX IndexName
ON TableName (ColumnName);
```

Example:

```
CREATE INDEX IX_Employees_DepartmentId
ON Employees (DepartmentId);
```

Naming convention commonly used in SQL Server:

```
IX_TableName_ColumnName
```

For example:

```
IX_Employees_DepartmentId
```

Here:

* `IX` means index.

* `Employees` is the table name.

* `DepartmentId` is the indexed column.

**How to Remove an Index**

In SQL Server:
```
DROP INDEX IX_Employees_DepartmentId
ON Employees;
```

In some other databases, the syntax is:
```
DROP INDEX IX_Employees_DepartmentId;
```

The exact syntax depends on the database system.

------------------------
**Types of Indexes**

**`1. Clustered Index`**

A clustered index determines the physical order in which table data is organized.

In SQL Server, a table can have only one clustered index because the table's data can have only one physical ordering.

```
CREATE CLUSTERED INDEX IX_Employees_EmployeeId
ON Employees (EmployeeId);
```

**Important points:**

* A table can have only one clustered index.

* The table data is organized according to the clustered index.

* A clustered index is often created on the primary key by default.

* The primary key is not automatically clustered in every database or every configuration.

Example:

```
CREATE TABLE Employees
(
    EmployeeId INT PRIMARY KEY CLUSTERED,
    EmployeeName VARCHAR(100),
    DepartmentId INT
);
```

**`2. Nonclustered Index`**

A nonclustered index is a separate structure that stores indexed column values and references to the corresponding table rows.


A table can have multiple nonclustered indexes. This is default index.

```
CREATE NONCLUSTERED INDEX IX_Employees_DepartmentId
ON Employees (DepartmentId);
```

This is useful when queries frequently search by `DepartmentId`.
```
SELECT *
FROM Employees
WHERE DepartmentId = 10;
```

**Clustered vs Nonclustered Index**

```
CLUSTERED INDEX
- Determines the order of the table's data.
- Only one clustered index is possible per table.
- Table data is organized according to the clustered index.
- Often used for the primary key.
- Useful for range-based access and ordered retrieval.

NONCLUSTERED INDEX
- Separate structure from the table data.
- Multiple nonclustered indexes can exist on one table.
- Stores indexed values and row references.
- Useful for searching, filtering, joining, and sorting.
- Requires additional storage.
```

**`3. Unique Index`**

A unique index ensures that duplicate values are not allowed in the indexed key.
```
CREATE UNIQUE INDEX UX_Employees_Email
ON Employees (Email);
```

This prevents two employees from having the same email value.
```
INSERT INTO Employees
(
    EmployeeId,
    EmployeeName,
    Email
)
VALUES
(
    1,
    'Swapnil',
    'swapnil@example.com'
);
```

A second row with the same email would fail because the index requires uniqueness.

A `UNIQUE` constraint can also enforce uniqueness:

```
ALTER TABLE Employees
ADD CONSTRAINT UQ_Employees_Email
UNIQUE (Email);
```

The database commonly creates a unique index internally to enforce the constraint.

**`4. Composite Index`**

A composite index is an index created on two or more columns.
```
CREATE INDEX IX_Employees_Department_Salary
ON Employees
(
    DepartmentId,
    Salary
);
```

This index contains two columns:

```
DepartmentId
Salary
```

It can help queries such as:

```
SELECT *
FROM Employees
WHERE DepartmentId = 10
  AND Salary > 50000;
```

It may also help:

```
SELECT *
FROM Employees
WHERE DepartmentId = 10;
```

However, its usefulness for a query involving only `Salary` may be limited because `DepartmentId` is the first column in the index.

**Composite Index Column Order**

The order of columns in a composite index is very important.

Consider this index:

```
CREATE INDEX IX_Employees_Department_Salary
ON Employees
(
    DepartmentId,
    Salary
);
```

The index is logically organized first by:

```
DepartmentId
```

Then within each department, by:
```
Salary
```

This index is generally useful for:
```
WHERE DepartmentId = 10
```

It is also useful for:
```
WHERE DepartmentId = 10
  AND Salary > 50000
```

It may be less useful for:
```
WHERE Salary > 50000
```

because the first indexed column, `DepartmentId`, is not being filtered.

**General Rule**

For a composite index, place columns based on:

* How queries filter the data

* How queries join tables

* How queries sort the data

* Column selectivity

* Equality predicates

* Range predicates

A common practical pattern is:

```
Equality columns first
Range columns later
```

**`5. Covering Index`**

A covering index contains all the columns required by a particular query.

Suppose the query is:
```
SELECT EmployeeName, Salary
FROM Employees
WHERE DepartmentId = 10;
```

A normal index may help locate matching rows:
```
CREATE INDEX IX_Employees_DepartmentId
ON Employees (DepartmentId);
```

However, the database may still need to access the table to retrieve:

* `EmployeeName`

* `Salary`

A covering index can include these columns:
```
CREATE INDEX IX_Employees_DepartmentId
ON Employees (DepartmentId)
INCLUDE
(
    EmployeeName,
    Salary
);
```

Here:

* `DepartmentId` is the indexed key column.

* `EmployeeName` and `Salary` are included columns.

* Included columns help the index return the required data without accessing the base table in many cases.

This is a SQL Server example.

**`6. Filtered Index`**

A filtered index is an index created only for rows that satisfy a condition.
```
CREATE INDEX IX_Orders_Pending
ON Orders (OrderDate)
WHERE Status = 'Pending';
```

This index contains only rows where:
```
Status = 'Pending'
```

It may be useful for:
```
SELECT *
FROM Orders
WHERE Status = 'Pending'
  AND OrderDate >= '2026-09-01';
```

Filtered indexes can be useful when:

* Only a small percentage of rows meet the condition.

* Queries frequently access that subset.

* The filtered condition is stable and useful.

Filtered indexes are supported by SQL Server. Other databases may provide similar features under different names.

------------
**Are Primary Keys Automatically Indexed?**

Usually, a database creates an index to enforce a primary key.

```
CREATE TABLE Employees
(
    EmployeeId INT PRIMARY KEY,
    EmployeeName VARCHAR(100)
);
```

The primary key generally gets a unique index internally.

However, you should remember:

* A primary key is a constraint.

* An index is a performance and storage structure.

* A primary key is not conceptually the same thing as an index.

* The database often uses an index to enforce the primary key efficiently.

**Are Foreign Keys Automatically Indexed?**

A foreign key is not automatically indexed in every database system.

```
CREATE TABLE Departments
(
    DepartmentId INT PRIMARY KEY,
    DepartmentName VARCHAR(100)
);

CREATE TABLE Employees
(
    EmployeeId INT PRIMARY KEY,
    EmployeeName VARCHAR(100),
    DepartmentId INT,

    FOREIGN KEY (DepartmentId)
        REFERENCES Departments(DepartmentId)
);
```

The referenced primary key is usually indexed.

But the foreign key column in `Employees` may need its own index:
```
CREATE INDEX IX_Employees_DepartmentId
ON Employees (DepartmentId);
```

An index on a foreign key can help with:

* Joins

* Filtering

* Deleting a referenced parent row

* Updating referenced key values

* Referential integrity checks

-------------
-------------

## Function

A function in SQL is a reusable database object or built-in operation that performs a specific task and returns a value or a result.

Functions are commonly used to:

* Perform calculations

* Manipulate strings

* Work with dates

* Handle `NULL` values

* Convert data types

* Aggregate multiple rows

* Return calculated results

```
SELECT UPPER('swapnil');
```

Output:
```
SWAPNIL
```

Here, `UPPER()` is a built-in SQL function.

**Types of SQL Functions**

SQL functions are commonly divided into two major categories:

```
1. Built-in Functions
2. User-Defined Functions
```

**`1. Built-in Functions`**

Built-in functions are already provided by the database system.

Examples include:

* String functions

* Numeric functions

* Date and time functions

* Aggregate functions

* Conversion functions

* NULL-handling functions

The exact functions vary between SQL Server, PostgreSQL, MySQL, Oracle, and other databases.

**`2. User-Defined Functions`**

A User-Defined Function, or UDF, is a function created by a developer to implement custom business logic.

User-defined functions can be reused in multiple SQL queries.

In SQL Server, common types include:

```
1. Scalar-valued function
2. Inline table-valued function
3. Multi-statement table-valued function
```

**A. Scalar-Valued Function**

A scalar-valued function returns a single value.

```
CREATE FUNCTION dbo.CalculateAnnualSalary
(
    @MonthlySalary DECIMAL(10, 2)
)
RETURNS DECIMAL(12, 2)
AS
BEGIN
    RETURN @MonthlySalary * 12;
END;
```

Call the function:
```
SELECT dbo.CalculateAnnualSalary(50000);
```

Output:
```
600000
```

Another example:
```
CREATE FUNCTION dbo.GetFullName
(
    @FirstName VARCHAR(50),
    @LastName VARCHAR(50)
)
RETURNS VARCHAR(101)
AS
BEGIN
    RETURN CONCAT(@FirstName, ' ', @LastName);
END;
```

```
SELECT dbo.GetFullName('Swapnil', 'Patil');
```

Output:

```
Swapnil Patil
```

**B. Inline Table-Valued Function**

An inline table-valued function returns a table and contains a single `SELECT` statement.

```
CREATE FUNCTION dbo.GetEmployeesByDepartment
(
    @DepartmentId INT
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        EmployeeId,
        EmployeeName,
        Salary
    FROM Employees
    WHERE DepartmentId = @DepartmentId
);
```

Call the function:
```
SELECT *
FROM dbo.GetEmployeesByDepartment(10);
```

This returns employees belonging to department `10`.

**C. Multi-Statement Table-Valued Function**

A multi-statement table-valued

--------------
--------------

## Stored Procedure

A stored procedure is a named collection of one or more SQL statements stored inside the database.

It is used to perform a specific task, such as:

* Fetching data

* Inserting records

* Updating records

* Deleting records

* Performing business logic

* Executing multiple SQL statements

* Managing transactions

* Generating reports

Instead of writing the same SQL statements repeatedly, we can store them inside a procedure and execute the procedure whenever required.

**Simple Example**

Suppose we have an `Employees` table:
```
CREATE TABLE Employees
(
    EmployeeId INT PRIMARY KEY,
    EmployeeName VARCHAR(100),
    DepartmentId INT,
    Salary DECIMAL(10, 2)
);
```

Create a stored procedure to fetch employees by department:
```
CREATE PROCEDURE dbo.GetEmployeesByDepartment
    @DepartmentId INT
AS
BEGIN
    SELECT
        EmployeeId,
        EmployeeName,
        DepartmentId,
        Salary
    FROM Employees
    WHERE DepartmentId = @DepartmentId;
END;
```

Execute the stored procedure:
```
EXEC dbo.GetEmployeesByDepartment
    @DepartmentId = 10;
```

The procedure returns employees whose `DepartmentId` is `10`.

**Syntax of a Stored Procedure**

In SQL Server, the basic syntax is:
```
CREATE PROCEDURE ProcedureName
    @ParameterName DataType
AS
BEGIN
    -- SQL statements
END;
```

Example
```
CREATE PROCEDURE dbo.GetAllEmployees
AS
BEGIN
    SELECT *
    FROM Employees;
END;
```

Execute it:
```
EXEC dbo.GetAllEmployees;
```

**Stored Procedure with Multiple Parameters**

A stored procedure can accept multiple parameters.
```
CREATE PROCEDURE dbo.GetEmployeesByDepartmentAndSalary
    @DepartmentId INT,
    @MinimumSalary DECIMAL(10, 2)
AS
BEGIN
    SELECT
        EmployeeId,
        EmployeeName,
        DepartmentId,
        Salary
    FROM Employees
    WHERE DepartmentId = @DepartmentId
      AND Salary >= @MinimumSalary;
END;
```

Execute it:
```
EXEC dbo.GetEmployeesByDepartmentAndSalary
    @DepartmentId = 10,
    @MinimumSalary = 50000;
```
---------------
**Stored Procedure with an OUTPUT Parameter**

A stored procedure can return values through output parameters.

```
CREATE PROCEDURE dbo.GetEmployeeCount
    @DepartmentId INT,
    @EmployeeCount INT OUTPUT
AS
BEGIN
    SELECT @EmployeeCount = COUNT(*)
    FROM Employees
    WHERE DepartmentId = @DepartmentId;
END;
```

Execute it:
```
DECLARE @Count INT;

EXEC dbo.GetEmployeeCount
    @DepartmentId = 10,
    @EmployeeCount = @Count OUTPUT;

SELECT @Count AS EmployeeCount;
```

The output parameter stores the number of employees in department `10`.

**Stored Procedure with Transaction**

Stored procedures can contain transaction logic.

Example:
```
CREATE PROCEDURE dbo.TransferSalary
    @SourceEmployeeId INT,
    @TargetEmployeeId INT,
    @Amount DECIMAL(10, 2)
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE Employees
        SET Salary = Salary - @Amount
        WHERE EmployeeId = @SourceEmployeeId;

        UPDATE Employees
        SET Salary = Salary + @Amount
        WHERE EmployeeId = @TargetEmployeeId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
```

The transaction ensures that both updates succeed together or are rolled back if an error occurs.

**Why Do We Use Stored Procedures?**

**`1. Code Reusability`**

A procedure can be created once and executed many times.
```
EXEC dbo.GetAllEmployees;
```

There is no need to rewrite the same SQL query repeatedly.

**`2. Centralized Business Logic`**

Business logic can be kept inside the database.

For example:

* Calculate an order total

* Validate an employee record

* Create an order

* Update inventory

* Transfer money between accounts

* Generate a report

**`3. Better Security`**

Users can be given permission to execute a stored procedure without giving them direct permission on the underlying tables.

Example:
```
GRANT EXECUTE
ON OBJECT::dbo.GetAllEmployees
TO SomeDatabaseUser;
```

This can help control access to database operations.

Security still depends on correct permissions and procedure design.

**`4. Reduced Network Traffic`**

Instead of sending many SQL statements separately from an application, the application can call one stored procedure.

For example:

```
Application
    |
    | Calls one stored procedure
    v
Database
    |
    | Executes multiple SQL statements
    v
Result
```

This can reduce communication between the application and the database.

**`5. Transaction Management`**

Stored procedures can group multiple operations into one transaction.

```
Operation 1
Operation 2
Operation 3
    |
    v
All succeed or all roll back
```

**`6. Easier Maintenance`**

If database logic changes, the stored procedure can be updated in one place.

Instead of modifying the same SQL logic in many application files, the procedure can be changed centrally.

```
DROP PROCEDURE dbo.GetAllEmployees;
```
--------------------

**Stored Procedure vs Function**

```
STORED PROCEDURE
- Executed using EXEC or EXECUTE in SQL Server.
- Can perform INSERT, UPDATE, and DELETE.
- Can return result sets.
- Can return output parameters.
- Can contain transaction logic.
- Can contain multiple SQL statements.
- Mainly used to perform operations or workflows.
- Does not have to return a value.

FUNCTION
- Must return a value or a table.
- Can be used in expressions or SELECT statements, depending on the type.
- Commonly used for calculations and reusable data retrieval.
- Has restrictions on certain operations.
- Usually should not be used for transaction control.
```
-----------
**Advantages of Stored Procedures**

```
- Reusable SQL logic
- Centralized database operations
- Can improve security
- Can reduce network communication
- Supports transactions
- Supports input and output parameters
- Can execute multiple SQL statements
- Easier to maintain database-specific logic
```

**Disadvantages of Stored Procedures**

```
- Can create database-specific dependency
- Complex procedures can become difficult to maintain
- Business logic may become distributed between application and database
- Testing and version control may require additional practices
- Poorly written procedures can still be slow
- Excessive use can tightly couple the application to the database
```

---------

**How to Improve the Performance of a Stored Procedure**

To improve the performance of a stored procedure, we need to identify the slow part and optimize:

* SQL queries

* Indexes

* Joins

* Filters

* Parameters

* Transactions

* Temporary tables

* Execution plans

* Returned data

A stored procedure itself is not automatically fast. Its performance depends mainly on the SQL statements inside it and how the database executes them.

**`1. Check the Actual Execution Plan`**

The first step is to find out which operation is slow.

In SQL Server Management Studio, enable the actual execution plan:

```
Ctrl + M
```

Then execute the stored procedure:
```
EXEC dbo.GetEmployeesByDepartment
    @DepartmentId = 10;
```

Look for expensive operations such as:

```
- Table Scan
- Clustered Index Scan
- Expensive Sort
- Hash Match
- Key Lookup
- Large number of logical reads
- Incorrect row estimates
```

Do not optimize blindly. First identify the actual bottleneck.

**`2. Create Proper Indexes`**

Indexes can improve performance when columns are frequently used in:

* `WHERE`

* `JOIN`

* `ORDER BY`

* `GROUP BY`

Suppose the stored procedure contains:

**`3. Use a Covering Index When Appropriate`**

Suppose the stored procedure contains:
```
SELECT
    EmployeeName,
    Salary
FROM Employees
WHERE DepartmentId = @DepartmentId;
```

A normal index may locate the rows but still need to access the base table to retrieve `EmployeeName` and `Salary`.

A covering index can help:
```
CREATE INDEX IX_Employees_DepartmentId
ON Employees (DepartmentId)
INCLUDE
(
    EmployeeName,
    Salary
);
```

This may eliminate additional lookups.

However, do not add every column to an index. Larger indexes require:

* More storage

* More maintenance

* More work during inserts and updates

**`4. Avoid SELECT *`**

Avoid this:
```
SELECT *
FROM Employees
WHERE DepartmentId = @DepartmentId;
```

Prefer selecting only the required columns:

Benefits:

* Less data transferred

* Less memory usage

* Smaller result sets

* Better chance of using a covering index

* Easier maintenance

**`5. Filter Data as Early as Possible`**

Avoid retrieving unnecessary rows and filtering them later in the application.


**`6. Avoid Functions on Indexed Columns in WHERE`**

Applying a function to an indexed column can prevent efficient index usage.

```
SELECT *
FROM Employees
WHERE YEAR(HireDate) = 2026;
```

Even if `HireDate` is indexed, SQL Server may need to evaluate `YEAR(HireDate)` for many rows.

Prefer a date range:
```
SELECT *
FROM Employees
WHERE HireDate >= '20260101'
  AND HireDate < '20270101';
```

This form is usually more index-friendly because the column is not wrapped in a function.
```
WHERE ISNULL(Status, 'Active') = 'Active'
```

Prefer a logically equivalent form when appropriate:
```
WHERE Status = 'Active'
   OR Status IS NULL;
```

Always verify the result with the execution plan because the best rewrite depends on the query.

**`7. Avoid Leading Wildcards in LIKE`**

This query may not use a normal index efficiently:
```
SELECT *
FROM Employees
WHERE EmployeeName LIKE '%swap%';
```

The leading `%` means the database does not know the starting characters.

This may be more index-friendly:
```
SELECT *
FROM Employees
WHERE EmployeeName LIKE 'Swap%';
```

For advanced text searching, consider:

* Full-text search

* Specialized search tools

* Appropriate database-specific text indexes

**`8. Use Appropriate Joins`**

Make sure join columns are indexed when necessary.
Also:

* Join using compatible data types.

* Avoid unnecessary joins.

* Avoid joining tables when their data is not needed.

* Check whether joins produce duplicate rows unintentionally.

**`9. Avoid Unnecessary Temporary Tables`**

Temporary tables can be useful, but creating them unnecessarily can increase overhead.

However, temporary tables can improve performance when:

* A complex result is reused multiple times.

* Intermediate data needs indexing.

* The query needs multiple processing stages.

* Breaking a complex query improves the execution plan.

The correct choice depends on the workload.

**`10. Avoid Cursors When Set-Based SQL Is Possible`**

Cursors process rows one by one.

This can be slower for large data sets.

Cursor-based approach:

```
Read one row
Process one row
Read next row
Process next row
...
```

**`11. Update Statistics`**

SQL Server uses statistics to estimate how many rows a query will return.

Outdated statistics can lead to poor execution plans.

Update statistics for a table:
```
UPDATE STATISTICS Employees;
```

Update statistics for a specific index or statistics object:
```
UPDATE STATISTICS Employees IX_Employees_DepartmentId;
```

Statistics are often updated automatically, but automatic updates may not always happen at the ideal time for every workload.

**`12. Avoid Returning Too Many Rows`**

A stored procedure may be slow because it returns a very large result set.

**`13. Use SET NOCOUNT ON`**

In SQL Server, use:
```
SET NOCOUNT ON;

`SET NOCOUNT ON` prevents SQL Server from sending messages such as:
(1 row affected)
```

after every statement.

**Benefits:**

* Reduces unnecessary network messages.

* Can improve performance in procedures containing many statements.

* Is commonly used in SQL Server stored procedures.

It does not eliminate the actual database work.

**`14. Use EXISTS When You Only Need to Check Existence`**

Instead of:

```
IF (
    SELECT COUNT(*)
    FROM Employees
    WHERE DepartmentId = @DepartmentId
) > 0
BEGIN
    PRINT 'Employees exist';
END;
```

Prefer:

```
IF EXISTS
(
    SELECT 1
    FROM Employees
    WHERE DepartmentId = @DepartmentId
)
BEGIN
    PRINT 'Employees exist';
END;
```

`EXISTS` can stop looking once a matching row is found.

**`15. Avoid Unnecessary DISTINCT`**

`DISTINCT` may require sorting or hashing.


**`16. Use Appropriate Isolation Levels`**

Isolation levels control how transactions interact with each other.

Poorly chosen isolation levels can cause:

* Blocking

* Locking

* Deadlocks

* Dirty reads

* Inconsistent results

Do not use `NOLOCK` simply because a query is slow.

Example:
```
SELECT *
FROM Employees WITH (NOLOCK);
```

`NOLOCK` can allow dirty or inconsistent reads. It is not a general performance fix.

Choose the isolation level based on the application's consistency requirements.

**`17. Avoid Dynamic SQL Unless Required`**

Dynamic SQL can be useful when the query structure genuinely needs to change, but careless dynamic SQL can cause:

* SQL injection risks

* Plan-cache problems

* Difficult debugging

* Poor maintainability

If dynamic SQL is required, use `sp_executesql` with parameters.

Example:
```
DECLARE @Sql NVARCHAR(MAX);

SET @Sql = N'
    SELECT
        EmployeeId,
        EmployeeName,
        Salary
    FROM Employees
    WHERE DepartmentId = @DepartmentId;
';

EXEC sys.sp_executesql
    @Sql,
    N'@DepartmentId INT',
    @DepartmentId = @DepartmentId;
```

Parameterized dynamic SQL helps avoid injecting values directly into the query string.

**`17. Measure Logical Reads and CPU Time`**

In SQL Server, you can inspect query resource usage using:

```
SET STATISTICS IO ON;
SET STATISTICS TIME ON;

EXEC dbo.GetEmployeesByDepartment
    @DepartmentId = 10;

SET STATISTICS IO OFF;
SET STATISTICS TIME OFF;
```

This provides information about:

```
- Logical reads
- Physical reads
- CPU time
- Elapsed time
```

Lower logical reads and CPU time are often signs of improvement, but always evaluate the complete workload.

**Practical Optimization Workflow**

Use this process when optimizing a stored procedure:

```
1. Identify the slow stored procedure.

2. Execute it with realistic parameter values.

3. Capture execution time.

4. Check the actual execution plan.

5. Check logical reads and CPU time.

6. Identify table scans, expensive joins, sorts, and lookups.

7. Verify existing indexes.

8. Create or modify indexes only when justified.

9. Review joins and WHERE conditions.

10. Avoid SELECT * and unnecessary data.

11. Check for parameter sniffing.

12. Review statistics.

13. Check blocking and locking.

14. Test the modified procedure.

15. Compare before and after performance.

16. Test with different parameter values.
```
------------
------------

## Indexes and Constraints in EF Core Code First

> In EF Core Code First, we normally define constraints and indexes in entity classes or through Fluent API. EF Core generates migration code based on these configurations. When we apply the migration using dotnet ef database update, EF Core executes the required database commands to create primary keys, foreign keys, unique constraints, and indexes. We usually do not create them manually. However, indexes required for specific performance scenarios must be explicitly configured because EF Core cannot automatically know every query pattern of the application.

When using Entity Framework Core Code First, you usually do not need to create primary keys, foreign keys, unique constraints, or indexes manually in SQL.

You define them in your:

* Entity classes

* Data annotations

* `OnModelCreating()`

* Fluent API configuration

Then EF Core generates a migration, and the migration creates or modifies the database objects.

**Basic Code First Flow**

```
1. Create entity classes
        ↓
2. Configure keys, relationships, and indexes
        ↓
3. Create an EF Core migration
        ↓
4. Apply the migration to the database
        ↓
5. EF Core creates tables, constraints, and indexes
```

Typical commands:
```
dotnet ef migrations add InitialCreate

dotnet ef database update
```

The migration contains SQL operations that create the required database objects.

**`1. Primary Key Creation`**

Consider this entity:
```csharp
public class Employee
{
    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public decimal Salary { get; set; }
}
```

EF Core convention recognizes `EmployeeId` as the primary key.

You can also explicitly configure it:
```csharp
using System.ComponentModel.DataAnnotations;

public class Employee
{
    [Key]
    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public decimal Salary { get; set; }
}
```

When you create a migration, EF Core generates a primary key definition.

Example migration code:
```csharp
migrationBuilder.CreateTable(
    name: "Employees",
    columns: table => new
    {
        EmployeeId = table.Column<int>(nullable: false)
            .Annotation("SqlServer:Identity", "1, 1"),

        EmployeeName = table.Column<string>(nullable: false),

        Salary = table.Column<decimal>(nullable: false)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_Employees", x => x.EmployeeId);
    });
```

The database creates the primary key when the migration is applied.

**`2. Foreign Key Creation`**

Suppose an employee belongs to a department.

**Department Entity**
```
public class Department
{
    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public ICollection<Employee> Employees { get; set; }
        = new List<Employee>();
}
```

**Employee Entity**
```
public class Employee
{
    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public Department Department { get; set; } = null!;
}
```

EF Core convention identifies:

```
Employee.DepartmentId
        ↓
Department.DepartmentId
```

as a foreign key relationship.

You can configure it explicitly:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Employee>()
        .HasOne(e => e.Department)
        .WithMany(d => d.Employees)
        .HasForeignKey(e => e.DepartmentId)
        .OnDelete(DeleteBehavior.Restrict);
}
```

The generated migration may contain:
```csharp
migrationBuilder.AddForeignKey(
    name: "FK_Employees_Departments_DepartmentId",
    table: "Employees",
    column: "DepartmentId",
    principalTable: "Departments",
    principalColumn: "DepartmentId",
    onDelete: ReferentialAction.Restrict);
```

The foreign key is created in the database when the migration runs.

**`3. NOT NULL Constraint`**

In EF Core, a non-nullable value type is generally required by convention.
```csharp
public class Employee
{
    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public decimal Salary { get; set; }
}
```

These properties are generally mapped as required:

```
EmployeeId
EmployeeName
Salary
```

For nullable reference types, the behavior depends on your project configuration.
```
public string? Email { get; set; }
```

This indicates that `Email` can be nullable.

You can configure required properties explicitly:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Employee>()
        .Property(e => e.EmployeeName)
        .IsRequired();
}
```

Generated database column:

```
EmployeeName VARCHAR(...) NOT NULL
```

The exact SQL type depends on the database provider.

**`4. Unique Constraint`**

Suppose an employee's email must be unique.

You can configure a unique index using Fluent API:
```
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Employee>()
        .HasIndex(e => e.Email)
        .IsUnique();
}
```

Entity:
```csharp
public class Employee
{
    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
```

The generated migration may contain:
```csharp
migrationBuilder.CreateIndex(
    name: "IX_Employees_Email",
    table: "Employees",
    column: "Email",
    unique: true);
```

This creates a unique index in the database.

A unique index prevents duplicate email values.

**`5. Normal Nonclustered Index`**

To create a normal index on `DepartmentId`:
```
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Employee>()
        .HasIndex(e => e.DepartmentId);
}
```

Generate and apply a migration:
```
dotnet ef migrations add AddEmployeeDepartmentIndex

dotnet ef database update
```

The migration may contain:
```
migrationBuilder.CreateIndex(
    name: "IX_Employees_DepartmentId",
    table: "Employees",
    column: "DepartmentId");
```

For SQL Server, a normal index created this way is generally a nonclustered index.

The actual index type depends on the database provider and any provider-specific configuration.

**`6. Composite Index`**

Suppose queries frequently filter by both:

```
DepartmentId
Salary
```

Create a composite index:
```
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Employee>()
        .HasIndex(e => new
        {
            e.DepartmentId,
            e.Salary
        });
}
```

The generated migration may contain:
```
migrationBuilder.CreateIndex(
    name: "IX_Employees_DepartmentId_Salary",
    table: "Employees",
    columns: new[]
    {
        "DepartmentId",
        "Salary"
    });
```

This creates an index on:

```
DepartmentId, Salary
```

The order is important.

For example, this index is generally more useful for:
```
WHERE DepartmentId = 10
```
or:
```
WHERE DepartmentId = 10
  AND Salary > 50000
```

It may be less useful for a query filtering only by `Salary`.

**`7. Covering Index with INCLUDE`**

A covering index contains all the columns required by a query.

Suppose the query is
```
SELECT EmployeeName, Salary
FROM Employees
WHERE DepartmentId = 10;
```

In SQL Server, you may want an index like:
```
CREATE INDEX IX_Employees_DepartmentId
ON Employees (DepartmentId)
INCLUDE
(
    EmployeeName,
    Salary
);
```

In EF Core, SQL Server-specific configuration can be used:
```
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Employee>()
        .HasIndex(e => e.DepartmentId)
        .IncludeProperties(e => new
        {
            e.EmployeeName,
            e.Salary
        });
}
```

Then create and apply a migration:
```
dotnet ef migrations add AddCoveringEmployeeIndex
dotnet ef database update
```

The migration may generate an index similar to:
```
migrationBuilder.CreateIndex(
    name: "IX_Employees_DepartmentId",
    table: "Employees",
    column: "DepartmentId")
    .Annotation(
        "SqlServer:Include",
        new[]
        {
            "EmployeeName",
            "Salary"
        });
```

Important points:

* `IncludeProperties()` is provider-specific.

* This example is intended for SQL Server.

* Other database providers may support different syntax or may not support included columns in the same way.

**`8. Filtered Index`**

Suppose only active employees are frequently searched.
```
CREATE INDEX IX_Employees_Active
ON Employees (DepartmentId)
WHERE IsActive = 1;
```

```
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Employee>()
        .HasIndex(e => e.DepartmentId)
        .HasFilter("[IsActive] = 1");
}
```

Entity:
```
public class Employee
{
    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public bool IsActive { get; set; }
}
```

The filter syntax is provider-specific.

For SQL Server, the filter expression commonly uses SQL Server column syntax.

**`9. Composite Unique Index`**

Suppose an employee cannot have the same combination of:

```
DepartmentId
EmployeeCode
```

Create a composite unique index:
```
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Employee>()
        .HasIndex(e => new
        {
            e.DepartmentId,
            e.EmployeeCode
        })
        .IsUnique();
}
```

This enforces uniqueness for the combination:

```
DepartmentId + EmployeeCode
```

It does not require each column to be unique individually.

For example, these records may be valid:

```
DepartmentId = 10, EmployeeCode = 100
DepartmentId = 20, EmployeeCode = 100
```

But these records would conflict:

```
DepartmentId = 10, EmployeeCode = 100
DepartmentId = 10, EmployeeCode = 100
```

**`10. Using Data Annotations for Indexes`**

In modern EF Core versions, indexes can also be configured using the `[Index]` attribute.

```
using Microsoft.EntityFrameworkCore;

[Index(nameof(DepartmentId))]
public class Employee
{
    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public int DepartmentId { get; set; }
}
```

Unique index:
```
using Microsoft.EntityFrameworkCore;

[Index(nameof(Email), IsUnique = true)]
public class Employee
{
    public int EmployeeId { get; set; }

    public string Email { get; set; } = string.Empty;
}
```

Composite index:
```
using Microsoft.EntityFrameworkCore;

[Index(nameof(DepartmentId), nameof(Salary))]
public class Employee
{
    public int EmployeeId { get; set; }

    public int DepartmentId { get; set; }

    public decimal Salary { get; set; }
}
```

For advanced configuration, Fluent API is generally more flexible.

**`11. Does EF Core Automatically Create Indexes?`**

EF Core automatically creates or configures some indexes through conventions.

Common examples include:

```
Primary key
- Usually has an index created by the database.

Foreign key
- EF Core may create an index for a foreign key through conventions,
  depending on the relationship and model configuration.

Unique constraint or unique index
- Created only when configured or required by the model.

Normal index
- Must be explicitly configured if you want one.

Composite index
- Must be explicitly configured.

Covering index
- Must be explicitly configured.
```

The important distinction is:

> EF Core conventions may create certain indexes automatically, but EF Core will not automatically create every performance-related index that your application may need.

**`12. Do We Need to Create Indexes Manually?`**

Usually, you should define indexes in EF Core and allow migrations to create them.

Recommended approach:

```
Define index in EF Core
        ↓
Create migration
        ↓
Review migration
        ↓
Apply migration
        ↓
Database creates the index
```

```
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Employee>()
        .HasIndex(e => e.DepartmentId);
}
```

Then:
```
dotnet ef migrations add AddDepartmentIndex

dotnet ef database update
```

You generally do not need to execute this manually:
```
CREATE INDEX IX_Employees_DepartmentId
ON Employees (DepartmentId);
```

However, manually creating an index may be appropriate when:

* The database is managed separately from EF Core.

* You are working with an existing production database.

* You need a database-specific feature not supported directly by EF Core.

* Your organization uses database deployment scripts.

* You need advanced provider-specific index options.

--------
--------

## Normalization in SQL

Normalization is the process of organizing data in a database to:

* Reduce duplicate data.

* Avoid data inconsistency.

* Prevent update, insert, and delete anomalies.

* Improve data integrity.

* Make relationships between tables clear.

Instead of storing all information in one large table, we divide it into smaller, related tables.

**Why Do We Need Normalization?**

Suppose we store student and course information in one table:
```
StudentId  StudentName  CourseId  CourseName       InstructorName
---------  -----------  --------  ---------------  --------------
1          Swapnil      101       SQL              Amit
1          Swapnil      102       C#               Rahul
2          Priya        101       SQL              Amit
```

Here, the student name and course information are repeated.

For example:

* `Swapnil` is repeated for every course.

* `SQL` and `Amit` are repeated for every student taking that course.

* If the instructor name changes, multiple rows must be updated.

* If we delete the only student enrolled in a course, we may lose the course information.

These are signs of poor database design.

**Types of Anomalies**

**`1. Update Anomaly`**

Suppose instructor Amit changes his name to Amit Sharma.

In the denormalized table, we must update every row containing that instructor.

If we update only some rows, the database becomes inconsistent.

```
Some rows: Amit
Other rows: Amit Sharma
```

**`2. Insert Anomaly`**

Suppose we want to add a new course, but no student has enrolled in it yet.

In the combined table, we may not be able to insert the course without providing a `StudentId`.

This means course information is unnecessarily dependent on student information.

**`3. Delete Anomaly`**

Suppose Priya is the only student enrolled in Course 101.

If we delete Priya's enrollment, we may accidentally lose the only record containing Course 101 and its instructor.

**Normal Forms**

The most commonly discussed normal forms are:

1. First Normal Form — 1NF

2. Second Normal Form — 2NF

3. Third Normal Form — 3NF

4. Boyce-Codd Normal Form — BCNF

In most business applications, designing tables up to Third Normal Form (3NF) is a common practical goal.

**`1NF — First Normal Form`**

A table is in First Normal Form when:

* Each column contains atomic, indivisible values.

* There are no repeating groups.

* Each row represents one record.

* A column does not store multiple values separated by commas.

**Incorrect Design**
```
CREATE TABLE Students (
    StudentId INT PRIMARY KEY,
    StudentName VARCHAR(100),
    Courses VARCHAR(200)
);
```

```
StudentId  StudentName  Courses
---------  -----------  ----------------
1          Swapnil      SQL, C#, Angular
```

The `Courses` column contains multiple values.

This makes searching and joining difficult.

```
SELECT *
FROM Students
WHERE Courses LIKE '%SQL%';
```

This is unreliable because:

* It may match partial words.

* It is difficult to join with a course table.

* It is difficult to enforce referential integrity.

* It is difficult to store course-specific information.

**Correct 1NF Design**

Store one course value per row:
```
CREATE TABLE StudentCourses (
    StudentId INT,
    StudentName VARCHAR(100),
    CourseName VARCHAR(100)
);
```

Example:

```
StudentId  StudentName  CourseName
---------  -----------  -----------
1          Swapnil      SQL
1          Swapnil      C#
1          Swapnil      Angular
```

The values are now atomic, but the student name is repeated. We can improve this further using 2NF and 3NF.

**`2NF — Second Normal Form`**

A table is in Second Normal Form when:

* It is already in 1NF.

* Every non-key column depends on the entire primary key.

* There is no partial dependency on only part of a composite key.

2NF mainly matters when a table has a composite primary key.

Consider this table:
```
CREATE TABLE StudentCourses (
    StudentId INT,
    CourseId INT,
    StudentName VARCHAR(100),
    CourseName VARCHAR(100),
    EnrollmentDate DATE,
    PRIMARY KEY (StudentId, CourseId)
);
```

The primary key is:

```
(StudentId, CourseId)
```

But:

* `StudentName` depends only on `StudentId`.

* `CourseName` depends only on `CourseId`.

* `EnrollmentDate` depends on both `StudentId` and `CourseId`.

Therefore:

```
StudentId + CourseId → EnrollmentDate
StudentId             → StudentName
CourseId              → CourseName
```

`StudentName` and `CourseName` do not depend on the complete composite key. This violates 2NF.

**Correct 2NF Design**

Separate the data into three tables:
```
CREATE TABLE Students (
    StudentId INT PRIMARY KEY,
    StudentName VARCHAR(100) NOT NULL
);

CREATE TABLE Courses (
    CourseId INT PRIMARY KEY,
    CourseName VARCHAR(100) NOT NULL
);

CREATE TABLE StudentCourses (
    StudentId INT,
    CourseId INT,
    EnrollmentDate DATE NOT NULL,

    PRIMARY KEY (StudentId, CourseId),

    FOREIGN KEY (StudentId)
        REFERENCES Students(StudentId),

    FOREIGN KEY (CourseId)
        REFERENCES Courses(CourseId)
);
```

Now:

* Student information belongs in `Students`.

* Course information belongs in `Courses`.

* Enrollment information belongs in `StudentCourses`.

**`3NF — Third Normal Form`**

A table is in Third Normal Form when:

* It is already in 2NF.

* Non-key columns do not depend on other non-key columns.

* There are no transitive dependencies.

Consider this table:
```
CREATE TABLE Employees (
    EmployeeId INT PRIMARY KEY,
    EmployeeName VARCHAR(100),
    DepartmentId INT,
    DepartmentName VARCHAR(100),
    DepartmentLocation VARCHAR(100)
);
```

Here:

```
EmployeeId → DepartmentId
DepartmentId → DepartmentName
DepartmentId → DepartmentLocation
```

Therefore:

```
EmployeeId → DepartmentId → DepartmentName
EmployeeId → DepartmentId → DepartmentLocation
```

`DepartmentName` and `DepartmentLocation` depend on `DepartmentId`, not directly on `EmployeeId`.

This is a transitive dependency and violates 3NF.

**Correct 3NF Design**
```
CREATE TABLE Departments (
    DepartmentId INT PRIMARY KEY,
    DepartmentName VARCHAR(100) NOT NULL,
    DepartmentLocation VARCHAR(100)
);

CREATE TABLE Employees (
    EmployeeId INT PRIMARY KEY,
    EmployeeName VARCHAR(100) NOT NULL,
    DepartmentId INT NOT NULL,

    FOREIGN KEY (DepartmentId)
        REFERENCES Departments(DepartmentId)
);
```

Now:

* Department details are stored once.

* Employees refer to departments using `DepartmentId`.

* Updating a department location requires changing only one row.
------------
**Practical Example: Normalizing an Order Table**

Suppose an application initially stores orders like this:
```
CREATE TABLE OrderDetails (
    OrderId INT,
    OrderDate DATE,
    CustomerId INT,
    CustomerName VARCHAR(100),
    CustomerPhone VARCHAR(20),
    ProductId INT,
    ProductName VARCHAR(100),
    ProductPrice DECIMAL(10, 2),
    Quantity INT
);
```

Example data:

```
OrderId  OrderDate    CustomerId  CustomerName  ProductId  ProductName  ProductPrice  Quantity
-------  -----------  ----------  -------------  ---------  -----------  ------------  --------
1001     2026-09-12   1           Swapnil        101        Laptop       60000         1
1001     2026-09-12   1           Swapnil        102        Mouse         1000          2
1002     2026-09-13   1           Swapnil        101        Laptop       60000         1
```

There is significant duplication:

* Customer information is repeated.

* Order information is repeated for every product.

* Product information is repeated across orders.

A normalized design would be:

**Customers**
```
CREATE TABLE Customers (
    CustomerId INT PRIMARY KEY,
    CustomerName VARCHAR(100) NOT NULL,
    CustomerPhone VARCHAR(20)
);
```

**Orders**
```
CREATE TABLE Orders (
    OrderId INT PRIMARY KEY,
    OrderDate DATE NOT NULL,
    CustomerId INT NOT NULL,

    FOREIGN KEY (CustomerId)
        REFERENCES Customers(CustomerId)
);
```

**Products**
```
CREATE TABLE Products (
    ProductId INT PRIMARY KEY,
    ProductName VARCHAR(100) NOT NULL,
    CurrentPrice DECIMAL(10, 2) NOT NULL
);
```

**OrderItems**
```
CREATE TABLE OrderItems (
    OrderId INT,
    ProductId INT,
    Quantity INT NOT NULL,

    PRIMARY KEY (OrderId, ProductId),

    FOREIGN KEY (OrderId)
        REFERENCES Orders(OrderId),

    FOREIGN KEY (ProductId)
        REFERENCES Products(ProductId)
);
```

The relationships are now:

```
Customers
    |
    | 1-to-many
    v
Orders
    |
    | 1-to-many
    v
OrderItems
    ^
    |
    | many-to-1
    |
Products
```

**How to Insert Data After Normalization**

First insert the customer:
```
INSERT INTO Customers
(
    CustomerId,
    CustomerName,
    CustomerPhone
)
VALUES
(
    1,
    'Swapnil',
    '9876543210'
);
```

Insert products:
```
INSERT INTO Products
(
    ProductId,
    ProductName,
    CurrentPrice
)
VALUES
(101, 'Laptop', 60000),
(102, 'Mouse', 1000);
```

Insert the order:
```
INSERT INTO Orders
(
    OrderId,
    OrderDate,
    CustomerId
)
VALUES
(
    1001,
    '2026-09-12',
    1
);
```

Insert order items:
```
INSERT INTO OrderItems
(
    OrderId,
    ProductId,
    Quantity
)
VALUES
(1001, 101, 1),
(1001, 102, 2);
```

**How to Read Normalized Data Using JOIN**

Normalization separates information into multiple tables. To display the complete order, we use `JOIN`.
```
SELECT
    o.OrderId,
    o.OrderDate,
    c.CustomerName,
    c.CustomerPhone,
    p.ProductName,
    p.CurrentPrice,
    oi.Quantity,
    p.CurrentPrice * oi.Quantity AS LineTotal
FROM Orders o
INNER JOIN Customers c
    ON o.CustomerId = c.CustomerId
INNER JOIN OrderItems oi
    ON o.OrderId = oi.OrderId
INNER JOIN Products p
    ON oi.ProductId = p.ProductId;
```

The result can look like:

```
OrderId  OrderDate    CustomerName  ProductName  CurrentPrice  Quantity  LineTotal
-------  -----------  ------------  -----------  ------------  --------  ---------
1001     2026-09-12   Swapnil       Laptop       60000         1         60000
1001     2026-09-12   Swapnil       Mouse         1000          2         2000
```

**How Normalization Is Performed in Real Projects**

In real applications, normalization is usually performed during database design.

-> Step 1: Identify the Business Entities

For an e-commerce application, entities may include:

```
Customer
Order
OrderItem
Product
Payment
Address
```

For an HR application:

```
Employee
Department
Designation
Project
EmployeeProject
```

-> Step 2: Identify Attributes

For an employee:

```
EmployeeId
EmployeeName
Email
DepartmentId
JoiningDate
```

For a department:

```
DepartmentId
DepartmentName
DepartmentLocation
```

-> Step 3: Identify Relationships

Examples:

```
One department can have many employees.
One customer can place many orders.
One order can contain many products.
One product can appear in many orders.
```

A many-to-many relationship usually requires a junction table.

Example:

```
Students ↔ Courses
```

This becomes:

```
Students
StudentCourses
Courses
```

-> Step 4: Apply Normal Forms

Check the design:

* Are columns atomic? Apply 1NF.

* Do columns depend on the complete composite key? Apply 2NF.

* Do non-key columns depend on other non-key columns? Apply 3NF.

-> Step 5: Add Keys and Constraints

Use:

* Primary keys.

* Foreign keys.

* Unique constraints or unique indexes.

* `NOT NULL`.

* `CHECK`.

* `DEFAULT`.

Example:
```
CREATE TABLE Employees (
    EmployeeId INT PRIMARY KEY,
    Email VARCHAR(150) NOT NULL UNIQUE,
    Salary DECIMAL(10, 2) CHECK (Salary >= 0),
    IsActive BIT NOT NULL DEFAULT 1,
    DepartmentId INT NOT NULL,

    FOREIGN KEY (DepartmentId)
        REFERENCES Departments(DepartmentId)
);
```

-> Step 6: Add Indexes Based on Queries

Normalization and indexing solve different problems.

* Normalization organizes data and reduces duplication.

* Indexes improve query performance.

For example:

SQL

```
CREATE INDEX IX_Employees_DepartmentId
ON Employees(DepartmentId);
```

This can improve queries that frequently filter employees by department.

-------
**Normalization vs Denormalization**

**`Normalization`**

Use normalization when:

* Data consistency is important.

* The system performs many inserts and updates.

* The same information is reused in many places.

* You need strong referential integrity.

* You are designing transactional systems such as banking, HR, orders, and inventory.

**`Denormalization`**

Denormalization intentionally introduces some duplication to improve read performance or simplify reporting.

For example, an order report may store:

```
OrderId
CustomerName
ProductName
Quantity
TotalAmount
```

Although these values can be obtained through joins, storing some calculated or repeated data may make reporting faster.

However, denormalization introduces additional responsibilities:

* Keeping duplicated values synchronized.

* Handling updates carefully.

* Preventing inconsistent data.

* Managing extra storage.

A common real-world approach is:

```
Transactional database → Mostly normalized
Reporting/data warehouse → Sometimes denormalized
```

---------
---------