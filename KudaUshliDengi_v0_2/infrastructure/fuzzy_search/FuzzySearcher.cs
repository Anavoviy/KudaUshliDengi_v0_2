namespace KudaUshliDengi_v0_2.infrastructure.fuzzy_search;

public class FuzzySearcher
{
    public float LevelOfErrorRate { get; }
    public LevenshteinDistanceParams LDParams { get; }
    public FuzzySearcher(float levelOfErrorRate)
    {
        LevelOfErrorRate = levelOfErrorRate;
    }
    
    /// <summary>
    /// Поиск некоторого количества схожих строк, отсортированных в порядке возрастания отличий
    /// </summary>
    /// <param name="target">Строка, с которой сравниваем</param>
    /// <param name="sources">Строки, которые сравниваем</param>
    /// <param name="countResults">Максимальное кол-во результатов</param>
    /// <returns>Список схожих строк</returns>
    public IEnumerable<string> SearchMany(string target, IEnumerable<string> sources, int countResults = 10)
    {
        target = target.Trim().ToLower();
        if(string.IsNullOrEmpty(target))
            return new List<string>();
        
        float length = target.Length;

        sources = sources.Where(s => !string.IsNullOrEmpty(s.Trim())).Select(s => s.ToLower()).ToList();

        SortedDictionary<float, List<string>> sortedErrLvls = new();

        foreach (var source in sources)
        {
            int d = LevenshteinDistance(source, target); 
            float errLvl = d == 0f ? 0f : ((int)((d / length) * 100)) / 100f;
            if (errLvl <= LevelOfErrorRate)
            {
                if (!sortedErrLvls.ContainsKey(errLvl))
                    sortedErrLvls[errLvl] = new List<string>();
                sortedErrLvls[errLvl].Add(source);
            }
        }
    
        if(sortedErrLvls.ContainsKey(0f))
            return new List<string>(sortedErrLvls[0f]);
        if(sortedErrLvls.Count == 0)
            return new List<string>();
    
        List<string> results = new();
        foreach (var (key, errs) in sortedErrLvls)
        {
            if(errs.Count < countResults - results.Count)
                results.AddRange(errs);
            else
                results.AddRange(results.Take(countResults - results.Count));

            if (results.Count == countResults)
                break;
        }
        return results;
    }
    
    /// <summary>
    /// Поиск максимально схожей строки
    /// </summary>
    /// <param name="target">Строку, с которой сравниваем</param>
    /// <param name="sources">Набор строк, которые сравниваем</param>
    /// <returns>Максимально схожая строка или Null (если таковых не найдено)</returns>
    public string? SearchOne(string target, IEnumerable<string> sources)
    {
        target = target.Trim().ToLower();
        if (string.IsNullOrEmpty(target))
            return null;
        
        float length = target.Length;
        
        sources = sources.Where(s => !string.IsNullOrEmpty(s.Trim())).Select(s => s.ToLower()).ToList();

        SortedDictionary<float, List<string>> sortedErrLvls = new();

        foreach (var source in sources)
        {
            int d = LevenshteinDistance(source, target);
            if (d == 0)
                return source;
            float errLvl = ((int)((d / length) * 100)) / 100f;
            if (errLvl <= LevelOfErrorRate)
            {
                if (!sortedErrLvls.ContainsKey(errLvl))
                    sortedErrLvls[errLvl] = new List<string>();
                sortedErrLvls[errLvl].Add(source);
            }
        }
    
        if (sortedErrLvls.Count == 0)
            return null;
            
        return sortedErrLvls.First().Value.First();
    }

    /// <summary>
    /// Рассчёт расстояния Левенштейна для двух слов. Сравнение регистрозависимое.
    /// </summary>
    /// <param name="source">Строка, с которой сравниваем</param>
    /// <param name="target">Строка, которую сравниваем</param>
    /// <returns>Целое число расстояния Левенштейна</returns>
    private int LevenshteinDistance(string source, string target)
    {
        if(source == target)
            return 0;
        
        // Вариант, когда source = "", а target = "привет" (как пример) вернёт target.Length (равное 6)
        if (string.IsNullOrEmpty(source))
        {
            if (!string.IsNullOrEmpty(target))
                return target.Length;
            return 0; //Если target и source пустые - тогда расстояние Левенштейна = 0
        } 
        if (string.IsNullOrEmpty(target))
            return source.Length; // Вариант, когда source != "", а target == "", вернёт source.Length

        if (target.Length < source.Length)
            (target, source) = (source, target);
        
    
        int [,] d = new int[3, target.Length + 1];
        for (int i = 0; i <= target.Length; i++)
            d[0, i] = i;
        int curIdCol, prevIdCol, cost;

        for (int col = 1; col <= source.Length; col++)
        {
            curIdCol = col % 3;
            prevIdCol = (col - 1) % 3;
            d[curIdCol, 0] = col;
            for (int row = 1; row <= target.Length; row++)
            {
                cost = source[col - 1] == target[row - 1] ? 0 : 1;
                d[curIdCol, row] = int.Min(d[curIdCol, row - 1] + 1, int.Min(d[prevIdCol, row] + 1, d[prevIdCol, row - 1] + cost));
            
                if(col > 1 && row > 1)
                    if(target[row - 1] == source[col - 2] && 
                       target[row - 2] == source[col - 1] && 
                       source[col - 2] != source[col - 1])
                        d[curIdCol, row] = int.Min(d[(col-2) % 3, row - 2], d[curIdCol, row]);
            }
        }
    
        return d[source.Length % 3, d.GetUpperBound(1)];
    }
}

public struct LevenshteinDistanceParams
{
    public int CostIn { get; }
    public int CostRm { get; }
    public int CostPr { get; }

    public LevenshteinDistanceParams(int costIn, int costRm, int costPr)
    {
        CostIn = costIn;
        CostRm = costRm;
        CostPr = costPr;
    }
}