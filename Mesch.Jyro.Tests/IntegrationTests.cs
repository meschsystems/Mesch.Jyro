using Xunit.Abstractions;

namespace Mesch.Jyro.Tests;

public class IntegrationTests
{
    private readonly ITestOutputHelper _output;

    public IntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void LeaderboardScenario_ProcessesPersonsAndCreatesRankedLeaderboard()
    {
        // This test is based on the leaderboard.jyro script
        var script = @"
            Data.leaderboard = []

            foreach p in Data.persons do
                # start with empty ribbons array
                var ribbons = []

                # award ribbons for each category >= 50
                if p.categories[""Customer Service""] >= 50 then
                    Append(ribbons, ""Customer Service"")
                end

                if p.categories[""Safe Working""] >= 50 then
                    Append(ribbons, ""Safe Working"")
                end

                if p.categories[""Team Player""] >= 50 then
                    Append(ribbons, ""Team Player"")
                end

                switch true do
                    case p.points >= 100 then
                        p.tier = ""Gold""
                    case p.points >= 50 then
                        p.tier = ""Silver""
                    default then
                        p.tier = ""Bronze""
                end

                Append(
                    Data.leaderboard,
                    {
                        ""Name"": p.lastName + "", "" + p.firstName,
                        ""Points"": p.points,
                        ""Tier"": p.tier,
                        ""Ribbons"": ribbons
                    }
                )
            end

            # sort by points, descending
            SortByField(Data.leaderboard, ""Points"", ""desc"")

            # assign ranks
            var rank = 1
            foreach entry in Data.leaderboard do
                entry.Rank = rank
                rank = rank + 1
            end
        ";

        var testData = new JyroObject();
        var persons = new JyroArray();

        // Person 1: High performer
        var person1 = new JyroObject();
        person1.SetProperty("firstName", new JyroString("Alice"));
        person1.SetProperty("lastName", new JyroString("Smith"));
        person1.SetProperty("points", new JyroNumber(120));
        var categories1 = new JyroObject();
        categories1.SetProperty("Customer Service", new JyroNumber(75));
        categories1.SetProperty("Safe Working", new JyroNumber(60));
        categories1.SetProperty("Team Player", new JyroNumber(55));
        person1.SetProperty("categories", categories1);
        persons.Add(person1);

        // Person 2: Medium performer
        var person2 = new JyroObject();
        person2.SetProperty("firstName", new JyroString("Bob"));
        person2.SetProperty("lastName", new JyroString("Johnson"));
        person2.SetProperty("points", new JyroNumber(75));
        var categories2 = new JyroObject();
        categories2.SetProperty("Customer Service", new JyroNumber(55));
        categories2.SetProperty("Safe Working", new JyroNumber(40));
        categories2.SetProperty("Team Player", new JyroNumber(30));
        person2.SetProperty("categories", categories2);
        persons.Add(person2);

        // Person 3: Low performer
        var person3 = new JyroObject();
        person3.SetProperty("firstName", new JyroString("Charlie"));
        person3.SetProperty("lastName", new JyroString("Davis"));
        person3.SetProperty("points", new JyroNumber(35));
        var categories3 = new JyroObject();
        categories3.SetProperty("Customer Service", new JyroNumber(20));
        categories3.SetProperty("Safe Working", new JyroNumber(15));
        categories3.SetProperty("Team Player", new JyroNumber(10));
        person3.SetProperty("categories", categories3);
        persons.Add(person3);

        testData.SetProperty("persons", persons);

        var result = TestHelpers.ExecuteSuccessfully(script, testData, _output);

        var data = (JyroObject)result.Data;
        var leaderboard = (JyroArray)data.GetProperty("leaderboard");

        // Verify leaderboard structure
        Assert.Equal(3, leaderboard.Length);

        // Verify rank 1 (Alice)
        var first = (JyroObject)leaderboard[0];
        Assert.Equal("Smith, Alice", ((JyroString)first.GetProperty("Name")).Value);
        Assert.Equal(120.0, ((JyroNumber)first.GetProperty("Points")).Value);
        Assert.Equal("Gold", ((JyroString)first.GetProperty("Tier")).Value);
        Assert.Equal(1.0, ((JyroNumber)first.GetProperty("Rank")).Value);
        var ribbons1 = (JyroArray)first.GetProperty("Ribbons");
        Assert.Equal(3, ribbons1.Length); // All three ribbons

        // Verify rank 2 (Bob)
        var second = (JyroObject)leaderboard[1];
        Assert.Equal("Johnson, Bob", ((JyroString)second.GetProperty("Name")).Value);
        Assert.Equal(75.0, ((JyroNumber)second.GetProperty("Points")).Value);
        Assert.Equal("Silver", ((JyroString)second.GetProperty("Tier")).Value);
        Assert.Equal(2.0, ((JyroNumber)second.GetProperty("Rank")).Value);
        var ribbons2 = (JyroArray)second.GetProperty("Ribbons");
        Assert.Equal(1, ribbons2.Length); // Only Customer Service ribbon

        // Verify rank 3 (Charlie)
        var third = (JyroObject)leaderboard[2];
        Assert.Equal("Davis, Charlie", ((JyroString)third.GetProperty("Name")).Value);
        Assert.Equal(35.0, ((JyroNumber)third.GetProperty("Points")).Value);
        Assert.Equal("Bronze", ((JyroString)third.GetProperty("Tier")).Value);
        Assert.Equal(3.0, ((JyroNumber)third.GetProperty("Rank")).Value);
        var ribbons3 = (JyroArray)third.GetProperty("Ribbons");
        Assert.Equal(0, ribbons3.Length); // No ribbons
    }

