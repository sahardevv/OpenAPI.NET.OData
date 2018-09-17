﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using Microsoft.OData.Edm;
using Microsoft.OpenApi.OData.Edm;
using Microsoft.OpenApi.OData.Tests;
using System.Linq;
using Xunit;

namespace Microsoft.OpenApi.OData.Operation.Tests
{
    public class NavigationPropertyPatchOperationHandlerTests
    {
        private NavigationPropertyPatchOperationHandler _operationHandler = new NavigationPropertyPatchOperationHandler();

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CreateNavigationPatchOperationReturnsCorrectOperation(bool enableOperationId)
        {
            // Arrange
            IEdmModel model = EdmModelHelper.TripServiceModel;
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                EnableOperationId = enableOperationId
            };
            ODataContext context = new ODataContext(model, settings);
            IEdmEntitySet people = model.EntityContainer.FindEntitySet("People");
            Assert.NotNull(people);

            IEdmEntityType person = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Person");
            IEdmNavigationProperty navProperty = person.DeclaredNavigationProperties().First(c => c.Name == "BestFriend");
            ODataPath path = new ODataPath(new ODataNavigationSourceSegment(people), new ODataKeySegment(people.EntityType()), new ODataNavigationPropertySegment(navProperty));

            // Act
            var operation = _operationHandler.CreateOperation(context, path);

            // Assert
            Assert.NotNull(operation);
            Assert.Equal("Update the navigation property BestFriend in People", operation.Summary);
            Assert.NotNull(operation.Tags);
            var tag = Assert.Single(operation.Tags);
            Assert.Equal("People.Person", tag.Name);

            Assert.NotNull(operation.Parameters);
            Assert.Equal(1, operation.Parameters.Count);

            Assert.NotNull(operation.RequestBody);
            Assert.Equal("New navigation property values", operation.RequestBody.Description);

            Assert.Equal(2, operation.Responses.Count);
            Assert.Equal(new string[] { "204", "default" }, operation.Responses.Select(e => e.Key));

            if (enableOperationId)
            {
                Assert.Equal("People.UpdateBestFriend", operation.OperationId);
            }
            else
            {
                Assert.Null(operation.OperationId);
            }
        }
    }
}
