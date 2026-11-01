using PAP.Initial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PAP.Bowl
{
    public class Mixer
    {

        // private camelCasing for private variables

        // public PascalCasing for public properties and methods

        // kebab casing names in json "my_value": "value"
        // kebab casing names in xml <my-value>value</my-value>
        // kebab casing names in yaml my-value: value
        // snake casing names in constants MY_CONSTANT = "value"

        public Fruit MyFruit { get; set; }

        // Constructor
        public Mixer(Fruit fruit)
        {
            MyFruit = fruit;
        }

        public Mixer()
        {
                
        }

        // Variables / Members
        private string _mixerName;
        private string _mixer;
        private string _mixerNameOne;
        private string _mixerNameTwo;
        private string mixerName;

        private byte _myBytes;
        private bool _isMixed;
        private int _mixingSpeed;

        private static string Carlos = "CARLOS";
        private char myChar = Carlos[2];
        private char myCharC = 'c';

        
        private short _myShort = 5553;
        private Int16 _myInt16 = 5553;

        private int _myInt = 555353544;
        private int lowestInt = int.MinValue;
        private int highestInt = int.MaxValue;
        private int parsedInt = int.Parse("555353544");
        private int convertedInt = Convert.ToInt32("555353544");
        private Int32 _myInt36 = 555353544;

        private long _myLong = 5553535448343658885;
        private Int64 _myInt64 = 5553535448343658885;

        private decimal _myDecimal = 555353544834354555.5548M;

        private float _myFloat = 5553535448343545555.5545f;

        private double _myDouble = 5553535448343545555.55480;

        private DateTime _myDateTime = DateTime.Now;


        // Properties
        public string MixerName
        {
            get { return _mixerName; }
            set { _mixerName = value; }
        }

        public string Name
        {
            get ;set;
        }

        public int ConvertedInt
        {
             get { return convertedInt; }
            set { convertedInt = value; }
        }

        public int HighestInt = int.MaxValue;

        // Methods
        public void Num1()
        {
            _isMixed = true;
        }
        public int Num1ReturnsInt()
        {
            _isMixed = true;
            return Convert.ToInt32(_isMixed);
        }

        protected void Num2()
        {
            _isMixed = false;
        }

        private void Num3(int speed)
        {
            _mixingSpeed = speed;
        }

        public Mixer CreateMixer(int speed)
        {
            Num3(speed);
            return new Mixer { _mixingSpeed = speed };
        }

        public DateTime CreateTime()
        {
            return DateTime.UtcNow;
        }

        public string DateTimeInString()
        {
            return DateTime.UtcNow.ToLongTimeString();
        } 

        // 

    }
}
