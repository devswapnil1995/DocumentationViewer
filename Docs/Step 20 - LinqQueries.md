**Our tables**

`Employee`

| Id | Name  | DepartmentId | Salary | IsActive |
| -: | ----- | -----------: | -----: | -------- |
|  1 | Amit  |            1 |  80000 | true     |
|  2 | Rahul |            1 |  90000 | true     |
|  3 | Priya |            2 |  75000 | true     |
|  4 | John  |            3 |  95000 | true     |
|  5 | Neha  |            4 |  70000 | false    |

`Department`

| Id | Name      |
| -: | --------- |
|  1 | IT        |
|  2 | HR        |
|  3 | Finance   |
|  4 | Sales     |
|  5 | Marketing |

Notice:

**Marketing has no employees.**

This will help us understand `INNER JOIN` vs `LEFT JOIN`.

---

**Basic WHERE**

**SQL**

```sql
SELECT *
FROM Employees
WHERE Salary > 80000
  AND IsActive = 1;
```

**LINQ Method Syntax**

```csharp
var result = employees
    .Where(e => e.Salary > 80000 &&
                e.IsActive);
```

**LINQ Query Syntax**

```csharp
var result =
    from e in employees
    where e.Salary > 80000
       && e.IsActive
    select e;
```

**Remember**

```text
SQL       → LINQ Method
WHERE     → Where()
SELECT    → Select()
```

---

**SELECT specific columns**

SQL:

```sql
SELECT Id, Name, Salary
FROM Employees;
```

Method:

```csharp
var result = employees
    .Select(e => new
    {
        e.Id,
        e.Name,
        e.Salary
    });
```

Query:

```csharp
var result =
    from e in employees
    select new
    {
        e.Id,
        e.Name,
        e.Salary
    };
```

---

**INNER JOIN**

This is one of the most important interview queries.

SQL

```sql
SELECT
    e.Name,
    e.Salary,
    d.Name AS DepartmentName
FROM Employees e
INNER JOIN Departments d
    ON e.DepartmentId = d.Id;
```

---

Method Syntax

```csharp
var result = employees
    .Join(
        departments,
        e => e.DepartmentId,
        d => d.Id,
        (e, d) => new
        {
            EmployeeName = e.Name,
            e.Salary,
            DepartmentName = d.Name
        });
```

**Understand `Join()`**

```text
Join(
    Outer collection,
    Inner collection,
    Outer key,
    Inner key,
    Result
)
```

Here:

```csharp
e => e.DepartmentId
```

matches:

```csharp
d => d.Id
```
Query Syntax

```csharp
var result =
    from e in employees
    join d in departments
        on e.DepartmentId equals d.Id
    select new
    {
        EmployeeName = e.Name,
        e.Salary,
        DepartmentName = d.Name
    };
```
---

**LEFT JOIN**

LINQ doesn't have a direct:

```csharp
.LeftJoin()
```

Instead we use:

```text
GroupJoin + SelectMany + DefaultIfEmpty
```

SQL

```sql
SELECT
    d.Name AS DepartmentName,
    e.Name AS EmployeeName
FROM Departments d
LEFT JOIN Employees e
    ON d.Id = e.DepartmentId;
```

Marketing will appear even though it has no employee.

---

Method Syntax

```csharp
var result = departments
    .GroupJoin(
        employees,
        d => d.Id,
        e => e.DepartmentId,
        (d, empGroup) => new
        {
            Department = d,
            Employees = empGroup
        })
    .SelectMany(
        x => x.Employees.DefaultIfEmpty(),
        (x, e) => new
        {
            DepartmentName = x.Department.Name,
            EmployeeName = e?.Name
        });
```

The important part:

```csharp
.DefaultIfEmpty()
```

means:

> If there is no matching employee, still return the department with `null` employee.


Query Syntax

This is much easier to remember:

```csharp
var result =
    from d in departments
    join e in employees
        on d.Id equals e.DepartmentId
        into employeeGroup
    from e in employeeGroup.DefaultIfEmpty()
    select new
    {
        DepartmentName = d.Name,
        EmployeeName = e?.Name
    };
```

