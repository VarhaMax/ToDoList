var tasks = new List<ChecklistTask>();
var nextId = 1;

while (true)
{
    DrawHeader("YOUR CHECKLIST");
    Console.WriteLine($"  {tasks.Count(task => !task.IsDone)} pending  /  {tasks.Count(task => task.IsDone)} completed");
    Console.WriteLine("\n  [1]  View tasks\n  [2]  Add a task\n  [3]  Mark as done\n  [4]  Remove a task\n\n  [0]  Exit\n");
    Console.Write("Choose an option: ");
    var choice = Console.ReadLine();
    if (choice is null or "0")
        break;

    choice = choice.Trim();
    if (choice == "0")
        break;
    DrawHeader(choice switch
    {
        "1" => "YOUR TASKS",
        "2" => "ADD A TASK",
        "3" => "COMPLETE A TASK",
        "4" => "REMOVE A TASK",
        _ => "CHOOSE AN ACTION"
    });

    switch (choice)
    {
        case "0":
            return;
        case "1":
            DrawTasks(tasks);
            break;
        case "2":
            Console.Write("Task title: ");
            var title = Console.ReadLine();
            if (title is null)
                return;
            if (string.IsNullOrWhiteSpace(title))
            {
                Console.WriteLine("A task title cannot be empty.");
                break;
            }
            if (title.Any(char.IsControl))
            {
                Console.WriteLine("A task title cannot contain control characters.");
                break;
            }
            var addedTask = new ChecklistTask(nextId++, title.Trim());
            tasks.Add(addedTask);
            Console.WriteLine($"Added task {addedTask.Id}.");
            break;
        case "3":
        case "4":
            if (tasks.Count == 0)
            {
                Console.WriteLine("Your checklist is empty. Choose 2 to add a task.");
                break;
            }
            DrawTasks(tasks);
            Console.Write("\nTask ID: ");
            var input = Console.ReadLine();
            if (input is null)
                return;
            if (!int.TryParse(input, out var id) || id <= 0)
            {
                Console.WriteLine("Enter a positive numeric task ID.");
                break;
            }
            var selectedTask = tasks.Find(task => task.Id == id);
            if (selectedTask is null)
            {
                Console.WriteLine($"Task {id} was not found. Choose 1 to list task IDs.");
                break;
            }
            if (choice == "4")
            {
                tasks.Remove(selectedTask);
                Console.WriteLine($"Removed task {id}.");
            }
            else if (selectedTask.IsDone)
                Console.WriteLine($"Task {id} is already done.");
            else
            {
                selectedTask.IsDone = true;
                Console.WriteLine($"Marked task {id} done.");
            }
            break;
        default:
            Console.WriteLine("Invalid option. Choose 0, 1, 2, 3, or 4.");
            break;
    }

    if (!Console.IsInputRedirected)
    {
        Console.Write("\nPress Enter to return to the menu...");
        if (Console.ReadLine() is null)
            break;
    }
}

static void DrawHeader(string title)
{
    if (!Console.IsOutputRedirected)
    {
        try
        {
            Console.Clear();
        }
        catch (IOException)
        {
            // Some terminal hosts do not support clearing the screen.
        }
    }

    WriteColored("\n  C H E C K L I S T", ConsoleColor.Cyan);
    Console.WriteLine("  ----------------------------------------");
    Console.WriteLine($"  {title}");
    Console.WriteLine("  Tasks are saved for this session only.\n");
}

static void DrawTasks(List<ChecklistTask> tasks)
{
    if (tasks.Count == 0)
    {
        Console.WriteLine("  Your checklist is empty. Add a task to get started.");
        return;
    }

    Console.WriteLine("   ID   STATUS   TASK");
    Console.WriteLine("  ----------------------------------------");
    foreach (var task in tasks)
        WriteColored($"  {task.Id,3}   [{(task.IsDone ? "x" : " ")}]      {task.Title}",
            task.IsDone ? ConsoleColor.Green : ConsoleColor.White);
}

static void WriteColored(string text, ConsoleColor color)
{
    if (Console.IsOutputRedirected)
    {
        Console.WriteLine(text);
        return;
    }

    var previousColor = Console.ForegroundColor;
    try
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
    }
    finally
    {
        Console.ForegroundColor = previousColor;
    }
}

/// <summary>Represents a task in the current checklist session.</summary>
/// <param name="id">The positive, stable identifier of the task.</param>
/// <param name="title">The task title, containing visible text and no control characters.</param>
/// <exception cref="ArgumentOutOfRangeException">The identifier is not positive.</exception>
/// <exception cref="ArgumentException">The title is blank or contains control characters.</exception>
/// <exception cref="ArgumentNullException">The title is null.</exception>
sealed class ChecklistTask(int id, string title)
{
    /// <summary>Gets the stable identifier used to select this task.</summary>
    public int Id { get; } = id > 0
        ? id
        : throw new ArgumentOutOfRangeException(nameof(id), id, "Task IDs must be positive.");

    /// <summary>Gets the task title with surrounding whitespace removed.</summary>
    public string Title { get; } = title is null
        ? throw new ArgumentNullException(nameof(title))
        : string.IsNullOrWhiteSpace(title) || title.Any(char.IsControl)
            ? throw new ArgumentException("Task titles must contain text and no control characters.", nameof(title))
            : title.Trim();

    /// <summary>Gets or sets whether the task has been completed.</summary>
    public bool IsDone { get; set; }
}
