using OpenTK.Input;
using Ryujinx.Common.Configuration.Hid;
using System.Collections.Generic;
using System;
using System.IO;

namespace Ryujinx.Ui.Input
{
    class JoystickButtonAssigner : ButtonAssigner
    {
        private int _index;

        private double _triggerThreshold;

        private JoystickState _currState;

        private JoystickState _prevState;

        private JoystickButtonDetector _detector;

        public JoystickButtonAssigner(int index, double triggerThreshold)
        {
            _index = index;
            _triggerThreshold = triggerThreshold;
            _detector = new JoystickButtonDetector();
        }

        public void Init()
        {
            _currState = Joystick.GetState(_index);
            _prevState = _currState;
        }

        public void ReadInput()
        {
            _prevState = _currState;
            _currState = Joystick.GetState(_index);

            CollectButtonStats();
        }

        public bool HasAnyButtonPressed()
        {
            return _detector.HasAnyButtonPressed();
        }

        public bool ShouldCancel()
        {
            return Mouse.GetState().IsAnyButtonDown || Keyboard.GetState().IsAnyKeyDown;
        }

        public string GetPressedButton()
        {
            List<ControllerInputId> pressedButtons = _detector.GetPressedButtons();

            // Reverse list so axis button take precedence when more than one button is recognized.
            pressedButtons.Reverse();

            return pressedButtons.Count > 0 ? pressedButtons[0].ToString() : "";
        }

        private void CollectButtonStats()
        {
            JoystickCapabilities capabilities = Joystick.GetCapabilities(_index);

            ControllerInputId pressedButton;

            // Buttons
            for (int i = 0; i != capabilities.ButtonCount; i++)
            {
                if (_currState.IsButtonDown(i) && _prevState.IsButtonUp(i))
                {
                    Enum.TryParse($"Button{i}", out pressedButton);
                    _detector.AddInput(pressedButton, 1);
                }

                if (_currState.IsButtonUp(i) && _prevState.IsButtonDown(i))
                {
                    Enum.TryParse($"Button{i}", out pressedButton);
                    _detector.AddInput(pressedButton, -1);
                }
            }

            // Axis
            for (int i = 0; i != capabilities.AxisCount; i++)
            {
                float axisValue = _currState.GetAxis(i);

                Enum.TryParse($"Axis{i}", out pressedButton);
                _detector.AddInput(pressedButton, axisValue);
            }

            // Hats
            for (int i = 0; i != capabilities.HatCount; i++)
            {
                string currPos = GetHatPosition(_currState.GetHat((JoystickHat)i));
                string prevPos = GetHatPosition(_prevState.GetHat((JoystickHat)i));

                if (currPos == prevPos)
                {
                    continue;
                }

                if (currPos != "")
                {
                    Enum.TryParse($"Hat{i}{currPos}", out pressedButton);
                    _detector.AddInput(pressedButton, 1);
                }

                if (prevPos != "")
                {
                    Enum.TryParse($"Hat{i}{prevPos}", out pressedButton);
                    _detector.AddInput(pressedButton, -1);
                }
            }
        }

        private string GetHatPosition(JoystickHatState hatState)
        {
            if (hatState.IsUp) return "Up";
            if (hatState.IsDown) return "Down";
            if (hatState.IsLeft) return "Left";
            if (hatState.IsRight) return "Right";
            return "";
        }

        private class JoystickButtonDetector
        {
            private Dictionary<ControllerInputId, InputSummary> _stats;

            public JoystickButtonDetector()
            {
                _stats = new Dictionary<ControllerInputId, InputSummary>();
            }

            public bool HasAnyButtonPressed()
            {
                foreach (var inputSummary in _stats.Values)
                {
                    if (checkButtonPressed(inputSummary))
                    {
                        return true;
                    }
                }
                
                return false;
            }

            public List<ControllerInputId> GetPressedButtons()
            {
                List<ControllerInputId> pressedButtons = new List<ControllerInputId>();

                foreach (var kvp in _stats)
                {
                    if (!checkButtonPressed(kvp.Value))
                    {
                        continue;
                    }
                    pressedButtons.Add(kvp.Key);
                }

                return pressedButtons;
            }

            public void AddInput(ControllerInputId button, float value)
            {
                InputSummary inputSummary;

                if (!_stats.TryGetValue(button, out inputSummary))
                {
                    inputSummary = new InputSummary();
                    _stats.Add(button, inputSummary);
                }

                inputSummary.AddInput(value);
            }

            public override string ToString()
            {
                TextWriter writer = new StringWriter();

                foreach (var kvp in _stats)
                {
                    writer.WriteLine($"Button {kvp.Key} -> {kvp.Value}");
                }

                return writer.ToString();
            }

            private bool checkButtonPressed(InputSummary sequence)
            {
                float distance = Math.Abs(sequence.Min - sequence.Avg) + Math.Abs(sequence.Max - sequence.Avg);
                return distance > 1.5; // distance range [0, 2]
            }
        }

        private class InputSummary
        {
            public float Min, Max, Sum, Avg;

            public int NumSamples;

            public InputSummary()
            {
                Min = float.MaxValue;
                Max = float.MinValue;
                Sum = 0;
                NumSamples = 0;
                Avg = 0;
            }

            public void AddInput(float value)
            {
                Min = Math.Min(Min, value);
                Max = Math.Max(Max, value);
                Sum += value;
                NumSamples += 1;
                Avg = Sum / NumSamples;
            }

            public override string ToString()
            {
                return $"Avg: {Avg} Min: {Min} Max: {Max} Sum: {Sum} NumSamples: {NumSamples}";
            }
        }
    }
}
