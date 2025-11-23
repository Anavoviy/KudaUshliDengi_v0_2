using System.Runtime.InteropServices.JavaScript;
using KudaUshliDengi_v0_2.domain.models;
using KudaUshliDengi_v0_2.domain.valueobjects;
using Microsoft.Extensions.Options;

namespace KudaUshliDengi_v0_2.application.telegram.builders;

public static class MessageBuilder
{
    
    private static Dictionary<string, string> emojiMap = new()
    {
        ["доходы"] = "💸", ["расходы"] = "🧾", ["продукты"] = "🛒",
        ["развлечения"] = "🎭", ["личное"] = "👤", ["электроника"] = "📱",
        ["одежда"] = "👕", ["образование"] = "📚", ["красота"] = "💄",
        ["дом"] = "🏠", ["финансы"] = "💳", ["путешествие"] = "✈️",
        ["здоровье"] = "🏥", ["связь"] = "📞", ["транспорт"] = "🚗"
    };
    private static string HelpExtension //TODO: Дополнения, которые хотелось бы добавить 
        => """
           🧐 *УМНЫЕ ВОЗМОЖНОСТИ:*
           • *Автоисправление* - `коффе` → `кофе`
           • *Нечёткий поиск* - найду `машин` для `машина`
           • *Контекстные подсказки* - помогу исправить ошибки
           • *Геймификация* - бейджи и ачивки за прогресс
           """;
    public static string Help
        => """
           *ПОМОЩЬ ПО КОМАНДАМ*

           💳 *БЫСТРЫЙ ВВОД:*
           ```
           150 такси          - расход
           +50000 зарплата    - доход  
           5000 продукты:     - сложный расход
             1000 хоз, 
             650 вкусняшки
           ```
           📋 *ОСНОВНЫЕ КОМАНДЫ:*
           • `/start` - начать работу
           • `отмена` - отменить последнюю операцию  
           • `исправить` - редактировать операции
           • `категории` - управление категориями
           • `лимиты` - управление бюджетом
           • `цели` - управление копилками
           • `копилка машина 1000000` - создать цель
           • `5000 на машина` - пополнить цель
           • `25000 из обучение` - снять с цели
         
           🗃️ *УПРАВЛЕНИЕ КАТЕГОРИЯМИ:*
           • `новый доход фриланс` - создать категорию дохода
           • `новый развлечения` - создать категорию расхода
           • `новый кафе в развлечения` - подкатегория расхода
           • `x кафе` - удалить
           • `кафе это кофейня` - переименовать
           
           📈 *ОТЧЁТЫ И АНАЛИТИКА:*
           ```
           отчёт за неделю
           отчёт за месяц  
           отчёт за 2024
           отчёт за январь
           ```
           🤖 *ПОДДЕРЖКА:*
           Если что-то не работает или есть идеи - пиши @Anavoviy

           💡 *СОВЕТ:* Начни с простого - введи `150 кофе` и увидишь как это работает!
           """;

    public static string Welcome
        => """
           💸 *КудаУшлиДеньги* — твой умный помощник в учёте финансов!

           📋 *Что умею:*
           • Записывать расходы и доходы
           • Вести учёт по категориям  
           • Помогать копить на цели
           • Показывать аналитику

           💡 *Начни с простого:*
           ```
           150 такси
           +50000 зарплата
           5000 продукты: 1000 хоз, 650 вкусняшки
           ```

           🔗 *Все команды:* нажми на кнопку "Помощь"
           """;

    public static string NotFoundGoals 
        => """
           У вас отсутствуют цели!
           
           Вы можете их добавить с помощью команды: 
           ```text
           копилка [название] цель [сумма]
           ```
           Как пример:
           ```text
           копилка айфон цель 120000
           ```
           """;

    public static string CreateGoal(Goal goal)
        => $"""
            🎯 *Цель добавлена*
            ├─ 📅 {goal.CreatedAt.ToString("dd.MM.yyyy")}
            ├─ {(emojiMap.TryGetValue(goal.Name, out var emojiExpense) ? emojiExpense : Emojies.Random())}{goal.Name}
            └─ 🤑 *{goal.TargetAmount}*
            """;

    public static string ListCategories(List<Category> categories)
    {
        string message = "🎯 *ВСЕ ТВОИ КАТЕГОРИИ* \n\n 💸 *ДОХОДЫ* \n";

        var incomes = categories.FirstOrDefault(c => c.Name == "доходы")?.Childrens.ToArray();
        if (incomes is not null)
        {
            for (int i = 0; i < incomes.Length; i++)
            {
                message += $"`{(i != 0 ? incomes[i].Name : incomes[i].Name[0].ToString().ToUpper() + incomes[i].Name[1..])}`";
                if (i < incomes.Length - 1)
                    message += ", ";
            }
            categories.Remove(categories.FirstOrDefault(c => c.Name == "доходы")!);
        }

        message += "\n\n🧾 *РАСХОДЫ* \n";
        
        foreach (var category in categories)
        {
            string categoryEmoji = emojiMap.TryGetValue(category.Name, out var emoji) ? emoji : Emojies.Random();
            
            message += categoryEmoji + " `" + (category.Name[0].ToString().ToUpper() + category.Name[1..]) + "`: ";
            var childrens = category.Childrens.ToArray();
            message += string.Join(", ", childrens.Select(c => $"`{c.Name.ToLower()}`"));
            message += "\n";
        }

        return message;
    }

