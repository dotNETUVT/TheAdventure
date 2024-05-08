
using System;
using Silk.NET.SDL;
// using SDL2_ttf;
// using SDL2;
// using Silk.NET.SDL;
// using Silk.NET.Maths;
using TheAdventure;
using TheAdventure.Models;

namespace Questalia
{
    public unsafe class Questalia
    {

        private Sdl _sdl;
        private GameRenderer _renderer;
        private Dictionary<int, GameObject> _gameObjects;
        private int _active_quests_no = 0;
        private int _done_quests_no = 0;
        private int _inactive_quests_no = 0;
        private Quest[] _active_quests = new Quest[100];
        private Quest[] _done_quests = new Quest[100];
        private Quest[] _inactive_quests = new Quest[100];

        // public Questalia (Sdl sdl)
        // {
        //     _sdl = sdl;
        //     Init();
        // }


        /// <summary>
        /// Used to initialize the plugin. This will reset all quests to NONE.
        /// </summary>
        public void Init(Silk.NET.SDL.Sdl sdl, GameRenderer renderer, Dictionary<int, GameObject> gameObjects)
        {
            Console.WriteLine("Questalia initializing...");
            _sdl = sdl;
            _renderer = renderer;
            _gameObjects = gameObjects;
            _active_quests_no = 0;
            _done_quests_no = 0;
            _inactive_quests_no = 0;
            _active_quests = [];
            _done_quests = [];
            _inactive_quests = [];
            
            PrintLogo();
            
            Console.WriteLine("Questalia initialization done!");
        }

        /// <summary>
        /// Used to create a new quest.
        /// </summary>
        /// <param name="questName">The name of the quest</param>
        /// <param name="type">The type of quest: 0 - Active, 1 - Done, 2 - Inactive</param>
        public void AddQuest(string questName, questType type = 0, int questAggressiveness = 1)
        {
            Quest newQuest = new Quest(questName, _active_quests_no++, type);
            if (type == questType.Active)
                _active_quests[_active_quests_no] = newQuest;
            else if (type == questType.Done)
                _done_quests[_active_quests_no] = newQuest;
            else if (type == questType.Inactive)
                _inactive_quests[_active_quests_no] = newQuest;
            
            PrintQuests(questName, 50, 100, 0);
            
        }

        /// <summary>
        /// Used to complete a quest and remove it from the active / inactive list
        /// </summary>
        /// <param name="id">The id of the target quest</param>
        public void CompleteQuest(int id)
        {
            int index = 0;
            Quest targetQuest = null;
            foreach (var quest in _active_quests)
            {
                if (quest._id == id)
                {
                    targetQuest = quest;
                    _done_quests[_done_quests_no++] = targetQuest;
                    // Remove from the original list.
                    for (int i = index + 1; i < _active_quests_no; i++)
                        _active_quests[i - 1] = _active_quests[i];

                    _active_quests_no--;
                    return;
                }

                index++;
            }

            index = 0;
            foreach (var quest in _inactive_quests)
            {
                if (quest._id == id)
                {
                    targetQuest = quest;
                    _done_quests[_done_quests_no++] = targetQuest;
                    // Remove from the original list.
                    for (int i = index + 1; i < _inactive_quests_no; i++)
                        _inactive_quests[i - 1] = _inactive_quests[i];

                    _inactive_quests_no--;
                    return;
                }

                index++;
            }

            foreach (var quest in _done_quests)
                if (quest._id == id)
                {
                    Console.WriteLine("Quest already done.");
                    targetQuest = quest;
                    return;
                }

            // Not found, but do not crash, just return;
            if (targetQuest == null)
            {
                Console.WriteLine("Unable to find that quest");
                return;
            }
        }

        public void PrintLogo()
        {
            Console.WriteLine("Printing Logo!");
            var translated = _renderer.TranslateFromScreenToWorldCoordinates(50, 50);
            var spriteSheet = SpriteSheet.LoadSpriteSheet("C:/Users/Stefan/RiderProjects/TheAdventure/Questalia/Assets/QuestaliaLogo.json", "C:/Users/Stefan/RiderProjects/TheAdventure/Questalia/Assets", _renderer);
            if (spriteSheet != null)
            {
                // spriteSheet.ActivateAnimation("Explode");
                RenderableGameObject bomb = new(spriteSheet, (translated.X, translated.Y));
                _gameObjects.Add(bomb.Id, bomb);
            }

        }
        
        public void PrintQuests(string questText, int x, int y, int questAggressiveness = 1)
        {
            Console.WriteLine("Printing Quests!");
            var translated = _renderer.TranslateFromScreenToWorldCoordinates(x, y);
            string path = "";
            switch (questAggressiveness)
            {
                case 0:
                    path = "C:/Users/Stefan/RiderProjects/TheAdventure/Questalia/Assets/AgressiveQuest.json";
                    break;
                case 1:
                    path = "C:/Users/Stefan/RiderProjects/TheAdventure/Questalia/Assets/NeutralQuest.json";
                    break;
            }
            var spriteSheet = SpriteSheet.LoadSpriteSheet(path, "C:/Users/Stefan/RiderProjects/TheAdventure/Questalia/Assets", _renderer);
            if (spriteSheet != null)
            {
                // spriteSheet.ActivateAnimation("Explode");
                RenderableGameObject bomb = new(spriteSheet, (translated.X, translated.Y));
                _gameObjects.Add(bomb.Id, bomb);
            }

        }
    }
}