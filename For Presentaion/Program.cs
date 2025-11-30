using System;
using System.IO;

namespace DAILYFITNESS
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
            this.Date = date.Date;
            this.Value = value;
            this.Mood = mood;
        }

        public void Update(bool value, Mood mood)
        {
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
            this.Date = date.Date;
            this.Value = value;
            this.Mood = mood;
        }

        public void Update(int value, Mood mood)
        {
            this.Value = value;
            this.Mood = mood;
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
        public HabitCategory Category { get; private set; }

        public Habit(string name, HabitCategory category)
        {
            this.Name = name;
            this.Category = category;
        }

        public void DisplayName()
        {
            Console.WriteLine("Habit: " + this.Name);
        }

        public abstract void LogProgressFromConsole();
        public abstract bool WasSuccessfulOn(DateTime date);
        public abstract Mood GetMoodForDay(DateTime date);

        public abstract void Save(StreamWriter writer);
    }

    public class Habitation : Habit, IAnalytics
    {
        private DailyLog[] logs;
        private int CountLogs;

        public Habitation(string name, HabitCategory category) : base(name, category)
        {
            this.logs = new DailyLog[100];
            this.CountLogs = 0;
        }

        public void AddHistoricLog(DateTime date, bool value, Mood mood)
        {
            if (this.CountLogs < this.logs.Length)
            {
                this.logs[this.CountLogs] = new DailyLog(date, value, mood);
                this.CountLogs++;
            }
        }

        public override void Save(StreamWriter writer)
        {
            writer.WriteLine("Habitation");
            writer.WriteLine(this.Name);
            writer.WriteLine(this.Category);
            writer.WriteLine(this.CountLogs);

            for (int i = 0; i < this.CountLogs; i++)
            {
                string line = $"{this.logs[i].Date.ToShortDateString()}|{this.logs[i].Value}|{this.logs[i].Mood}";
                writer.WriteLine(line);
            }
            writer.WriteLine("----------------------------------------");
        }


        public void LogToday(bool complete, Mood mood)
        {
            DateTime today = DateTime.Today;
            for (int i = 0; i < CountLogs; i++)
            {
                if (logs[i].Date == today)
                {
                    logs[i].Update(complete, mood);
                    Console.WriteLine($"\nLog for {today.ToShortDateString()} UPDATED to: {complete} (Mood: {mood})");
                    return;
                }
            }
            if (this.CountLogs < this.logs.Length)
            {
                this.logs[this.CountLogs] = new DailyLog(today, complete, mood);
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
            int currentStreak = 0;
            DateTime dayToCheck = DateTime.Today;
            while (true)
            {
                bool foundLogForDay = false;
                for (int i = 0; i < CountLogs; i++)
                {
                    if (logs[i].Date == dayToCheck)
                    {
                        if (logs[i].Value == true)
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
                if (this.logs[i].Value == true)
                {
                    successCount++;
                }
            }
            double rate = (double)successCount / this.CountLogs * 100.0;
            return Math.Round(rate, 2);
        }

        public override Mood GetMoodForDay(DateTime date)
        {
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
            for (int i = 0; i < CountLogs; i++)
            {
                if (logs[i].Date == date.Date)
                {
                    return logs[i].Value;
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
        public string Unit { get; private set; }

        public Quantity(string name, int target, string unit, HabitCategory category) : base(name, category)
        {
            this.DailyTarget = target;
            this.Unit = unit;
            this.logs = new QuantityLog[100];
            this.CountLogs = 0;
        }

        public int GetCurrentTodayValue()
        {
            DateTime today = DateTime.Today;
            for (int i = 0; i < CountLogs; i++)
            {
                if (logs[i].Date == today)
                {
                    return logs[i].Value;
                }
            }
            return 0;
        }

        public void AddHistoricLog(DateTime date, int value, Mood mood)
        {
            if (this.CountLogs < this.logs.Length)
            {
                this.logs[this.CountLogs] = new QuantityLog(date, value, mood);
                this.CountLogs++;
            }
        }

        public override void Save(StreamWriter writer)
        {
            writer.WriteLine("Quantity");
            writer.WriteLine(this.Name);
            writer.WriteLine(this.Category);
            writer.WriteLine(this.DailyTarget);
            writer.WriteLine(this.Unit);
            writer.WriteLine(this.CountLogs);

            for (int i = 0; i < this.CountLogs; i++)
            {
                string line = $"{this.logs[i].Date.ToShortDateString()}|{this.logs[i].Value}|{this.logs[i].Mood}";
                writer.WriteLine(line);
            }
            writer.WriteLine("----------------------------------------");
        }

        public void LogToday(int amountToAdd, Mood mood)
        {
            DateTime today = DateTime.Today;
            QuantityLog existingLog = null;

            for (int i = 0; i < CountLogs; i++)
            {
                if (logs[i].Date == today)
                {
                    existingLog = logs[i];
                    break;
                }
            }

            if (existingLog != null)
            {
                int currentTotal = existingLog.Value;
                int newTotal = currentTotal + amountToAdd;
                bool isExceeding = newTotal > this.DailyTarget;
                bool wasExceeding = currentTotal >= this.DailyTarget;

                if (isExceeding && !wasExceeding)
                {
                    Console.WriteLine($"\nWARNING: Your daily goal is {this.DailyTarget} {this.Unit}.");
                    Console.WriteLine($"You already have {currentTotal} {this.Unit} logged.");
                    Console.WriteLine($"Adding {amountToAdd} {this.Unit} will bring your new total to {newTotal} {this.Unit}.");
                    Console.Write("Do you still want to add this amount? (yes/no): ");
                    string confirm = Console.ReadLine().Trim().ToLower();

                    if (confirm != "yes" && confirm != "y")
                    {
                        Console.WriteLine($"Log cancelled. Your total for today is still {currentTotal} {this.Unit}.");
                        return;
                    }
                }

                existingLog.Update(newTotal, mood);
                Console.WriteLine($"\nLog for {today.ToShortDateString()} UPDATED. New total: {newTotal} {this.Unit} (Mood: {mood})");
            }
            else
            {
                if (amountToAdd > this.DailyTarget)
                {
                    Console.WriteLine($"\nWARNING: Your daily goal is {this.DailyTarget} {this.Unit}.");
                    Console.WriteLine($"You are about to log {amountToAdd} {this.Unit} (exceeding your goal).");
                    Console.Write("Do you still want to log this amount? (yes/no): ");
                    string confirm = Console.ReadLine().Trim().ToLower();

                    if (confirm != "yes" && confirm != "y")
                    {
                        Console.WriteLine("Log cancelled.");
                        return;
                    }
                }

                if (this.CountLogs < this.logs.Length)
                {
                    this.logs[this.CountLogs] = new QuantityLog(today, amountToAdd, mood);
                    this.CountLogs++;
                    Console.WriteLine($"\nLogged {amountToAdd} {this.Unit} for {this.Name} on {today.ToShortDateString()} (Mood: {mood}).");
                }
                else
                {
                    Console.WriteLine("Error: Logbook is full.");
                }
            }
        }

        public override void LogProgressFromConsole()
        {
            int amountToAdd;
            Console.Write($"Enter amount to ADD for '{this.Name}' (Goal: {this.DailyTarget} {this.Unit}): ");
            while (!int.TryParse(Console.ReadLine(), out amountToAdd) || amountToAdd < 0)
            {
                Console.Write("Invalid input. Please enter a positive number: ");
            }

            Mood selectedMood = Program.SelectMood();
            this.LogToday(amountToAdd, selectedMood);
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

        public override Mood GetMoodForDay(DateTime date)
        {
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

        private static string saveFilePath;

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
                "Save Data",
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
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFilePath = Path.Combine(documentsPath, "habits.txt");

            LoadData();

            int selectedIndex = 0;
            bool appIsRunning = true;

            int maxMenuIndex = 7;

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
                            selectedIndex = maxMenuIndex;
                        }
                        break;

                    case ConsoleKey.DownArrow:
                        selectedIndex++;
                        if (selectedIndex > maxMenuIndex)
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
                                SaveData();
                                Console.WriteLine("\nPress any key to return to menu...");
                                Console.ReadKey();
                                break;
                            case 7:
                                appIsRunning = false;
                                Console.WriteLine("Goodbye!");
                                break;
                        }
                        break;
                }
            }
        }

        public static void SaveData()
        {
            try
            {
                using (StreamWriter writer = File.CreateText(saveFilePath))
                {
                    for (int i = 0; i < habitCount; i++)
                    {
                        allHabits[i].Save(writer);
                    }
                    writer.Flush();
                }
                Console.WriteLine($"File Saved Successfully to: {saveFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving data to {saveFilePath}: {ex.Message}");
            }
        }

        public static void LoadData()
        {
            if (!File.Exists(saveFilePath))
            {
                return;
            }

            try
            {
                using (StreamReader reader = File.OpenText(saveFilePath))
                {
                    string habitType;
                    while ((habitType = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(habitType) || habitType.StartsWith("-")) continue;

                        if (habitCount >= allHabits.Length)
                        {
                            break;
                        }

                        if (habitType == "Habitation")
                        {
                            string name = reader.ReadLine();
                            HabitCategory category = (HabitCategory)Enum.Parse(typeof(HabitCategory), reader.ReadLine());
                            Habitation habit = new Habitation(name, category);

                            int logCount = int.Parse(reader.ReadLine());
                            for (int i = 0; i < logCount; i++)
                            {
                                string logLine = reader.ReadLine();
                                string[] parts = logLine.Split('|');

                                DateTime date = DateTime.Parse(parts[0]);
                                bool value = bool.Parse(parts[1]);
                                Mood mood = (Mood)Enum.Parse(typeof(Mood), parts[2]);
                                habit.AddHistoricLog(date, value, mood);
                            }
                            reader.ReadLine(); // consume separator

                            allHabits[habitCount] = habit;
                            habitCount++;
                        }
                        else if (habitType == "Quantity")
                        {
                            string name = reader.ReadLine();
                            HabitCategory category = (HabitCategory)Enum.Parse(typeof(HabitCategory), reader.ReadLine());
                            int target = int.Parse(reader.ReadLine());
                            string unit = reader.ReadLine();

                            Quantity habit = new Quantity(name, target, unit, category);

                            int logCount = int.Parse(reader.ReadLine());
                            for (int i = 0; i < logCount; i++)
                            {
                                string logLine = reader.ReadLine();
                                string[] parts = logLine.Split('|');

                                DateTime date = DateTime.Parse(parts[0]);
                                int value = int.Parse(parts[1]);
                                Mood mood = (Mood)Enum.Parse(typeof(Mood), parts[2]);
                                habit.AddHistoricLog(date, value, mood);
                            }
                            reader.ReadLine(); // consume separator

                            allHabits[habitCount] = habit;
                            habitCount++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data (File might be old format): {ex.Message}");
                Console.WriteLine("Starting with a fresh session.");
                Console.ReadKey();
                allHabits = new Habit[10];
                habitCount = 0;
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
                    Console.Write("Enter Daily Target (e.g., 8, 30, 10): ");
                    if (int.TryParse(Console.ReadLine(), out target) && target > 0)
                    {
                        Console.Write("Enter the unit of measurement (e.g., cups, minutes, km): ");
                        string unit = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(unit))
                        {
                            unit = "units";
                        }

                        allHabits[habitCount] = new Quantity(name, target, unit, selectedCategory);
                        Console.WriteLine($"\n'Measure' habit '{name}' ({target} {unit}) added.");
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

            Console.WriteLine("\n" + new string('-', 30));
            if (selectedHabit is Quantity quantityHabit)
            {
                int currentVal = quantityHabit.GetCurrentTodayValue();
                Console.WriteLine($"UPDATED Progress: {currentVal} / {quantityHabit.DailyTarget} {quantityHabit.Unit} logged today.");
            }
            else if (selectedHabit is Habitation habitationHabit)
            {
                bool isDone = habitationHabit.WasSuccessfulOn(DateTime.Today);
                Console.WriteLine($"UPDATED Status: {(isDone ? "Completed" : "Not yet completed")} for today.");
            }

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

            Console.WriteLine($"  {"[#]",-4} {"Name",-25} {"Today's Progress",-20} {"Category",-12} {"Streak",-10} {"Success",-12} {"Mood",-10}");
            Console.WriteLine("  " + new string('-', 95));

            for (int i = 0; i < habitCount; i++)
            {
                Habit habit = allHabits[i];
                IAnalytics analytics = (IAnalytics)habit;
                Mood todayMood = habit.GetMoodForDay(DateTime.Today);

                string name = habit.Name;
                if (name.Length > 25)
                {
                    name = name.Substring(0, 22) + "...";
                }

                string progress = "N/A (Day)";
                if (habit is Quantity quantityHabit)
                {
                    int currentVal = quantityHabit.GetCurrentTodayValue();
                    progress = $"{currentVal} / {quantityHabit.DailyTarget} {quantityHabit.Unit}";
                }
                if (progress.Length > 20)
                {
                    progress = progress.Substring(0, 17) + "...";
                }

                string category = habit.Category.ToString();
                string streak = analytics.StreakDay().ToString() + " days";
                string success = analytics.SuccessRate().ToString() + "%";
                string mood = todayMood == Mood.Unknown ? "N/A" : todayMood.ToString();
                string index = $"[{i + 1}]";

                Console.WriteLine($"  {index,-4} {name,-25} {progress,-20} {category,-12} {streak,-10} {success,-12} {mood,-10}");
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

            // Updated header with Status
            Console.WriteLine($"  {"[#]",-4} {"Name / Goal",-30} {"Category",-12} {"Status"}");
            Console.WriteLine("  " + new string('-', 60));

            for (int i = 0; i < habitCount; i++)
            {
                string name = allHabits[i].Name;
                if (allHabits[i] is Quantity quantityHabit)
                {
                    name = $"{allHabits[i].Name} ({quantityHabit.DailyTarget} {quantityHabit.Unit})";
                }

                if (name.Length > 30)
                {
                    name = name.Substring(0, 27) + "...";
                }
                string index = $"[{i + 1}]";
                string category = allHabits[i].Category.ToString();

                // Check for completion today
                bool isDone = allHabits[i].WasSuccessfulOn(DateTime.Today);
                string status = isDone ? "[DONE]" : "[ ]";

                Console.WriteLine($"  {index,-4} {name,-30} {category,-12} {status}");
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