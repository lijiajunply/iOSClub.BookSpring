namespace BookSpring.WebApp.Models;

public static class DataStatic
{
    public static string BaseUrl
    {
        get
        {
#if DEBUG
            return "http://localhost:5259";
#else
      return "https://book.hnd1.zeabur.app";
#endif
        }
    }
}