using System;
using System.Collections.Generic;
using Flicker.Core;
using Mirror;
using UnityEngine;

namespace Flicker.Module
{
    public class CharacterControllerManager: Singleton<CharacterControllerManager>
    {
        private readonly List<GameCharacterController> gameCharacterControllers = new();

        private void Register(GameCharacterController gameCharacterController)
        {
            gameCharacterControllers.Add(gameCharacterController);
        }
        private void Unregister(GameCharacterController gameCharacterController)
        {
            gameCharacterControllers.Remove(gameCharacterController);
        }
        public List<GameCharacterController> GetAll()
        {
            List<GameCharacterController> results = new();
            foreach (var i in gameCharacterControllers)
            {
                results.Add(i);
            }
            return results;
        }
        public void RemoveAll()
        {
            foreach (var i in GetAll())
            {
                if (i==null) continue;
                if (i.gameObject==null) continue;
                Remove(i.gameObject);
            }
        }
        public GameObject Create(GameObject gameObject, float y=0, bool isRightWall=false){
            GameObject result = Instantiate(gameObject);
            var gcc = result.GetComponent<GameCharacterController>();
            gcc.SetPosition(y,isRightWall);
            Register(gcc);
            return result;
        }
        public void Remove(GameObject gameObject){
            var gcc = gameObject.GetComponent<GameCharacterController>();
            if (gcc==null) return;
            if (gcc.GetComponent<NetworkIdentity>() == null)
            {
                Destroy(gcc.gameObject);
            }
            else
            {
                NetworkServer.Destroy(gcc.gameObject);
            }
            Unregister(gcc);
        }
    }
}