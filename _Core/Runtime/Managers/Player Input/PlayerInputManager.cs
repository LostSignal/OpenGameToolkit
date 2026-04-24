
namespace OGT
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;

    public class PlayerInputManager : Manager
    {
        [SerializeField] private int maxPlayers = 8;

        private List<Stack<IPlayerInput>> playerInputs;

        public void PushInput<T>(int playerIndex, params IInputUpdater<T>[] inputUpdaters)
        {
            // TODO [bgish]: Verify playerIndex (>= 0 and < this.maxPlayers)
            this.playerInputs[playerIndex].Push(new PlayerInput<T>(inputUpdaters));
        }

        public void PopInput(int playerIndex)
        {
            // TODO [bgish]: Verify playerIndex (>= 0 and < this.maxPlayers)
            this.playerInputs[playerIndex].Pop();
        }

        public bool TryGetPlayerInput<T>(int playerIndex, float deltaTime, out T input)
        {
            // TODO [bgish]: Verify playerIndex (>= 0 and < this.maxPlayers)
            var stack = this.playerInputs[playerIndex];

            if (stack.Count == 0)
            {
                input = default;
                return false;
            }

            var playerInput = stack.Peek();

            // TODO [bgish]: to an IsAssignableTo so we can pass interfaces here
            if (playerInput.Input.GetType() != typeof(T))
            {
                input = default;
                return false;
            }

            playerInput.Update(deltaTime);
            input = (T)playerInput.Input;
            return true;
        }

        protected override Task InitializeManager(Bootloader bootloader)
        {
            this.playerInputs = new List<Stack<IPlayerInput>>();

            for (int i = 0; i < this.maxPlayers; i++)
            {
                this.playerInputs.Add(new Stack<IPlayerInput>());
            }

            return Task.CompletedTask;
        }

        private interface IPlayerInput
        {
            void Update(float deltaTime);

            object Input { get; }
        }

        private class PlayerInput<T> : IPlayerInput
        {
            private List<IInputUpdater<T>> inputUpdaters = new();

            public T input;

            public object Input => this.input;

            public int PlayerIndex { get; private set; }

            public PlayerInput(params IInputUpdater<T>[] inputUpdaters)
            {
                if (inputUpdaters != null)
                {
                    this.inputUpdaters.AddRange(inputUpdaters);
                }
            }

            public void Update(float deltaTime)
            {
                foreach (var inputUpdater in inputUpdaters)
                {
                    if (inputUpdater.IsActive == false)
                    {
                        continue;
                    }

                    inputUpdater.Update(ref this.input, deltaTime);
                    break;
                }
            }
        }
    }
}
