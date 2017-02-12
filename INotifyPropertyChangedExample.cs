using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CSharpSamples
{
	class Element: INotifyPropertyChanged 
	{
		string name;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name 
		{
			get 
			{
				return name;
			}
			set 
			{
				if( value != name ) 
				{
					name = value;
					NotifyPropertyChanged();
				}
			}
		}

		public Element(string name) 
		{
			this.name = name;
		}

		void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
	}

	class Observer: IDisposable
	{
		Element element;
		public Observer(Element element) 
		{
			this.element = element;
			if( this.element != null ) {
				element.PropertyChanged += OnPropertyChanged;
				Console.WriteLine("Subscribed.");
			}
		}

        public void Dispose() {
			if( this.element != null ) {
				element.PropertyChanged -= OnPropertyChanged;
				Console.WriteLine("Unsubscribed.");
			}
        }

		void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
			Console.WriteLine($"{sender}: property {e.PropertyName} is changed.");
		}
    }

    public static class INotifyPropertyChangedExample
    {
        public static void Test() {
			var element = new Element("originalName");
			using (var observer = new Observer(element) ) {
				element.Name = "newName";
			}
		}
    }
}