# Verifiers library

Sometimes in automation tests we need to validate work of methods, returning very complex objects as a result. We want to be sure, that these objects have correct structure. Common assertion libraries allow us to check our expectation about simple objects (strings, numbers), and collections of objects of primitive types. But it can happen, that we need more.

Verifiers library is created to support such cases. It allows you to combine many expectations in one package, execute them all, and return all results in one place.

Let's consider an example. You want to test method, returning string of very special structure. This string should comply with the following expectations:
* it should not be shorter than 5 symbols
* it should start with 'A'.
* it should end with 'C'.
* it should contain even number of 'T'.

You can write down these expectaions this way:
```csharp
Assert.True(str.Length >= 5);
Assert.StartsWith("A", str);
Assert.EndsWith("C", str);
Assert.True(str.Count(c => c == 'T') % 2 == 0);
```
But what, if your test fails on the first assertion? It will give you information, that length of your string is incorrect. But you will know nothing about it's starting and ending symbols, and about number of 'T' letters inside.

With Verifier library you can do the same thing with only slightly more lines of code:

```csharp
public class ComplexStringVerifier : Verifier<ComplexStringVerifier, string> {}

...

new ComplexStringVerifier()
    .AddNormalVerifiers(
        str => Assert.True(str.Length >= 5),
        str => Assert.StartsWith("A", str),
        str => Assert.EndsWith("C", str),
        str => Assert.True(str.Count(c => c == 'T') % 2 == 0)
    )
    .Check(complexString);
```

For slightly longer code, you get information about all problems with your string in one place.

Why do we need to know about all problems with our complex object? You may say, that even one problem is the problem you need to fix. Why do you need to know about all of them? Let me ask you one thing. Do you prefer to have a compiler, which reports you only about the first error in a code it has met, or you prefer to have a compiler, which reports you about all errors in the code? This is the same situation here. It is up to you, to decide what you want to have. And if you want to have all information at once, this library is at your service.

## Verification result

In general, this library works with functions taking object, you want to check, and returning an instance of *VerificationResult* structure. This instance contains collection of error messages. These error messages can be passed to the constructor of the structure. Be aware, that all null, empty or whitespace messages will be ignored.

There are two types of *VerificationResult*: normal and critical. Let's consider differences between them. Let's imagine, that in our example with complex string we obtained null string from our system under test:

```csharp
public class ComplexStringVerifier : Verifier<ComplexStringVerifier, string> {}

...

new ComplexStringVerifier()
    .AddNormalVerifiers(
        str => Assert.NotNull(str),
        str => Assert.True(str.Length >= 5),
        str => Assert.StartsWith("A", str),
        str => Assert.EndsWith("C", str),
        str => Assert.True(str.Count(c => c == 'T') % 2 == 0)
    )
    .Check(null);
```
It is obvious, that all checks after the first one (check for null) do not make sense. This is exaclty what critical verification results for. If some verification function returns critical verification result with non-empty list of error messages, then other verification functions will not be executed. Here is how we can make it:
```csharp
new ComplexStringVerifier()
    .AddCriticalVerifiers(
        str => Assert.NotNull(str)
    )
    .AddNormalVerifiers(
        str => Assert.True(str.Length >= 5),
        str => Assert.StartsWith("A", str),
        str => Assert.EndsWith("C", str),
        str => Assert.True(str.Count(c => c == 'T') % 2 == 0)
    )
    .Check(null);
```

To construct verification result one can use constructor, or static functions *Normal* and *Critical*. You also can implicitely convert string into verification result:
```csharp
VerificationResult result = "error";
```
 In this case verification result always will be normal.
 
 Additionally verification result provides overload of '+' operator:
```csharp
VerificationResult v1 = ...;
VerificationResult v2 = ...;
VerificationResult v = v1 + v2;
```
Be aware, that if either 'v1' or 'v2' are critical and have error messages, then 'v' will be critical too. Otherwise, 'v' will be normal verification result.

## Class Verifier

Verifier represents base class for creation of verifiers of your complex objects. It implements simple interface *IVerifier*:
```csharp
public interface IVerifier<in TUnderTest>
{
    VerificationResult Verify(TUnderTest instanceUnderTest);
}
```
You can create your own verifier for *YourComplexClass* the following way:
```csharp
public class YourVerifier : Verifier<YourVerifier, YourComplexClass> 
{
    ...
}
```
This class provides you with methods *AddVerifiers*, which can accept arrays of:
* Func<TUnderTest, VerificationResult>
* IVerifier\<TUnderTest\>