**Interview answer**

> "LINQ doesn't have a direct LeftJoin operator in the traditional LINQ operators. We generally implement it using GroupJoin followed by DefaultIfEmpty, or use the equivalent query syntax."

---

**INNER JOIN + WHERE**

SQL

```sql
SELECT
    e.Name,
    d.Name AS DepartmentName,
    e.Salary
FROM Employees e
INNER JOIN Departments d
    ON e.DepartmentId = d.Id
WHERE e.Salary > 80000
ORDER BY e.Salary DESC;
```

Method

```csharp
var result = employees
    .Join(
        departments,
        e => e.DepartmentId,
        d => d.Id,
        (e, d) => new
        {
            Employee = e,
            Department = d
        })
    .Where(x => x.Employee.Salary > 80000)
    .OrderByDescending(x => x.Employee.Salary)
    .Select(x => new
    {
        EmployeeName = x.Employee.Name,
        DepartmentName = x.Department.Name,
        x.Employee.Salary
    });
```

Query

```csharp
var result =
    from e in employees
    join d in departments
        on e.DepartmentId equals d.Id
    where e.Salary > 80000
    orderby e.Salary descending
    select new
    {
        EmployeeName = e.Name,
        DepartmentName = d.Name,
        e.Salary
    };
```

---

**ORDER BY**

SQL:

```sql
SELECT *
FROM Employees
ORDER BY Salary ASC;
```

Method:

```csharp
var result = employees
    .OrderBy(e => e.Salary);
```

Query:

```csharp
var result =
    from e in employees
    orderby e.Salary
    select e;
```

---

**ORDER BY DESC**

SQL:

```sql
SELECT *
FROM Employees
ORDER BY Salary DESC;
```

Method:

```csharp
var result = employees
    .OrderByDescending(e => e.Salary);
```

Query:

```csharp
var result =
    from e in employees
    orderby e.Salary descending
    select e;
```

---

**ORDER BY + THEN BY **

SQL:

```sql
SELECT *
FROM Employees
ORDER BY DepartmentId ASC, Salary DESC;
```

Method:

```csharp
var result = employees
    .OrderBy(e => e.DepartmentId)
    .ThenByDescending(e => e.Salary);
```

Query:

```csharp
var result =
    from e in employees
    orderby e.DepartmentId,
            e.Salary descending
    select e;
```

**Important**

Don't do:

```csharp
.OrderBy(e => e.DepartmentId)
.OrderByDescending(e => e.Salary)
```

The second `OrderBy` replaces the first ordering.

Use:

```csharp
OrderBy()
ThenBy()
```

---

**GROUP BY**

Suppose:

> Get employee count for each department.

SQL

```sql
SELECT
    DepartmentId,
    COUNT(*) AS EmployeeCount
FROM Employees
GROUP BY DepartmentId;
```

Method Syntax

```csharp
var result = employees
    .GroupBy(e => e.DepartmentId)
    .Select(g => new
    {
        DepartmentId = g.Key,
        EmployeeCount = g.Count()
    });
```

Query Syntax

```csharp
var result =
    from e in employees
    group e by e.DepartmentId
    into g
    select new
    {
        DepartmentId = g.Key,
        EmployeeCount = g.Count()
    };
```

Important concept

After:

```csharp
.GroupBy(e => e.DepartmentId)
```

you get groups.

```text
Group
 Key = 1
 Items = [Amit, Rahul]

Group
 Key = 2
 Items = [Priya]

Group
 Key = 3
 Items = [John]
```

`g.Key` = grouping value.

---

**GROUP BY + SUM**

SQL:

```sql
SELECT
    DepartmentId,
    SUM(Salary) AS TotalSalary
FROM Employees
GROUP BY DepartmentId;
```

Method:

