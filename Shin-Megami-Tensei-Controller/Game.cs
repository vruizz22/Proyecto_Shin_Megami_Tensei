using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.GameLogic;
using Shin_Megami_Tensei.Presentation;

namespace Shin_Megami_Tensei;

public class Game
{ 
    private View _view;
    private string _teamsFolder;
    
    public Game(View view, string teamsFolder)
    {
        _view = view;
        _teamsFolder = teamsFolder;
    }
    
    public void Play()
    {
        var presenter = new ConsoleBattlePresenter(_view);
        var gameManager = new GameManager(presenter);
        gameManager.StartGame(_teamsFolder);
    }
}