where *TUnderTest* is the class of objects, you are testing. Be aware, that if any of these verification functions throws an exception, critical verification result will be generated for it. Error messages of this verification result will contain message of the exception.

There are many assertion libraries on the market. But they are designed to throw exceptions, if verification fails. Verifier supports these libraries, using *AddNormalVerifiers* and *AddCriticalVerifiers* methods. They accept *Action\<TUnderTest\>* as a parameter. They produce normal and critical verification results correspondingly. If action throws an exception, then its message will be in the list of error messages of the verification result:
```csharp
verifier.AddNormalVerifiers(
        str => Assert.StartsWith("A", str),
        str => Assert.EndsWith("C", str)
    );
```

### Static and dynamic verification functions

When you add verification functions using *AddVerifiers* and *AddNNNVerifiers* methods, you create static verifications. It means, that you can reuse added verification functions several times:
```csharp
var verifier = new ComplexStringVerifier()
    .AddCriticalVerifiers(
        str => Assert.NotNull(str)
    )
    .AddNormalVerifiers(
        str => Assert.True(str.Length >= 5),
        str => Assert.StartsWith("A", str),
        str => Assert.EndsWith("C", str),
        str => Assert.True(str.Count(c => c == 'T') % 2 == 0)
    );
...
verifier.Check(string1);
...
verifier.Check(string2);
...
```
It allows you to create verifier only once and then reuse it in different places.

But sometimes you need to know instance of your complex object to decide, which verification functions to use. In this case you should override *AddDynamicVerifiers* methods. Inside this method, you have access to the instance of object under test, and you can use the same *AddVerifiers* and *AddNNNVerifiers* methods to add verification functions, based on the knowledge of instance of this object. These verification functions are added only for one verification. They are not stored inside verifier after verification is finished. This is why they are called dynamic. 

For example, your method should generate string, which contains 3 letters 'b' if it starts from 'b', and only 2 letters 'b' otherwise. Here is how you can express these expectations.
```csharp
private class ComplexStringVerifier : Verifier<ComplexStringVerifier, string>
{
    public ComplexStringVerifier()
    {
        AddCriticalVerifiers(Assert.NotNull);
    }

    protected override void AddDynamicVerifiers(string instanceUnderTest)
    {
        if (instanceUnderTest.StartsWith("b"))
        {
            AddNormalVerifiers(sut => Assert.Equal(3, sut.Count(c => c == 'b')));
        }
        else
        {
            AddNormalVerifiers(sut => Assert.Equal(2, sut.Count(c => c == 'b')));
        }
    }
}
```
And now you can reuse this verifier on different strings:
```csharp
var verifier = new ComplexStringVerifier();
...
verifier.Check(str1);
...
verifier.Check(str2);
...
verifier.Check(str3);
```
All verification function, added inside *AddDynamicVerifiers* method, will be used only for single call of *Check* (or *Verify*).

## Verification of lists

Sometimes we should create complex lists. For example, our method can return list, containing objects of different types, having common ancestor or interface. To check such lists you can use *CollectionVerifier* class. Lets say, we have the following hierarchy of classes:
```csharp
public abstract class Base {}

public class IntClass : Base
{
    public int Value;
}

public class StringClass : Base
{
    public string Value;
}
```
We want to check *IEnumerable\<Base\>*. Here is how we can construct verifier for it:
```csharp
public class IntVerifier : Verifier<IntVerifier, Base>
{
    private int _value;

    public IntVerifier(int value)
    {
        _value = value;

        AddCriticalVerifiers(Assert.NotNull);
        AddCriticalVerifiers(sut => Assert.IsType(typeof(IntClass), sut));
    }

    protected override void AddDynamicVerifiers(Base instanceUnderTest)
    {
        var intObj = (IntClass)instanceUnderTest;

        AddNormalVerifiers(sut => Assert.Equal(_value, intObj.Value));
    }
}

public class StringVerifier : Verifier<StringVerifier, Base>
{
    private string _value;

    public StringVerifier(string value)
    {
        _value = value;

        AddCriticalVerifiers(Assert.NotNull);
        AddCriticalVerifiers(sut => Assert.IsType(typeof(StringClass), sut));
    }

    protected override void AddDynamicVerifiers(Base instanceUnderTest)
    {
        var stringObj = (StringClass)instanceUnderTest;

        AddNormalVerifiers(sut => Assert.Equal(_value, stringObj.Value));
    }
}

public class BaseListVerifier : CollectionVerifier<BaseListVerifier, Base>
{}
```
Now our verification can look like this:
```csharp
var verifier = new BaseListVerifier()
    .AddItemVerifiers(
        new StringVerifier("abc"),
        new IntVerifier(123)
    );
verifier.Check(list);
```
Or you can make your *BaseListVerifier* slightly more complex, to be able to write tests easier:
```csharp
public class BaseListVerifier : CollectionVerifier<BaseListVerifier, Base>
{
    public BaseListVerifier Expect(params object[] values)
    {
        foreach (var value in values)
        {
            if (value is string)
            {
                AddItemVerifiers(new StringVerifier((string) value));
            }
            if (value is int)
            {
                AddItemVerifiers(new IntVerifier((int)value));
            }
        }
        return this;
    }
}
```
In this case you can write your test like this:
```csharp
var verifier = new BaseListVerifier()
    .Expect("abc", 123);
verifier.Check(list);
```

