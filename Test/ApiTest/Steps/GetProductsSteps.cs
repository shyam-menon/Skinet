using ApiTest.Base;
using ApiTest.Dtos;
using ApiTest.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TechTalk.SpecFlow;

namespace ApiTest.Steps
{
    [Binding]
    class GetProductsSteps
    {
        private Settings _settings;
        public GetProductsSteps(Settings settings) => _settings = settings;


        [Given(@"I perform GET operation for ""(.*)""")]
        public void GivenIPerformGETOperationFor(string url)
        {
            _settings.Request = new RestRequest(url, Method.GET);
        }       

        [Given(@"I perform operation for product ""(.*)""")]
        public void GivenIPerformOperationForProduct(int productid)
        {            
            _settings.Request.AddUrlSegment("productid", productid.ToString());
            _settings.Response = _settings.RestClient.ExecuteAsyncRequest<ProductToReturnDto>(_settings.Request).GetAwaiter().GetResult();
        }       


        [Then(@"I should see the ""(.*)"" name as ""(.*)""")]
        public void ThenIShouldSeeTheProductTypeAs(string key, string value)
        {
            var productTypeFromResponse = _settings.Response.GetResponseObject(key);
            var productTypeFromScenario = value;
            Assert.AreEqual(productTypeFromResponse, productTypeFromScenario, $"The {key} is not matching");
        }

        [Given(@"I perform operation for products")]
        public void GivenIPerformOperationForProducts()
        {
            _settings.Response = _settings.RestClient.ExecuteAsyncRequest<List<ProductToReturnDto>>(_settings.Request).GetAwaiter().GetResult();
        }

        [Then(@"I should see count of products greater than (.*)")]
        public void ThenIShouldSeeCountOfProductsGreaterThan(int zeroProducts)
        {
            var content = _settings.Response.Content;

            var productList = new JsonDeserializer().Deserialize<List<Dictionary<string, string>>>(_settings.Response);

            Assert.IsTrue(productList.Count > zeroProducts);
        }
    }
}
