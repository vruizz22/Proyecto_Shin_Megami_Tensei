namespace Shin_Megami_Tensei.Models;

public class Stats
{
    public int HP { get; set; }
    public int MP { get; set; }
    public int Str { get; set; }
    public int Skl { get; set; }
    public int Mag { get; set; }
    public int Spd { get; set; }
    public int Lck { get; set; }

    public Stats(int hp, int mp, int str, int skl, int mag, int spd, int lck)
    {
        HP = hp;
        MP = mp;
        Str = str;
        Skl = skl;
        Mag = mag;
        Spd = spd;
        Lck = lck;
    }
}
