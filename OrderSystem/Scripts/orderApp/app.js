/* global angular */
/* global $ */

/* 
public partial class MenuSubClass{
    public int AutoId { get; set; }
    public string SubClassId { get; set; }
    public string SubClassName { get; set; }
    public string ClassId { get; set; }
    public string SubClassRemark { get; set; }
    public Nullable<bool> Usable { get; set; }
    public string Creator { get; set; }
    public Nullable<System.DateTime> CreateDate { get; set; }
    public string Updator { get; set; }
    public Nullable<System.DateTime> UpdateDate { get; set; }
    public string Deletor { get; set; }
    public Nullable<System.DateTime> DeleteDate { get; set; }
}

public partial class MenuDetail{
    public int AutoId { get; set; }
    public string DisherId { get; set; }
    public string DisherCode { get; set; }
    public string DisherName { get; set; }
    public string DisherPicture { get; set; }
    public string DisherEnglishName { get; set; }
    public string DisherDescription { get; set; }
    public string DisherSubclassID1 { get; set; }
    public string DisherSubclassID2 { get; set; }
    public string DisherSize { get; set; }
    public Nullable<decimal> DisherPrice { get; set; }
    public Nullable<bool> DisherStatus { get; set; }
    public Nullable<decimal> DisherVipPrice { get; set; }
    public Nullable<decimal> DisherServicePrice { get; set; }
    public bool Usable { get; set; }
    public Nullable<double> DisherDiscount { get; set; }
    public string DepartmentId { get; set; }
    public Nullable<int> DisherPoint { get; set; }
    public Nullable<short> SourIndex { get; set; }
    public Nullable<short> SweetIndex { get; set; }
    public Nullable<short> SaltyIndex { get; set; }
    public Nullable<short> SpicyIndex { get; set; }
    public Nullable<short> Evaluate { get; set; }
    public string Creator { get; set; }
    public Nullable<System.DateTime> CreateDate { get; set; }
    public string Updator { get; set; }
    public Nullable<System.DateTime> updateDate { get; set; }
    public string Deletor { get; set; }
    public Nullable<System.DateTime> DeleteDate { get; set; }
}
public partial class Note{
    public int AutoID { get; set; }
    public string Note1 { get; set; }
}
*/


var app = angular.module('orderApp', []);

app.directive('ngStaticHeight', function () {
	return function ($scope, $elem) {
		$elem.height($(window).height() - $('section.search').height() - $('section.search').height());
	};
});

app.factory('generateMenuSubClassPromise', ['$http', '$q', function ($http, $q) {
	return $q(function (resolve) {
		$http.get('/Home/GetMenuSubClass').success(function (data) {
			data.splice(0, 0, {
				SubClassName: '销量榜',
				IsRankClass: true
			});
			for (var i = 0; i < data.length; i++) {
				data[i].cart = {
					isSelected: false,
					ordered: 0,
					isRemarkCollapsed: false
				};
			}
			data[0].cart.isSelected = true;

			resolve(data);
		}).error(function (data, status) {
			alert(status);
		});
	});
}]).factory('generateMenuDetailPromise', ['$http', '$q', function ($http, $q) {
	return $q(function (resolve) {
		$http.get('/Home/GetMenuDetail').success(function (data) {
			for (var i = 0; i < data.length; i++) {
				if (data[i].DisherPoint == null) {
					data[i].DisherPoint = 0;
				}
				data[i].cart = {
					ordered: 0,
					notes:[]
				};
			}
			resolve(data);
		}).error(function (data, status) {
			alert(status);
		});
	});
}]).factory('generateRemarkPromise', ['$http', '$q', function ($http, $q) {
	return $q(function (resolve) {
		$http.get('/Home/GetNote').success(function (data) {
			resolve(data);
		}).error(function (data, status) {
			alert(status);
		});
	});
}]);


app.controller('cartCtrl', [
	'$scope',
	'$filter',
	'generateMenuSubClassPromise',
	'generateMenuDetailPromise',
	'generateRemarkPromise',
	function ($scope, $filter, GMSCP, GMDP,GRP) {
		var activeClass;
		GMSCP.then(function (data) {
			$scope.menuSubClass = data;
			activeClass = $scope.menuSubClass[0];
		});
		GMDP.then(function (data) {
			$scope.menuDetail = data;
			filterMenu();
		});
		GRP.then(function(data){
			$scope.notes = data;
		});

		$scope.test  = function(menu,note){
			menu.cart.notes.push(note);
			console.log(menu.cart.notes);
		};

		$scope.sizeAll = 0;
		$scope.priceAll = 0;
		$scope.add = function (menu) {
			menu.cart.ordered++;
			$scope.sizeAll++;
			$scope.priceAll += menu.DisherPrice;
			for (var i = 0; i < $scope.menuSubClass.length; i++) {
				if ($scope.menuSubClass[i].SubClassId == menu.DisherSubclassID1) {
					$scope.menuSubClass[i].cart.ordered++;
					return;
				}
			}
		};
		$scope.decrease = function (menu) {
			menu.cart.ordered--;
			$scope.sizeAll--;
			$scope.priceAll -= menu.DisherPrice;
			for (var i = 0; i < $scope.menuSubClass.length; i++) {
				if ($scope.menuSubClass[i].SubClassId == menu.DisherSubclassID1) {
					$scope.menuSubClass[i].cart.ordered--;
					return;
				}
			}
		};
		$scope.toggleSelected = function (c) {
			// exit the search mode
			$scope.searchMode = false;
			$scope.searchText = '';

			activeClass.cart.isSelected = false;
			c.cart.isSelected = true;
			activeClass = c;
			filterMenu();
		};

		$scope.searchMode = false;
		$scope.searchChange = function (searchText) {
			if (searchText == '') {
				// exit the search mode
				$scope.searchMode = false;
				activeClass.cart.isSelected = true;
				filterMenu();
			} else {
				// enter the search mode
				$scope.searchMode = true;
				activeClass.cart.isSelected = false;
				filterMenu(searchText);
			}
		};


		function filterMenu(searchText) {
			var filteredArr = $scope.menuDetail;
			if ($scope.searchMode) {
				// the filterMenu is filtered by searchText
				filteredArr = $filter('filter')($scope.menuDetail, {
					DisherName: searchText
				});
			} else if (activeClass.IsRankClass == true) {
				// the filterMenu is a ranklist
				filteredArr = $filter('orderBy')(filteredArr, '-DisherPoint');
				filteredArr = $filter('limitTo')(filteredArr, 10);
			} else {
				// the filterMenu is classified by subclassid
				filteredArr = $filter('filter')(filteredArr, {
					DisherSubclassID1: activeClass.SubClassId
				}, true);
				filteredArr = $filter('orderBy')(filteredArr, '-DisherPoint');
			}
			$scope.filteredMenuDetail = filteredArr;
		}
	}
]);