## Verification and Visitor pattern

As you can see from the previous example, verification of objects of different types having one base class looks cumbersome. You have to make type casting to extract required data. But, if you use Visitor pattern for your classes, you can make your verification code cleaner. Let's say we have the same classes, but with implementation of Visitor pattern:
```csharp
public interface IBaseVisitor
{
    void VisitInt(int value);
    void VisitString(string value);
}

public abstract class Base
{
    public abstract void Visit(IBaseVisitor visitor);
}

public class IntClass : Base
{
    public int Value;

    public override void Visit(IBaseVisitor visitor)
    {
        visitor.VisitInt(Value);
    }
}

public class StringClass : Base
{
    public string Value;

    public override void Visit(IBaseVisitor visitor)
    {
        visitor.VisitString(Value);
    }
}
```
Now our verifiers can look like this:
```csharp
public abstract class BaseVerifier<TVerifier> : Verifier<TVerifier, Base>, IBaseVisitor
    where TVerifier : BaseVerifier<TVerifier>
{
    protected bool Executed;

    protected BaseVerifier()
    {
        AddCriticalVerifiers(Assert.NotNull);
    }

    protected override void AddDynamicVerifiers(Base instanceUnderTest)
    {
        Executed = false;
        instanceUnderTest.Visit(this);
        if (!Executed)
        {
            AddCriticalVerifiers(iut => throw new InvalidOperationException("Visitor was not called"));
        }

        base.AddDynamicVerifiers(instanceUnderTest);
    }

    public virtual void VisitInt(int value)
    {}

    public virtual void VisitString(string value)
    {}
}

public class IntVerifier : BaseVerifier<IntVerifier>
{
    private int _value;

    public IntVerifier(int value)
    {
        _value = value;
    }

    public override void VisitInt(int value)
    {
        Executed = true;

        AddNormalVerifiers(sut => Assert.Equal(_value, value));
    }
}

public class StringVerifier : BaseVerifier<StringVerifier>
{
    private string _value;

    public StringVerifier(string value)
    {
        _value = value;
    }

    public override void VisitString(string value)
    {
        Executed = true;

        AddNormalVerifiers(sut => Assert.Equal(_value, value));
    }
}
```
Now we get rid of type casts, and work with clean data.

## NoObjectVerifier

There is a class *NoObjectVerifier*, which allows you to make verification without object. You can use it for auxiliary verifications:
```csharp
    public class NotTooSmallVerifier : NoObjectVerifier<NotTooSmallVerifier>
    {
        public Func<int> GetLength { get; set; }

        public NotTooSmallVerifier(int minLength)
        {
            AddNormalVerifiers(() =>
            {
                Assert.True(GetLength() > minLength);
            });
        }
    }

    public class StringVerifier : Verifier<StringVerifier, string>
    {
        public StringVerifier ExpectMinLength(int minLength)
        {
            AddVerifiers(sut =>
            {
                var verifier = new NotTooSmallVerifier(minLength)
                {
                    GetLength = () => sut.Length
                };
                return verifier.Verify();
            });
            return this;
        }
    }

    public class CollectionVerifier : CollectionVerifier<CollectionVerifier, int>
    {
        public CollectionVerifier ExpectMinLength(int minLength)
        {
            AddVerifiers(sut =>
            {
                var verifier = new NotTooSmallVerifier(minLength)
                {
                    GetLength = sut.Count
                };
                return verifier.Verify();
            });
            return this;
        }
    }
```