```csharp
var result = employees
    .GroupBy(e => e.DepartmentId)
    .Select(g => new
    {
        DepartmentId = g.Key,
        TotalSalary = g.Sum(e => e.Salary)
    });
```

Query:

```csharp
var result =
    from e in employees
    group e by e.DepartmentId
    into g
    select new
    {
        DepartmentId = g.Key,
        TotalSalary = g.Sum(e => e.Salary)
    };
```

---

**GROUP BY + AVG**

SQL:

```sql
SELECT
    DepartmentId,
    AVG(Salary) AS AverageSalary
FROM Employees
GROUP BY DepartmentId;
```

Method:

```csharp
var result = employees
    .GroupBy(e => e.DepartmentId)
    .Select(g => new
    {
        DepartmentId = g.Key,
        AverageSalary = g.Average(e => e.Salary)
    });
```

Query:

```csharp
var result =
    from e in employees
    group e by e.DepartmentId
    into g
    select new
    {
        DepartmentId = g.Key,
        AverageSalary = g.Average(e => e.Salary)
    };
```

---

**GROUP BY + HAVING**

Suppose:

> Departments having more than 2 employees.

SQL

```sql
SELECT
    DepartmentId,
    COUNT(*) AS EmployeeCount
FROM Employees
GROUP BY DepartmentId
HAVING COUNT(*) > 2;
```

Method

```csharp
var result = employees
    .GroupBy(e => e.DepartmentId)
    .Where(g => g.Count() > 2)
    .Select(g => new
    {
        DepartmentId = g.Key,
        EmployeeCount = g.Count()
    });
```

Query

```csharp
var result =
    from e in employees
    group e by e.DepartmentId
    into g
    where g.Count() > 2
    select new
    {
        DepartmentId = g.Key,
        EmployeeCount = g.Count()
    };
```

**Key mapping**

```text
SQL HAVING
    ↓
LINQ Where() after GroupBy()
```

---

**GROUP BY Department Name using JOIN**

This is a **very common interview query**.

> Get department name and number of employees.

SQL

```sql
SELECT
    d.Name,
    COUNT(e.Id) AS EmployeeCount
FROM Departments d
LEFT JOIN Employees e
    ON d.Id = e.DepartmentId
GROUP BY d.Name;
```

Method Syntax

```csharp
var result = departments
    .GroupJoin(
        employees,
        d => d.Id,
        e => e.DepartmentId,
        (d, employees) => new
        {
            DepartmentName = d.Name,
            EmployeeCount = employees.Count()
        });
```

Query Syntax

```csharp
var result =
    from d in departments
    join e in employees
        on d.Id equals e.DepartmentId
        into employeeGroup
    select new
    {
        DepartmentName = d.Name,
        EmployeeCount = employeeGroup.Count()
    };
```

This is an excellent example because `GroupJoin` naturally gives you the grouped employees.

---

**DISTINCT**

SQL:

```sql
SELECT DISTINCT DepartmentId
FROM Employees;
```

Method:

```csharp
var result = employees
    .Select(e => e.DepartmentId)
    .Distinct();
```

Query:

```csharp
var result =
    (from e in employees
     select e.DepartmentId)
    .Distinct();
```

---

**TOP 5**

SQL:

```sql
SELECT TOP 5 *
FROM Employees
ORDER BY Salary DESC;
```

Method:

```csharp
var result = employees
    .OrderByDescending(e => e.Salary)
    .Take(5);
```

Query:

```csharp
var result =
    (from e in employees
     orderby e.Salary descending
     select e)
    .Take(5);
```

---

**First / FirstOrDefault**

SQL concept:

```sql
SELECT TOP 1 *
FROM Employees
WHERE Id = 10;
```

Method:

```csharp
var employee = employees
    .FirstOrDefault(e => e.Id == 10);
```

Query:

```csharp
var employee =
    (from e in employees
     where e.Id == 10
     select e)
    .FirstOrDefault();
```

**Difference**

```text
First()
       → throws exception if no result

FirstOrDefault()
       → returns default/null
```

---

**Any**

SQL:

