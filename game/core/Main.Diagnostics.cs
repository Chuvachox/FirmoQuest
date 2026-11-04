using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BusinessSim;

// Parcurge ecranele pentru capturi si mentine comenzile de diagnostic separate de navigare.
public partial class Main
{
    private void StartGallery()
    {
        GD.Print("Mod galerie pornit - capturi în res://shots/");
        GD.Print($"  fereastra      = {GetWindow().Size}");
        GD.Print($"  viewport vizibil = {GetViewport().GetVisibleRect().Size}");
        GD.Print($"  project settings = {ProjectSettings.GetSetting("display/window/size/viewport_width")}x" +
                 $"{ProjectSettings.GetSetting("display/window/size/viewport_height")} / " +
                 $"override {ProjectSettings.GetSetting("display/window/size/window_width_override")}x" +
                 $"{ProjectSettings.GetSetting("display/window/size/window_height_override")}");
        GD.Print($"  root (Main)    = {Size} anchors={AnchorLeft},{AnchorRight},{AnchorTop},{AnchorBottom}");
        _galleryStep = 1;
        _galleryFrame = 0;
        ShowMenu();
    }

    private void Shot(string name)
    {
        if (DisplayServer.GetName() == "headless")
            return;
        try
        {
            var dir = ProjectSettings.GlobalizePath($"res://shots/{_galleryScenario}");
            DirAccess.MakeDirRecursiveAbsolute(dir);
            var img = GetViewport().GetTexture().GetImage();
            img.SavePng($"{dir}/{name}.png");
            GD.Print($"  shot -> shots/{name}.png");
        }
        catch (System.Exception e)
        {
            GD.PrintErr($"Captură eșuată ({name}): {e.Message}");
        }
    }

    private bool GameAlive => _gameScreen != null && GodotObject.IsInstanceValid(_gameScreen);

    public override void _Process(double delta)
    {
        RefreshSafeArea(delta);
        if (_galleryStep <= 0)
            return;

        _galleryFrame++;
        if (_galleryFrame % 30 != 0)
            return;

        switch (_galleryStep)
        {
            case 1:
                Shot("1-meniu");
                ShowHelp();
                _galleryStep = 2;
                break;

            case 2:
                Shot("1b-ajutor");
                ShowDifficulty();
                _galleryStep = 3;
                break;

            case 3:
                Shot("2-dificultate");
                ShowScenario();
                _galleryStep = 4;
                break;

            case 4:
                Shot("3-scenariu");
                StartGame(_galleryScenario);
                _galleryStep = 5;
                break;

            case 5:
                Shot("4-joc-gol");
                _gameScreen?.AutoTurn();
                _gameScreen?.AutoTurn();
                _galleryTurns = 2;
                _galleryStep = 6;
                break;

            case 6:
                if (GameAlive && _gameScreen.HasEvent)
                {
                    Shot("6-eveniment");
                    _gameScreen.ResolveEvent();
                    _galleryShot = false;
                    _galleryStep = 7;
                    break;
                }
                if (GameAlive && _galleryTurns++ < 10)
                    _gameScreen.AutoTurn();
                else
                {
                    _galleryShot = false;
                    _galleryStep = 7;
                }
                break;

            case 7:
                if (!GameAlive)
                {
                    _galleryStep = 8;
                    break;
                }
                if (!_galleryShot)
                {
                    Shot("5-joc-activ");
                    _galleryShot = true;
                    break;
                }
                if (_gameScreen.HasEvent)
                    _gameScreen.ResolveEvent();
                else
                    _gameScreen.AutoTurn();
                if (++_galleryTurns > 90)
                    _galleryStep = 8;
                break;

            case 8:
                Shot("7-final");
                _galleryStep = 9;
                break;

            case 9:
                GD.Print("Galerie încheiată.");
                QuitGame();
                break;
        }
    }
}
