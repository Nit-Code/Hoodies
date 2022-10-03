using UnityEngine;
using SharedScripts.DataId;
using System;

public class Options
{
    public class BooleanOption
    {
        public BooleanOption(BooleanOptionData aData, Settings aSettingsReference)
        {
            myData = aData;
            mySettingsReference = aSettingsReference;
        }
        protected bool myValue;
        protected BooleanOptionData myData;
        protected Settings mySettingsReference;

        // These should be fine as is
        public bool GetIsCurrentValueDefault() { return myData.myDefaultValue == myValue; }
        public BooleanOptionId GetId() { return myData.myId; }
        public override bool Equals(object obj)
        {
            if (obj is BooleanOption option)
            {
                return option.GetId() == myData.myId;
            }

            Shared.LogError("[HOOD][CLIENT][OPTIONS] - BooleanOption, incompatible types!");
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(myData);
        }

        // Override these, if you wish
        public virtual void SetValue(bool aNewValue) 
        {
            myValue = aNewValue; 
            Effect(); 
        }
        public virtual bool GetValue() { return myValue; }
        public virtual void ResetValue() { SetValue(myData.myDefaultValue); Effect(); }

        // Override these, or else...
        protected virtual void Effect() { }
    }

    public class FloatRangeOption
    {
        public FloatRangeOption(FloatRangeOptionData aData, Settings aSettingsReference)
        {
            myData = aData;
            mySettingsReference = aSettingsReference;
        }

        protected const float MY_FLOAT_RANGE_MIN = 0.0f;
        protected const float MY_FLOAT_RANGE_MAX = 1.0f;

        protected float myPercentualValue; // Used for sliders display and setting
        protected float myContextualValue; // Used for effecting stuff in the game
        protected FloatRangeOptionData myData;
        protected Settings mySettingsReference;

        // These should be fine as is
        public bool GetIsCurrentValueDefault() { return myData.myDefaultValue == myContextualValue; }
        public FloatRangeOptionId GetId() { return myData.myId; }
        public override bool Equals(object obj)
        {
            if (obj is FloatRangeOption option)
            {
                return option.GetId() == myData.myId;
            }

            Shared.LogError("[HOOD][CLIENT][OPTIONS] - FloatRangeOption, incompatible types!");
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(myData);
        }

        // Override these, if you wish
        public virtual float GetContextualValue() { return myContextualValue; }
        public virtual float GetPercentualValue() { return myPercentualValue; }

        public virtual void SetValue(float aNewPercentualValue) 
        {
            if (aNewPercentualValue == myPercentualValue) 
            {
                return;
            }
            myPercentualValue = aNewPercentualValue;
            Vector2 percentualRange = new Vector2(MY_FLOAT_RANGE_MIN, MY_FLOAT_RANGE_MAX);
            Vector2 contextualRange = new Vector2(myData.myMinValue, myData.myMaxValue);
            myContextualValue = RemapClamped(myPercentualValue, percentualRange, contextualRange);
            Effect();
        }

        public virtual void ResetValue() 
        {
            myContextualValue = myData.myDefaultValue;
            Vector2 percentualRange = new Vector2(MY_FLOAT_RANGE_MIN, MY_FLOAT_RANGE_MAX);
            Vector2 contextualRange = new Vector2(myData.myMinValue, myData.myMaxValue);
            myPercentualValue = RemapClamped(myContextualValue, contextualRange, percentualRange);
            Effect();
        }

        // Override these, or else...
        protected virtual void Effect() { }
    }

    private static float RemapClamped(float aValue, Vector2 aRangeIn, Vector2 aRangeOut)
    {
        float t = (aValue - aRangeIn.x) / (aRangeIn.y - aRangeIn.x);
        if (t > 1f)
            return aRangeOut.x;
        if (t < 0f)
            return aRangeOut.y;
        return aRangeOut.x + (aRangeOut.y - aRangeOut.x) * t;
    }

    public class VolumeMaster : FloatRangeOption
    {
        public VolumeMaster(FloatRangeOptionData aData, Settings aSettingsReference) : base(aData, aSettingsReference)
        {
        }

        protected override void Effect()
        {
            if (mySettingsReference == null) 
            {
                Shared.LogError("[HOOD][CLIENT][OPTIONS] - missing mySettingsReference, Effect aborted.");
                return;
            }

            mySettingsReference.GetAudioManagerReference().SetMasterVolume(myPercentualValue);
        }
    }

    public class VolumeMusic : FloatRangeOption
    {
        public VolumeMusic(FloatRangeOptionData aData, Settings aSettingsReference) : base(aData, aSettingsReference)
        {
        }

        protected override void Effect()
        {
            if (mySettingsReference == null)
            {
                Shared.LogError("[HOOD][CLIENT][OPTIONS] - missing mySettingsReference, Effect aborted.");
                return;
            }

            mySettingsReference.GetAudioManagerReference().SetMusicMasterVolume(myPercentualValue);
        }
    }

    public class VolumeSound : FloatRangeOption
    {
        public VolumeSound(FloatRangeOptionData aData, Settings aSettingsReference) : base(aData, aSettingsReference)
        {
        }

        protected override void Effect()
        {
            if (mySettingsReference == null)
            {
                Shared.LogError("[HOOD][CLIENT][OPTIONS] - missing mySettingsReference, Effect aborted.");
                return;
            }

            mySettingsReference.GetAudioManagerReference().SetSoundMasterVolume(myPercentualValue);
        }
    }

    public class VolumeAmbient : FloatRangeOption
    {
        public VolumeAmbient(FloatRangeOptionData aData, Settings aSettingsReference) : base(aData, aSettingsReference)
        {
        }

        protected override void Effect()
        {
            if (mySettingsReference == null)
            {
                Shared.LogError("[HOOD][CLIENT][OPTIONS] - missing mySettingsReference, Effect aborted.");
                return;
            }

            mySettingsReference.GetAudioManagerReference().SetAmbientMasterVolume(myPercentualValue);
        }
    }

}