```sql
IF EXISTS (
    SELECT 1
    FROM Employees
    WHERE Salary > 100000
)
```

LINQ:

```csharp
bool exists = employees
    .Any(e => e.Salary > 100000);
```

Query:

```csharp
bool exists =
    (from e in employees
     where e.Salary > 100000
     select e)
    .Any();
```

---

**IN equivalent**

SQL:

```sql
SELECT *
FROM Employees
WHERE DepartmentId IN (1, 2, 3);
```

Method:

```csharp
var departmentIds = new[] { 1, 2, 3 };

var result = employees
    .Where(e => departmentIds.Contains(e.DepartmentId));
```

Query:

```csharp
var departmentIds = new[] { 1, 2, 3 };

var result =
    from e in employees
    where departmentIds.Contains(e.DepartmentId)
    select e;
```

Very common EF Core pattern.

---

**Multiple conditions**

SQL:

```sql
SELECT *
FROM Employees
WHERE Salary >= 70000
  AND Salary <= 100000
  AND IsActive = 1;
```

Method:

```csharp
var result = employees
    .Where(e =>
        e.Salary >= 70000 &&
        e.Salary <= 100000 &&
        e.IsActive);
```

Query:

```csharp
var result =
    from e in employees
    where e.Salary >= 70000
       && e.Salary <= 100000
       && e.IsActive
    select e;
```

---

**String search**

SQL:

```sql
SELECT *
FROM Employees
WHERE Name LIKE 'A%';
```

LINQ:

```csharp
var result = employees
    .Where(e => e.Name.StartsWith("A"));
```

SQL:

```sql
WHERE Name LIKE '%it%'
```

LINQ:

```csharp
var result = employees
    .Where(e => e.Name.Contains("it"));
```

For EF Core, the exact SQL translation and case sensitivity depend on provider/database configuration, so don't assume `Contains` has identical behavior across all databases.

---

**Multiple JOINs**

Suppose we add:

```text
Employees
Departments
Projects
```

Relationship:

```text
Employee
   ↓
Department
```

and:

```text
Employee
   ↓
Project
```

SQL:

```sql
SELECT
    e.Name,
    d.Name AS Department,
    p.Name AS Project
FROM Employees e
INNER JOIN Departments d
    ON e.DepartmentId = d.Id
INNER JOIN Projects p
    ON e.ProjectId = p.Id;
```

Method:

```csharp
var result = employees
    .Join(
        departments,
        e => e.DepartmentId,
        d => d.Id,
        (e, d) => new
        {
            Employee = e,
            Department = d
        })
    .Join(
        projects,
        x => x.Employee.ProjectId,
        p => p.Id,
        (x, p) => new
        {
            EmployeeName = x.Employee.Name,
            DepartmentName = x.Department.Name,
            ProjectName = p.Name
        });
```

Query:

```csharp
var result =
    from e in employees
    join d in departments
        on e.DepartmentId equals d.Id
    join p in projects
        on e.ProjectId equals p.Id
    select new
    {
        EmployeeName = e.Name,
        DepartmentName = d.Name,
        ProjectName = p.Name
    };
```

---

> Get active employees with their department name, only for departments having more than 1 active employee, calculate average salary per department, and sort by average salary descending.

SQL

```sql
SELECT
    d.Name AS DepartmentName,
    COUNT(e.Id) AS EmployeeCount,
    AVG(e.Salary) AS AverageSalary
FROM Departments d
INNER JOIN Employees e
    ON d.Id = e.DepartmentId
WHERE e.IsActive = 1
GROUP BY d.Name
HAVING COUNT(e.Id) > 1
ORDER BY AverageSalary DESC;
```

---

Method Syntax

```csharp
var result = employees
    .Where(e => e.IsActive)
    .Join(
        departments,
        e => e.DepartmentId,
        d => d.Id,
        (e, d) => new
        {
            Employee = e,
            Department = d
        })
    .GroupBy(x => x.Department.Name)
    .Where(g => g.Count() > 1)
    .Select(g => new
    {
        DepartmentName = g.Key,
        EmployeeCount = g.Count(),
        AverageSalary = g.Average(x => x.Employee.Salary)
    })
    .OrderByDescending(x => x.AverageSalary);
```