    public static string NotFoundCategory(string categoryName, TransactionType? type = null)
        => $"У вас отсутствует категория {(type is null ? "" : (type == TransactionType.Income ? "доходов" : "расходов"))} `{categoryName}`!";

    public static string CreateOperation(Operation operation, string name)
        => operation.Type == TransactionType.Expense 
            ?$"""
                🎯 *Расход добавлен*
                ├─ 📅 {operation.Date.ToString("dd.MM.yyyy")}
                ├─ {(emojiMap.TryGetValue(name, out var emojiExpense) ? emojiExpense : Emojies.Random())} {name}  
                └─ 💰 *{operation.Amount}*
                """
            : $"""
               💚 *Доход добавлен*
               ├─ 📅 {operation.Date.ToString("dd.MM.yyyy")}
               ├─ {(emojiMap.TryGetValue(name, out var emojiIncome) ? emojiIncome : Emojies.Random())} {name}
               └─ 💰 *{operation.Amount}*
               """;

    public static string ListCategoriesForEnter(List<Category> searchCategoriesValue)
    {
        string mes = "Нашлось несколько подходящих категорий, выберите одну из них:\n";
        for (int i = 0; i < searchCategoriesValue.Count; i++)
            mes += $"{i + 1}. {searchCategoriesValue[i].Name}\n";
        mes += "\nВведите номер выбранной категории";
        return mes;
    }

    public static string CategoryExists(string nameNew, bool? income = false)
        => $"""
            Категория: `{nameNew}`
            Уже есть {(income is null ? "среди ваших категорий!" : ("в " + ((bool)income ? "доходах 💚" : "расходах 💰")))}
            """;

    public static string Oops
        => "Упс... 👀\nВозникла ошибка при выполнении команды\n\nПроверьте команду и попробуйте снова!";

    public static string CategoryCreate(string nameNew, string nameParent = null)
        => $"""
            Категория : `{nameNew}`
            Успешно добавлена в {
                (nameParent is null 
                    ? "расходы 💰" 
                    : (nameParent == "доходы" 
                        ? "доходы 💚" 
                        : $"категорию {nameParent[0].ToString().ToUpper() + nameParent[1..]} " 
                          + (emojiMap.TryGetValue(nameParent, out var emoji) ? emoji : Emojies.Random())))
            }
            """;

    public static string DeleteCategory(string nameCategory)
        => $"""
            Категория: `{nameCategory}`
            Успешно удалена!
            """;

    public static string ExistsManyParentCategories(string parentCategoryName, List<Category> searchCategories)
    {
        var mes = "Найдено несколько категорий подходящих по названию основной категории:\n";

        for (int i = 0; i < searchCategories.Count; i++)
            mes += $"{i + 1}. {searchCategories[i].Name}\n";
        
        mes += "\nПопробуйте ещё раз, введя более конкретно основную категорию";
        
        return mes;
    }

    public static string CategoryRenamed(string oldCategoryName, string newCategoryName)
        => $"""
            Категория: `{oldCategoryName}`
            ├─ 🔃 успешно переименована
            └─ 🔜 теперь -> `{newCategoryName}`
            """;

    public static string ListGoals(List<Goal>? goals)
    {
        string mes = "*ЦЕЛИ:*\n--- \n";

        foreach (var goal in goals)
        {
            string remains = "много";
            if (goal.CurrentAmount != 0)
            {
                int days = DateOnly.FromDateTime(DateTime.UtcNow).DayNumber -
                           DateOnly.FromDateTime(goal.CreatedAt).DayNumber;
                remains = Math.Round((goal.TargetAmount - goal.CurrentAmount) / (goal.CurrentAmount / days)).ToString();
            }

            mes += "*" + (emojiMap.TryGetValue(goal.Name.ToLower(), out var emoji) ? emoji : "") + goal.Name[0].ToString().ToUpper() + goal.Name[1..] + "*\n";
            mes += $"├─ ";
            mes += ProgressBar(goal.CurrentAmount / goal.TargetAmount);

            mes += $"\n├─ 💰 {goal.CurrentAmount} / {goal.TargetAmount}";
            mes += $"\n└─ 📆 ещё *~{remains} дней*\n\n";
        }
        
        return mes;
    }

    private static string ProgressBar(decimal rate, int steps = 10)
    {
        if(rate < 0)
            throw new ArgumentOutOfRangeException();
        if (rate >= 1)
            return $"{new string('■', steps)} {(int)(rate * 100)}%";
        
        decimal step = 100m / steps;
        int countSteps = (int)(rate / step);

        if (rate % step > 0.85m * step)
            countSteps++;
        
        return $"{new string('■', countSteps)}{new string('▢', steps - countSteps)} *{(int)(rate * 100)}%*";
    }

    public static string CancelGoal(string name)
        => $"""
            Цель:  `{name}`
            └─ ❎ закрыта
            """;

    public static string GoalRename(string name, string newName)
        => $"""
            Цель: `{name}`
            ├─ 🔃 успешно переименована
            └─ 🔜 теперь -> `{newName}`
            """;
}

public static class Emojies
{
    private static string[] emojies = ["⭐", "🔸", "➡️", "▪️", "▶️", "🔹", "🆔", "⏩", "❕"];    
    public static string Random()
    {
        Random random = new Random();
        return emojies[random.Next(0, emojies.Length-1)];
    }
}