    [Fact]
    public void DataTransformation_FiltersAndMapsArray()
    {
        var script = @"
            Data.result = []

            foreach item in Data.items do
                if item.active == true and item.price > 10 then
                    Append(Data.result, {
                        ""id"": item.id,
                        ""discountedPrice"": item.price * 0.9
                    })
                end
            end
        ";

        var testData = new JyroObject();
        var items = new JyroArray();

        var item1 = new JyroObject();
        item1.SetProperty("id", new JyroNumber(1));
        item1.SetProperty("price", new JyroNumber(15));
        item1.SetProperty("active", JyroBoolean.FromBoolean(true));
        items.Add(item1);

        var item2 = new JyroObject();
        item2.SetProperty("id", new JyroNumber(2));
        item2.SetProperty("price", new JyroNumber(5));
        item2.SetProperty("active", JyroBoolean.FromBoolean(true));
        items.Add(item2);

        var item3 = new JyroObject();
        item3.SetProperty("id", new JyroNumber(3));
        item3.SetProperty("price", new JyroNumber(20));
        item3.SetProperty("active", JyroBoolean.FromBoolean(false));
        items.Add(item3);

        testData.SetProperty("items", items);

        var result = TestHelpers.ExecuteSuccessfully(script, testData, _output);

        var data = (JyroObject)result.Data;
        var resultArray = (JyroArray)data.GetProperty("result");

        _output.WriteLine($"Result array length: {resultArray.Length}");
        for (int i = 0; i < resultArray.Length; i++)
        {
            var item = (JyroObject)resultArray[i];
            var id = ((JyroNumber)item.GetProperty("id")).Value;
            var price = ((JyroNumber)item.GetProperty("discountedPrice")).Value;
            _output.WriteLine($"  Item {i}: id={id}, discountedPrice={price}");
        }

        Assert.Equal(1, resultArray.Length); // Only item1 should match
        var resultItem = (JyroObject)resultArray[0];
        Assert.Equal(1.0, ((JyroNumber)resultItem.GetProperty("id")).Value);
        Assert.Equal(13.5, ((JyroNumber)resultItem.GetProperty("discountedPrice")).Value);
    }