---

Query Syntax

```csharp
var result =
    from e in employees
    where e.IsActive

    join d in departments
        on e.DepartmentId equals d.Id

    group e by d.Name
    into g

    where g.Count() > 1

    orderby g.Average(x => x.Salary) descending

    select new
    {
        DepartmentName = g.Key,
        EmployeeCount = g.Count(),
        AverageSalary = g.Average(x => x.Salary)
    };
```

This one query covers:

```text
WHERE
INNER JOIN
GROUP BY
HAVING
COUNT
AVG
ORDER BY
SELECT
```

That's a **very good interview practice query**.

---

```text
Employees with highest salary
Second highest salary
Department-wise employee count
Department-wise average salary
Employees without department
Departments without employees
Duplicate records
Top N per group
Latest record per user
Employees earning above department average
```

Those **real interview queries** are where we should go next, because they will force you to combine `Where + GroupBy + Join + OrderBy + Select + First/Any`, and you'll become much more comfortable with LINQ.

------
**Employees with highest salary**

SQL:
```text
SELECT TOP 1 *
FROM Employees
ORDER BY Salary DESC;
```

Method:
```
var employee = employees
    .OrderByDescending(x => x.Salary)
    .FirstOrDefault();
```
If use take(1), it returns collection. FirstOrDefault returns one employee.

Query:
```
var result =
    (from e in employees
     orderby e.Salary descending
     select e)
    .Take(1);  //.FirstOrDefault();
```
----
**Second highest salary**

SQL - used Desse_Rank() to get all emp with same salary, or use row num of single
```
WITH ranked AS
(
    SELECT
        EmployeeId,
        Name,
        Salary,
        DENSE_RANK() OVER (ORDER BY Salary DESC) AS RankNo
    FROM Employees
)
SELECT *
FROM ranked
WHERE RankNo = 2;
```

```
//Second-highest DISTINCT salary
var result = employees
    .Select(x => x.Salary)
    .Distinct()
    .OrderByDescending(x => x)
    .Skip(1)
    .FirstOrDefault();

//Find all employees who have the second-highest salary.
var secondHighestSalary = employees
    .Select(x => x.Salary)
    .Distinct()
    .OrderByDescending(x => x)
    .Skip(1)
    .FirstOrDefault();

var result = employees
    .Where(x => x.Salary == secondHighestSalary);
```

Query:
```
//second-highest distinct salary
var result =
    (from e in employees
     select e.Salary)
    .Distinct()
    .OrderByDescending(x => x)
    .Skip(1)
    .FirstOrDefault();

//employees having second-highest salary

var salary = employees
    .Select(x => x.Salary)
    .Distinct()
    .OrderByDescending(x => x)
    .Skip(1)
    .FirstOrDefault();

var result = employees
    .Where(x => x.Salary == salary);
```
----
**All employees who do NOT have a matching department.**

SQL:
```
SELECT
    e.*
FROM Employees e
LEFT JOIN Departments d
    ON e.DepartmentId = d.Id
WHERE d.Id IS NULL;
```

Method Syntax:
```
var result = employees
    .GroupJoin(
        departments,
        e => e.DepartmentId,
        d => d.Id,
        (e, departmentGroup) => new
        {
            Employee = e,
            Departments = departmentGroup
        })
    .SelectMany(
        x => x.Departments.DefaultIfEmpty(),
        (x, d) => new
        {
            Employee = x.Employee,
            Department = d
        })
    .Where(x => x.Department == null)
    .Select(x => x.Employee);
```

Query:
```
var result =
    from e in employees
    join d in departments
        on e.DepartmentId equals d.Id
        into departmentGroup
    from d in departmentGroup.DefaultIfEmpty()
    where d == null
    select e;
```
---
**Find the department with the highest number of employees.**

