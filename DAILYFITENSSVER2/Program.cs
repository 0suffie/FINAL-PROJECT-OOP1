using System;
namespace DAILYFITENSSVER2
{
    public enum HabitCategory
    {
        General,
        School,
        Health,
        Sport,
        Work,
        Personal
    }

    public enum Mood
    {
        Unknown,
        Happy,
        Neutral,
        Sad,
        Stressed,
        Energized
    }

    public class DailyLog
    {
        public DateTime Date { get; private set; }
        public bool Value { get; private set; }
        public Mood Mood { get; private set; }

        public DailyLog(DateTime date, bool value, Mood mood)
        {
            // make new log
            this.Date = date.Date;
            this.Value = value;
            this.Mood = mood;
        }

        public void Update(bool value, Mood mood)
        {
            // change log
            this.Value = value;
            this.Mood = mood;
        }
    }

    public class QuantityLog
    {
        public DateTime Date { get; private set; }
        public int Value { get; private set; }
        public Mood Mood { get; private set; }

        public QuantityLog(DateTime date, int value, Mood mood)
        {
            // make number log
            this.Date = date.Date;
            this.Value = value;
            this.Mood = mood;
        }

        public void Update(int value, Mood mood)
        {
            // change number log
            this.Value = value;
            this.Mood = mood;
        }
    }

    public interface IAnalytics
    {
        // count good days
        int StreakDay();
        // how much good
        double SuccessRate();
    }

    public abstract class Habit
    {
        public string Name { get; private set; }
        public HabitCategory Category { get; private set; }

        public Habit(string name, HabitCategory category)
        {
            // new habit
            this.Name = name;
            this.Category = category;
        }

        public void DisplayName()
        {
            // show name
            Console.WriteLine("Habit: " + this.Name);
        }

        // get log
        public abstract void LogProgressFromConsole();
        // did good?
        public abstract bool WasSuccessfulOn(DateTime date);
        // feel what?
        public abstract Mood GetMoodForDay(DateTime date);
    }

    public class Habitation : Habit, IAnalytics
    {
        private DailyLog[] _logs;
        private int CountLogs;

        public Habitation(string name, HabitCategory category) : base(name, category)
        {
            // make yes/no habit
            this._logs = new DailyLog[100];
            this.CountLogs = 0;
        }

        public void LogToday(bool complete, Mood mood)
        {
            // save today log
            DateTime today = DateTime.Today;
            for (int i = 0; i < CountLogs; i++)
            {
                if (_logs[i].Date == today)
                {
                    _logs[i].Update(complete, mood);
                    Console.WriteLine($"\nLog for {today.ToShortDateString()} UPDATED to: {complete} (Mood: {mood})");
                    return;
                }
            }
            if (this.CountLogs < this._logs.Length)
            {
                this._logs[this.CountLogs] = new DailyLog(today, complete, mood);
                this.CountLogs++;
                Console.WriteLine($"\nLogged {complete} for {this.Name} on {today.ToShortDateString()} (Mood: {mood}).");
            }
            else
            {
                Console.WriteLine("Error: Logbook is full.");
            }
        }

        public override void LogProgressFromConsole()
        {
            // ask yes/no
            bool value;
            Console.Write($"Did you complete '{this.Name}'? (true/false): ");
            while (!bool.TryParse(Console.ReadLine(), out value))
            {
                Console.Write("Invalid input. Please enter 'true' or 'false': ");
            }

            Mood selectedMood = Program.SelectMood();
            this.LogToday(value, selectedMood);
        }

        public int StreakDay()
        {
            // count good days row
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
            // how much good
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

        public override Mood GetMoodForDay(DateTime date)
        {
            // find mood
            for (int i = 0; i < CountLogs; i++)
            {
                if (_logs[i].Date == date.Date)
                {
                    return _logs[i].Mood;
                }
            }
            return Mood.Unknown;
        }

        public override bool WasSuccessfulOn(DateTime date)
        {
            // check day good
            for (int i = 0; i < CountLogs; i++)
            {
                if (_logs[i].Date == date.Date)
                {
                    // --- SIMPLIFIED THIS LINE ---
                    return _logs[i].Value;
                }
            }
            return false;
        }
    }

