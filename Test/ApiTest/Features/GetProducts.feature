Feature: GetProducts
	In order view the available products 
	As a non-logged in user
	I want to be able to get available products

@products
Scenario: Verify producttype of the product 18 
	Given I perform GET operation for "products/{productid}"
	And I perform operation for product "18"
	Then I should see the "productType" name as "Boots"

Scenario: Verify count of products
	Given I perform GET operation for "products"
	And I perform operation for products
	Then I should see count of products greater than 0