SQL:
```
SELECT TOP 1
    d.Name AS DepartmentName,
    COUNT(e.Id) AS EmployeeCount
FROM Employee e
INNER JOIN Department d
    ON e.DepartmentId = d.Id
GROUP BY d.Id, d.Name
ORDER BY COUNT(e.Id) DESC;
```

Method:
```
var result = employees
    .Join(
        departments,
        e => e.DepartmentId,
        d => d.Id,
        (e, d) => new
        {
            Employee = e,
            Department = d
        })
    .GroupBy(x => new
    {
        x.Department.Id,
        x.Department.Name
    })
    .Select(g => new
    {
        DepartmentId = g.Key.Id,
        DepartmentName = g.Key.Name,
        EmployeeCount = g.Count()
    })
    .OrderByDescending(x => x.EmployeeCount)
    .FirstOrDefault();

```

Query
```
var result =
    (from e in employees
     join d in departments
         on e.DepartmentId equals d.Id
     group e by new
     {
         d.Id,
         d.Name
     }
     into g
     orderby g.Count() descending
     select new
     {
         DepartmentId = g.Key.Id,
         DepartmentName = g.Key.Name,
         EmployeeCount = g.Count()
     })
    .FirstOrDefault();
```
---
**Find employees whose salary is greater than the average salary of their department.**


SQL:
```
SELECT
    e.Id,
    e.Name,
    e.Salary,
    d.Name AS DepartmentName
FROM Employees e
INNER JOIN Departments d
    ON d.Id = e.DepartmentId
WHERE e.Salary >
(
    SELECT AVG(e2.Salary)
    FROM Employees e2
    WHERE e2.DepartmentId = e.DepartmentId
);
;

```

Method:
```
var departmentAverages = employees
    .GroupBy(e => e.DepartmentId)
    .Select(g => new
    {
        DepartmentId = g.Key,
        AverageSalary = g.Average(e => e.Salary)
    });

var result = employees
    .Join(
        departmentAverages,
        e => e.DepartmentId,
        a => a.DepartmentId,
        (e, a) => new
        {
            Employee = e,
            AverageSalary = a.AverageSalary
        })
    .Where(x => x.Employee.Salary > x.AverageSalary)
    .Select(x => x.Employee);
```

Query:
```
var departmentAverages =
    from e in employees
    group e by e.DepartmentId
    into g
    select new
    {
        DepartmentId = g.Key,
        AverageSalary = g.Average(x => x.Salary)
    };

var result =
    from e in employees
    join a in departmentAverages
        on e.DepartmentId equals a.DepartmentId
    where e.Salary > a.AverageSalary
    select e;
```
----
**Find the highest-paid employee in each department. If two employees have the same highest salary, return both.**

SQL:
```
SELECT
    e.*,
    d.Name AS DepartmentName
FROM Employees e
INNER JOIN Departments d
    ON d.Id = e.DepartmentId
WHERE e.Salary =
(
    SELECT MAX(e2.Salary)
    FROM Employees e2
    WHERE e2.DepartmentId = e.DepartmentId
);

```

Method Syntax:
```
var result = employees
    .Join(
        maxSalaryTable,
        e => e.DepartmentId,
        a => a.DepartmentId,
        (e, a) => new
        {
            Employee = e,
            MaxSalary = a.Max
        })
    .Where(x => x.Employee.Salary == x.MaxSalary)
    .Select(x => x.Employee);


///
var result = employees
    .GroupBy(e => e.DepartmentId)
    .SelectMany(g =>
        g.Where(e => e.Salary == g.Max(x => x.Salary)));
```

Query Syntax:
```
var departmentMax =
    from e in employees
    group e by e.DepartmentId
    into g
    select new
    {
        DepartmentId = g.Key,
        Max = g.Max(x => x.Salary)
    };

var result =
    from e in employees
    join a in departmentMax
        on e.DepartmentId equals a.DepartmentId
    where e.Salary == a.Max
    select e;

```