    public class Quantity : Habit, IAnalytics
    {
        private QuantityLog[] logs;
        private int CountLogs;
        public int DailyTarget { get; private set; }

        public Quantity(string name, int target, HabitCategory category) : base(name, category)
        {
            // make number habit
            this.DailyTarget = target;
            this.logs = new QuantityLog[100];
            this.CountLogs = 0;
        }

        public void LogToday(int amount, Mood mood)
        {
            // save number log
            DateTime today = DateTime.Today;
            for (int i = 0; i < CountLogs; i++)
            {
                if (logs[i].Date == today)
                {
                    logs[i].Update(amount, mood);
                    Console.WriteLine($"\nLog for {today.ToShortDateString()} UPDATED to: {amount} (Mood: {mood})");
                    return;
                }
            }
            if (this.CountLogs < this.logs.Length)
            {
                this.logs[this.CountLogs] = new QuantityLog(today, amount, mood);
                this.CountLogs++;
                Console.WriteLine($"\nLogged {amount} for {this.Name} on {today.ToShortDateString()} (Mood: {mood}).");
            }
            else
            {
                Console.WriteLine("Error: Logbook is full.");
            }
        }

        public override void LogProgressFromConsole()
        {
            // ask number
            int amount;
            Console.Write($"Enter amount for '{this.Name}': ");
            while (!int.TryParse(Console.ReadLine(), out amount) || amount < 0)
            {
                Console.Write("Invalid input. Please enter a positive number: ");
            }

            Mood selectedMood = Program.SelectMood();
            this.LogToday(amount, selectedMood);
        }

        public int StreakDay()
        {
            // count good number days
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
            // how much good number
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

        public override Mood GetMoodForDay(DateTime date)
        {
            // find mood
            for (int i = 0; i < CountLogs; i++)
            {
                if (logs[i].Date == date.Date)
                {
                    return logs[i].Mood;
                }
            }
            return Mood.Unknown;
        }

        public override bool WasSuccessfulOn(DateTime date)
        {
            // check number good
            for (int i = 0; i < CountLogs; i++)
            {
                if (logs[i].Date == date.Date)
                {
                    return logs[i].Value >= this.DailyTarget;
                }
            }
            return false;
        }
    }
        public class Program
        {
            public static Habit[] allHabits = new Habit[10];
            public static int habitCount = 0;

            private static void DrawMenu(int selectedIndex)
            {
                Console.Clear();
                Console.WriteLine("-- Daily Fitness Habit Tracker --");
                Console.WriteLine("\n(Use Up/Down Arrows to navigate, Enter to select)\n");

                string[] menuOptions = {
                "Add New Habit",
                "Log Progress for a Habit",
                "View All Habit Stats",
                "Remove Habit",
                "List All Habits",
                "View Weekly Report",
                "Exit"
            };

                for (int i = 0; i < menuOptions.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.WriteLine($"  > {menuOptions[i]} <");
                    }
                    else
                    {
                        Console.WriteLine($"    {menuOptions[i]}");
                    }
                }
            }

            public static void Main(string[] args)
            {
                int selectedIndex = 0;
                bool appIsRunning = true;

                while (appIsRunning)
                {
                    DrawMenu(selectedIndex);

                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.UpArrow:
                            selectedIndex--;
                            if (selectedIndex < 0)
                            {
                                selectedIndex = 6;
                            }
                            break;

                        case ConsoleKey.DownArrow:
                            selectedIndex++;
                            if (selectedIndex > 6)
                            {
                                selectedIndex = 0;
                            }
                            break;

                        case ConsoleKey.Enter:
                            Console.Clear();
                            switch (selectedIndex)
                            {
                                case 0:
                                    AddNewHabit();
                                    break;
                                case 1:
                                    LogProgress();
                                    break;
                                case 2:
                                    ViewStats();
                                    break;
                                case 3:
                                    RemoveHabit();
                                    break;
                                case 4:
                                    ListAllHabits();
                                    Console.WriteLine("\nPress any key to return to menu...");
                                    Console.ReadKey();
                                    break;
                                case 5:
                                    ViewWeeklyReport();
                                    break;
                                case 6:
                                    appIsRunning = false;
                                    Console.WriteLine("Goodbye!");
                                    break;
                            }
                            break;
                    }
                }
            }

