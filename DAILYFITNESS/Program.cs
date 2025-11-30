using System;



public class DailyLog
{
    public DateTime Date { get; private set; }
    public bool Value { get; private set; }
    public DailyLog(DateTime date, bool value)
    {
        this.Date = date.Date;
        this.Value = value;
    }
    public void Update(bool value)
    {
        this.Value = value;
    }
}
public class QuantityLog
{
    public DateTime Date { get; private set; }
    public int Value { get; private set; }
    public QuantityLog(DateTime date, int value)
    {
        this.Date = date.Date;
        this.Value = value;
    }
    public void Update(int value)
    {
        this.Value = value;
    }
}



public interface IAnalytics
{
    int StreakDay();
    double SuccessRate();
}



public abstract class Habit
{
    public string Name { get; private set; }
    public Habit(string name)
    {
        this.Name = name;
    }
    public void DisplayName()
    {
        Console.WriteLine("Habit: " + this.Name);
    }
    public abstract void LogProgressFromConsole();
}



public class Habitation : Habit, IAnalytics
{
    private DailyLog[] _logs;
    private int CountLogs;
    public Habitation(string name) : base(name)
    {
        this._logs = new DailyLog[100];
        this.CountLogs = 0;
    }
    public void LogToday(bool complete)
    {
        DateTime today = DateTime.Today;
        for (int i = 0; i < CountLogs; i++)
        {
            if (_logs[i].Date == today)
            {
                _logs[i].Update(complete);
                Console.WriteLine($"\nLog for {today.ToShortDateString()} UPDATED to: {complete}");
                return;
            }
        }
        if (this.CountLogs < this._logs.Length)
        {
            this._logs[this.CountLogs] = new DailyLog(today, complete);
            this.CountLogs++;
            Console.WriteLine($"\nLogged {complete} for {this.Name} on {today.ToShortDateString()}.");
        }
        else
        {
            Console.WriteLine("Error: Logbook is full.");
        }
    }
    public override void LogProgressFromConsole()
    {
        bool value;
        Console.Write($"Did you complete '{this.Name}'? (true/false): ");
        while (!bool.TryParse(Console.ReadLine(), out value))
        {
            Console.Write("Invalid input. Please enter 'true' or 'false': ");
        }
        this.LogToday(value);
    }
    public int StreakDay()
    {
        int currentStreak = 0;
        DateTime dayToCheck = DateTime.Today;
        while (true)
        {
            bool foundLogForDay = false;
            for (int i = 0; i < CountLogs; i++)
            {
                if (_logs[i].Date == dayToCheck)
                {
                    if (_logs[i].Value == true)
                    {
                        currentStreak++;
                        foundLogForDay = true;
                    }
                    break;
                }
            }
            if (foundLogForDay)
            {
                dayToCheck = dayToCheck.AddDays(-1);
            }
            else
            {
                break;
            }
        }
        return currentStreak;
    }
    public double SuccessRate()
    {
        if (this.CountLogs == 0) return 0.0;
        int successCount = 0;
        for (int i = 0; i < this.CountLogs; i++)
        {
            if (this._logs[i].Value == true)
            {
                successCount++;
            }
        }
        double rate = (double)successCount / this.CountLogs * 100.0;
        return Math.Round(rate, 2);
    }
}



public class Quantity : Habit, IAnalytics
{
    private QuantityLog[] logs;
    private int CountLogs;
    public int DailyTarget { get; private set; }
    public Quantity(string name, int target) : base(name)
    {
        this.DailyTarget = target;
        this.logs = new QuantityLog[100];
        this.CountLogs = 0;
    }
    public void LogToday(int amount)
    {
        DateTime today = DateTime.Today;
        for (int i = 0; i < CountLogs; i++)
        {
            if (logs[i].Date == today)
            {
                logs[i].Update(amount);
                Console.WriteLine($"\nLog for {today.ToShortDateString()} UPDATED to: {amount}");
                return;
            }
        }
        if (this.CountLogs < this.logs.Length)
        {
            this.logs[this.CountLogs] = new QuantityLog(today, amount);
            this.CountLogs++;
            Console.WriteLine($"\nLogged {amount} for {this.Name} on {today.ToShortDateString()}.");
        }
        else
        {
            Console.WriteLine("Error: Logbook is full.");
        }
    }
    public override void LogProgressFromConsole()
    {
        int amount;
        Console.Write($"Enter amount for '{this.Name}': ");
        while (!int.TryParse(Console.ReadLine(), out amount) || amount < 0)
        {
            Console.Write("Invalid input. Please enter a positive number: ");
        }
        this.LogToday(amount);
    }
    public int StreakDay()
    {
        int currentStreak = 0;
        DateTime dayToCheck = DateTime.Today;
        while (true)
        {
            bool foundLogForDay = false;
            for (int i = 0; i < CountLogs; i++)
            {
                if (logs[i].Date == dayToCheck)
                {
                    if (logs[i].Value >= this.DailyTarget)
                    {
                        currentStreak++;
                        foundLogForDay = true;
                    }
                    break;
                }
            }
            if (foundLogForDay)
            {
                dayToCheck = dayToCheck.AddDays(-1);
            }
            else
            {
                break;
            }
        }
        return currentStreak;
    }
    public double SuccessRate()
    {
        if (this.CountLogs == 0) return 0.0;
        int successCount = 0;
        for (int i = 0; i < this.CountLogs; i++)
        {
            if (this.logs[i].Value >= this.DailyTarget)
            {
                successCount++;
            }
        }
        double rate = (double)successCount / this.CountLogs * 100.0;
        return Math.Round(rate, 2);
    }
}



