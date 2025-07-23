using UnityEngine;

namespace Code.Frameworks.InteractionSystem.Context.Interfaces
{
    public interface IGameAction  
    {  
        string Name { get; }  
        string Description { get; }  
        Sprite Icon { get; }  
	  
        GameObject Target { get; set; }

        void Run();
    }  
}