    [Fact]
    public void ComplexDataAggregation_CalculatesStatistics()
    {
        var script = @"
            var total = 0
            var count = 0
            var maxValue = 0

            foreach item in Data.values do
                total = total + item
                count = count + 1
                if item > maxValue then
                    maxValue = item
                end
            end

            Data.average = total / count
            Data.total = total
            Data.count = count
            Data.max = maxValue
        ";

        var testData = new JyroObject();
        var values = new JyroArray();
        values.Add(new JyroNumber(10));
        values.Add(new JyroNumber(20));
        values.Add(new JyroNumber(30));
        values.Add(new JyroNumber(40));
        testData.SetProperty("values", values);

        var result = TestHelpers.ExecuteSuccessfully(script, testData, _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(25.0, ((JyroNumber)data.GetProperty("average")).Value);
        Assert.Equal(100.0, ((JyroNumber)data.GetProperty("total")).Value);
        Assert.Equal(4.0, ((JyroNumber)data.GetProperty("count")).Value);
        Assert.Equal(40.0, ((JyroNumber)data.GetProperty("max")).Value);
    }

    [Fact]
    public void NestedDataTransformation_BuildsHierarchy()
    {
        var script = @"
            Data.departments = []

            foreach dept in Data.rawDepartments do
                var employees = []

                foreach emp in Data.rawEmployees do
                    if emp.departmentId == dept.id then
                        Append(employees, {
                            ""name"": emp.name,
                            ""salary"": emp.salary
                        })
                    end
                end

                Append(Data.departments, {
                    ""name"": dept.name,
                    ""employees"": employees,
                    ""employeeCount"": Length(employees)
                })
            end
        ";

        var testData = new JyroObject();

        var rawDepartments = new JyroArray();
        var dept1 = new JyroObject();
        dept1.SetProperty("id", new JyroNumber(1));
        dept1.SetProperty("name", new JyroString("Engineering"));
        rawDepartments.Add(dept1);

        var dept2 = new JyroObject();
        dept2.SetProperty("id", new JyroNumber(2));
        dept2.SetProperty("name", new JyroString("Sales"));
        rawDepartments.Add(dept2);

        testData.SetProperty("rawDepartments", rawDepartments);

        var rawEmployees = new JyroArray();
        var emp1 = new JyroObject();
        emp1.SetProperty("name", new JyroString("Alice"));
        emp1.SetProperty("departmentId", new JyroNumber(1));
        emp1.SetProperty("salary", new JyroNumber(100000));
        rawEmployees.Add(emp1);

        var emp2 = new JyroObject();
        emp2.SetProperty("name", new JyroString("Bob"));
        emp2.SetProperty("departmentId", new JyroNumber(1));
        emp2.SetProperty("salary", new JyroNumber(90000));
        rawEmployees.Add(emp2);

        var emp3 = new JyroObject();
        emp3.SetProperty("name", new JyroString("Charlie"));
        emp3.SetProperty("departmentId", new JyroNumber(2));
        emp3.SetProperty("salary", new JyroNumber(80000));
        rawEmployees.Add(emp3);

        testData.SetProperty("rawEmployees", rawEmployees);

        var result = TestHelpers.ExecuteSuccessfully(script, testData, _output);

        var data = (JyroObject)result.Data;
        var departments = (JyroArray)data.GetProperty("departments");

        Assert.Equal(2, departments.Length);

        var engineering = (JyroObject)departments[0];
        Assert.Equal("Engineering", ((JyroString)engineering.GetProperty("name")).Value);
        Assert.Equal(2.0, ((JyroNumber)engineering.GetProperty("employeeCount")).Value);

        var sales = (JyroObject)departments[1];
        Assert.Equal("Sales", ((JyroString)sales.GetProperty("name")).Value);
        Assert.Equal(1.0, ((JyroNumber)sales.GetProperty("employeeCount")).Value);
    }

    [Fact]
    public void ConditionalBusinessLogic_AppliesComplexRules()
    {
        var script = @"
            Data.orders = []

            foreach order in Data.rawOrders do
                var discount = 0
                var shippingCost = 10

                # Apply volume discount
                if order.quantity >= 10 then
                    discount = 0.15
                elseif order.quantity >= 5 then
                    discount = 0.10
                end

                # Free shipping for large orders
                if order.total >= 100 then
                    shippingCost = 0
                end

                # Calculate final price
                var subtotal = order.total * (1 - discount)
                var finalPrice = subtotal + shippingCost

                Append(Data.orders, {
                    ""orderId"": order.id,
                    ""originalPrice"": order.total,
                    ""discount"": discount * 100,
                    ""shippingCost"": shippingCost,
                    ""finalPrice"": finalPrice
                })
            end
        ";

        var testData = new JyroObject();
        var rawOrders = new JyroArray();

        var order1 = new JyroObject();
        order1.SetProperty("id", new JyroNumber(1));
        order1.SetProperty("total", new JyroNumber(150));
        order1.SetProperty("quantity", new JyroNumber(12));
        rawOrders.Add(order1);

        var order2 = new JyroObject();
        order2.SetProperty("id", new JyroNumber(2));
        order2.SetProperty("total", new JyroNumber(50));
        order2.SetProperty("quantity", new JyroNumber(3));
        rawOrders.Add(order2);

        testData.SetProperty("rawOrders", rawOrders);

        var result = TestHelpers.ExecuteSuccessfully(script, testData, _output);

        var data = (JyroObject)result.Data;
        var orders = (JyroArray)data.GetProperty("orders");

        Assert.Equal(2, orders.Length);

        var processedOrder1 = (JyroObject)orders[0];
        Assert.Equal(150.0, ((JyroNumber)processedOrder1.GetProperty("originalPrice")).Value);
        Assert.Equal(15.0, ((JyroNumber)processedOrder1.GetProperty("discount")).Value); // Should be 0.15 * 100 when quantity >= 10
        Assert.Equal(0.0, ((JyroNumber)processedOrder1.GetProperty("shippingCost")).Value);
        Assert.Equal(127.5, ((JyroNumber)processedOrder1.GetProperty("finalPrice")).Value); // Should be 150 * 0.85 + 0

        var processedOrder2 = (JyroObject)orders[1];
        Assert.Equal(50.0, ((JyroNumber)processedOrder2.GetProperty("originalPrice")).Value);
        Assert.Equal(0.0, ((JyroNumber)processedOrder2.GetProperty("discount")).Value);
        Assert.Equal(10.0, ((JyroNumber)processedOrder2.GetProperty("shippingCost")).Value);
        Assert.Equal(60.0, ((JyroNumber)processedOrder2.GetProperty("finalPrice")).Value);
    }
}
