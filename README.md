# Janknerics

An alternative to built in generics. With a couple bonus features.

I use this to auto-generate a ViewModel from a Model.
The ViewModel can optionally have a constructor taking the Model auto-generated as well.
In this case each property or field in the ViewModel is constructed with the corresponding property or field from the Model.

## Simple Example
```c#
partial class MyViewModel;

[JanknericConstructor(typeof(MyViewModel))]
class MyModel
{
    [Jankneric(typeof(MyViewModel), NewType = typeof(double))]
    public float P1;
}
```
This will create ```MyViewModel.g.cs``` containing:
```c#
partial class MyViewModel
{
    public double P1;
    public MyViewModel(MyModel source)
    {
        P1 = (double)source.P1;
    }
}
```

Clearly this simple example would be better done with regular Generics.
My suspicion is that as the number of 'generic' parameters grows this approach will become more attractive.
But that's just a guess at this point.