public class Program
{
    public static Habit[] allHabits = new Habit[10];
    public static int habitCount = 0;
    public static void Main(string[] args)
    {
        bool Rep = true;
        while (Rep)
        {
            Console.WriteLine("\n--- Nordstrom Habit Tracker ---");
            Console.WriteLine("1. Add New Habit");
            Console.WriteLine("2. Log Progress for a Habit");
            Console.WriteLine("3. View All Habit Stats");
            Console.WriteLine("4. Remove Habit");
            Console.WriteLine("5. List All Habits");
            Console.WriteLine("6. Exit");
            Console.Write("Choose an option (1-6): ");
            string choice = Console.ReadLine();
            Console.WriteLine();
            switch (choice)
            {
                case "1":
                    AddNewHabit();
                    break;
                case "2":
                    LogProgress();
                    break;
                case "3":
                    ViewStats();
                    break;
                case "4":
                    RemoveHabit();
                    break;
                case "5":
                    ListAllHabits();
                    break;
                case "6":
                    Rep = false;
                    Console.WriteLine("Goodbye!");
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please enter a number from 1 to 5.");
                    break;
            }
        }
    }
    public static void AddNewHabit()
    {
        if (habitCount >= allHabits.Length)
        {
            Console.WriteLine("Habit list is full.");
            return;
        }
        Console.Write("Enter Habit Name: ");
        string name = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Name cannot be empty. Habit not added.");
            return;
        }
        Console.Write("Select Type: 1(Yes/No)  2(Number): ");
        string type = Console.ReadLine();
        if (type == "1")
        {
            allHabits[habitCount] = new Habitation(name);
            Console.WriteLine($"Habit '{name}' added.");
            habitCount++;
        }
        else if (type == "2")
        {
            int target;
            Console.Write("Enter Daily Target (e.g., 8 for 8 cups): ");
            if (int.TryParse(Console.ReadLine(), out target) && target > 0)
            {
                allHabits[habitCount] = new Quantity(name, target);
                Console.WriteLine($"Quantity'{name}' with target {target} added.");
                habitCount++;
            }
            else
            {
                Console.WriteLine("Invalid target. Must be a positive number. Habit not added.");
            }
        }
        else
        {
            Console.WriteLine("Invalid type. Habit not added.");
        }
    }
    public static void LogProgress()
    {
        if (habitCount == 0)
        {
            Console.WriteLine("No habits added yet. Please add a habit first.");
            return;
        }
        Console.WriteLine("Which habit do you want to log?");
        ListAllHabits();
        Console.Write("Enter habit number: ");
        int habitIndex;
        if (!int.TryParse(Console.ReadLine(), out habitIndex) || habitIndex < 1 || habitIndex > habitCount)
        {
            Console.WriteLine("Invalid habit number.");
            return;
        }
        Habit selectedHabit = allHabits[habitIndex - 1];
        selectedHabit.LogProgressFromConsole();
    }
    public static void ViewStats()
    {
        if (habitCount == 0)
        {
            Console.WriteLine("No habits to show.");
            return;
        }
        Console.WriteLine("--- CURRENT HABIT STATS ---");
        for (int i = 0; i < habitCount; i++)
        {
            Habit habit = allHabits[i];
            IAnalytics analytics = (IAnalytics)habit;
            Console.WriteLine($"\n[{i + 1}] {habit.Name}");
            Console.WriteLine($"    Streak: {analytics.StreakDay()} days");
            Console.WriteLine($"    Success: {analytics.SuccessRate()}%");
        }
        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey();
    }
    public static void RemoveHabit()
    {
        if (habitCount == 0)
        {
            Console.WriteLine("No habits to remove.");
            return;
        }
        Console.WriteLine("Which habit do you want to remove?");
        ListAllHabits();
        Console.Write("Enter habit number to remove: ");
        int habitIndex;
        if (!int.TryParse(Console.ReadLine(), out habitIndex) || habitIndex < 1 || habitIndex > habitCount)
        {
            Console.WriteLine("Invalid habit number.");
            return;
        }
        int indexToRemove = habitIndex - 1;
        string removedHabitName = allHabits[indexToRemove].Name;
        for (int i = indexToRemove; i < habitCount - 1; i++)
        {
            allHabits[i] = allHabits[i + 1];
        }
        allHabits[habitCount - 1] = null;
        habitCount--;
        Console.WriteLine($"Habit '{removedHabitName}' has been removed.");
    }
    public static void ListAllHabits()
    {
        Console.WriteLine("Your Habits:");
        for (int i = 0; i < habitCount; i++)
        {
            Console.WriteLine($"  {i + 1}. {allHabits[i].Name}");
        }
    }
}