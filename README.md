# Builder for unit test mocks purpose

This repo shows a purpose to make unit test classes more legible and organized when you need to mock your dependencies. 

# Before the approach

The folder `RecurrentPayment.TestBefore` shows a common approach when we need to mock something but it have a lot of duplicated code and the test comes in very verbose. 

# With the approach applied

The folder `RecurrentPayment.TestAfter` shows another purpose to make exactly the same thing implemented in `RecurrentPayment.TestBefore` folder, but introducing a builder for the class that will be tested.