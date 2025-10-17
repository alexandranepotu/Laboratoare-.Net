var project1 = new Project("Lab2", new List<Task> { new Task("Task 1", false, "2022-01-01"), new Task("Task 2", false, "2022-01-02")});
var project2 = project1 with {Tasks = new List<Task>(project1.Tasks) {new Task("Task 3", false, "2022-01-03") }};
Console.WriteLine(project1);
Console.WriteLine(project2);

var manager = new Manager
{
    Name = "Alexandra",
    Team = "Team 1",
    Email = "alexandra@gmail.com"
};
Console.WriteLine(manager);

var tasks = new List<Task>();
while (true)
{
    Console.WriteLine("Enter a task title(or press enter to exit): ");
    var title = Console.ReadLine();
    if (string.IsNullOrEmpty(title))
    {
        break; 
    }
    
    var task = new Task(title, true, DateTime.Now.ToString("yyyy-MM-dd")); //data curenta
    tasks.Add(task);
}

foreach (var task in tasks)
{
    Console.WriteLine(task);
}

static void PrintType(object obj)
{
    switch (obj)
    {
        case Task task when task is not null:
            Console.WriteLine($"Name: {task.Title}, Status: {task.IsCompleted}");
            break;
        case Project project when project is not null:
            Console.WriteLine($"Name: {project.Name}, Number of tasks: {project.Tasks.Count}");
            break;
        default:
            Console.WriteLine("Unknown type");
            break;       
    }
}

PrintType(project1);
PrintType(project2);
PrintType(manager);

var listTasks = new List<Task>{new Task("Task 1", false, "2022-01-01"), new Task("Task 2", true, "2024-01-02"), new Task("Task 3", false, "2025-10-10")};
Func<Task, bool> filter = t => !t.IsCompleted && DateTime.Parse(t.DueDate) < DateTime.Now;
var filteredTasks = listTasks.Where(filter);
foreach (var task in filteredTasks)
{
    Console.WriteLine(task);
}

public record Task(string Title, bool IsCompleted, string DueDate)
{
    public override string ToString() => $"Title: {Title}, Status: {IsCompleted}, Due date: {DueDate}";
}

public record Project(string Name, List<Task> Tasks)
{
    public override string ToString() => $"Name: {Name}, Tasks: {string.Join(", ", Tasks.Select(t => $"{t.Title} {t.IsCompleted} {t.DueDate}"))}"; 
}

public class Manager
{
    public string Name { get; init; }
    public string Team { get; init; }
    public string Email { get; init; }
    public override string ToString() => $"Name: {Name} Team: {Team} Email: {Email}";
}
