namespace ArtEFundAPIServices;

public class CreateMapperScript
{
    public static void Main(string args)
    {
        
        string currentPath = Directory.GetCurrentDirectory();


        String.Concat(currentPath, $"/Mapper/{args[1]}.cs");
        
        
    }
}