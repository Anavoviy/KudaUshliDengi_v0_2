namespace KudaUshliDengi_v0_2.services.result.errors;

public static class AllErrors
{
    // В TCathcer
    // 101 - возвращено значение null
    //
    public static UserErrors User => new UserErrors(); //Коды 300-315
    
    public static CategoryErrors Category => new CategoryErrors(); //Коды 200-215
    
    public static GoalErrors Goal => new GoalErrors(); //Коды 700-715
}