            public static HabitCategory SelectCategory()
            {
                string[] categoryNames = Enum.GetNames(typeof(HabitCategory));

                Console.WriteLine("Select a Category:");
                for (int i = 0; i < categoryNames.Length; i++)
                {
                    Console.WriteLine($"  {i + 1}. {categoryNames[i]}");
                }

                int selectedIndex;

                while (true)
                {
                    Console.Write($"Enter number (1-{categoryNames.Length}): ");
                    string input = Console.ReadLine();

                    if (int.TryParse(input, out selectedIndex))
                    {
                        if (selectedIndex >= 1 && selectedIndex <= categoryNames.Length)
                        {
                            break;
                        }
                        else
                        {
                            Console.WriteLine($"Invalid choice. Please enter a number between 1 and {categoryNames.Length}.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter a number.");
                    }
                }
                return (HabitCategory)(selectedIndex - 1);
            }

            public static Mood SelectMood()
            {
                string[] moodNames = Enum.GetNames(typeof(Mood));

                Console.WriteLine("How are you feeling? (Optional)");
                for (int i = 0; i < moodNames.Length; i++)
                {
                    Console.WriteLine($"  {i + 1}. {moodNames[i]}");
                }

                int selectedIndex;

                while (true)
                {
                    Console.Write($"Enter number (1-{moodNames.Length}): ");
                    string input = Console.ReadLine();

                    if (int.TryParse(input, out selectedIndex))
                    {
                        if (selectedIndex >= 1 && selectedIndex <= moodNames.Length)
                        {
                            break;
                        }
                        else
                        {
                            Console.WriteLine($"Invalid choice. Please enter a number between 1 and {moodNames.Length}.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter a number.");
                    }
                }
                return (Mood)(selectedIndex - 1);
            }

            public static void AddNewHabit()
            {
                if (habitCount >= allHabits.Length)
                {
                    Console.WriteLine("Habit list is full.");
                    Console.ReadKey();
                    return;
                }

                Console.Write("Enter Habit Name: ");
                string name = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(name))
                {
                    Console.WriteLine("Name cannot be empty. Habit not added.");
                    Console.ReadKey();
                    return;
                }

                if (double.TryParse(name, out _))
                {
                    Console.WriteLine("Habit name cannot be just a number. Please use a descriptive name. Habit not added.");
                    Console.ReadKey();
                    return;
                }

                for (int i = 0; i < habitCount; i++)
                {
                    if (allHabits[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"A habit named '{name}' already exists. Habit not added.");
                        Console.ReadKey();
                        return;
                    }
                }

                HabitCategory selectedCategory = SelectCategory();

                while (true)
                {
                    Console.Write("Select Type: [1] Day (for Yes/No) or [2] Measure (for a number): ");
                    string type = Console.ReadLine().Trim();

                    if (type == "1")
                    {
                        allHabits[habitCount] = new Habitation(name, selectedCategory);
                        Console.WriteLine($"\n'Day' habit '{name}' ({selectedCategory}) added.");
                        habitCount++;
                        break;
                    }
                    else if (type == "2")
                    {
                        int target;
                        Console.Write("Enter Daily Target (e.g., 8 for 8 cups, 30 for 30 minutes): ");
                        if (int.TryParse(Console.ReadLine(), out target) && target > 0)
                        {
                            allHabits[habitCount] = new Quantity(name, target, selectedCategory);
                            Console.WriteLine($"\n'Measure' habit '{name}' ({selectedCategory}) with target {target} added.");
                            habitCount++;
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Invalid target. Must be a positive number. Habit not added.");
                            Console.ReadKey();
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid type. Please enter '1' or '2'.");
                    }
                }
                Console.WriteLine("\nPress any key to return to menu...");
                Console.ReadKey();
            }

            public static void LogProgress()
            {
                if (habitCount == 0)
                {
                    Console.WriteLine("No habits added yet. Please add a habit first.");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine("Which habit do you want to log?");
                ListAllHabits();

                Console.Write("Enter habit number: ");
                int habitIndex;
                if (!int.TryParse(Console.ReadLine(), out habitIndex) || habitIndex < 1 || habitIndex > habitCount)
                {
                    Console.WriteLine("Invalid habit number.");
                    Console.ReadKey();
                    return;
                }

                Habit selectedHabit = allHabits[habitIndex - 1];
                selectedHabit.LogProgressFromConsole();

                Console.WriteLine("\nPress any key to return to menu...");
                Console.ReadKey();
            }

            public static void ViewStats()
            {
                if (habitCount == 0)
                {
                    Console.WriteLine("No habits to show.");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine("--- CURRENT HABIT STATS ---");
                for (int i = 0; i < habitCount; i++)
                {
                    Habit habit = allHabits[i];
                    IAnalytics analytics = (IAnalytics)habit;
                    Mood todayMood = habit.GetMoodForDay(DateTime.Today);

                    Console.WriteLine($"\n[{i + 1}] {habit.Name}  (Category: {habit.Category})");
                    Console.WriteLine($"    Streak: {analytics.StreakDay()} days");
                    Console.WriteLine($"    Success: {analytics.SuccessRate()}%");

                    if (todayMood != Mood.Unknown)
                    {
                        Console.WriteLine($"    Today's Mood: {todayMood}");
                    }
                }
                Console.WriteLine("\nPress any key to return to menu...");
                Console.ReadKey();
            }

            public static void RemoveHabit()
            {
                if (habitCount == 0)
                {
                    Console.WriteLine("No habits to remove.");
                    Console.ReadKey();
                    return;
                }
                Console.WriteLine("Which habit do you want to remove?");
                ListAllHabits();
                Console.Write("Enter habit number to remove: ");
                int habitIndex;
                if (!int.TryParse(Console.ReadLine(), out habitIndex) || habitIndex < 1 || habitIndex > habitCount)
                {
                    Console.WriteLine("Invalid habit number.");
                    Console.ReadKey();
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
                Console.WriteLine("\nPress any key to return to menu...");
                Console.ReadKey();
            }

            public static void ListAllHabits()
            {
                if (habitCount == 0)
                {
                    Console.WriteLine("No habits added yet.");
                    return;
                }

                Console.WriteLine("Your Habits:");
                for (int i = 0; i < habitCount; i++)
                {
                    Console.WriteLine($"  {i + 1}. {allHabits[i].Name} ({allHabits[i].Category})");
                }
            }

            public static void ViewWeeklyReport()
            {
                if (habitCount == 0)
                {
                    Console.WriteLine("No habits to show. Add and log habits to see a report.");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine("--- Weekly Completion Report ---");
                Console.WriteLine("(Total habits completed per day for the last 7 days)\n");

                DateTime[] lastSevenDays = new DateTime[7];
                int[] completionCounts = new int[7];

                for (int i = 6; i >= 0; i--)
                {
                    DateTime dayToCheck = DateTime.Today.AddDays(-i);
                    int arrayIndex = 6 - i;
                    lastSevenDays[arrayIndex] = dayToCheck;
                    int dailySuccessCount = 0;

                    for (int j = 0; j < habitCount; j++)
                    {
                        Habit habit = allHabits[j];
                        if (habit.WasSuccessfulOn(dayToCheck))
                        {
                            dailySuccessCount++;
                        }
                    }
                    completionCounts[arrayIndex] = dailySuccessCount;
                }

                for (int i = 0; i < 7; i++)
                {
                    DateTime day = lastSevenDays[i];
                    int count = completionCounts[i];

                    string bar = "";
                    for (int j = 0; j < count; j++)
                    {
                        bar += "*";
                    }

                    string dayLabel = day.ToString("ddd, MMM dd");
                    Console.WriteLine($"{dayLabel.PadRight(12)}: {bar} ({count})");
                }

                Console.WriteLine("\nPress any key to return to menu...");
                Console.ReadKey();
            }
        }
    }
