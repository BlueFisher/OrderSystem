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


var app = angular.module('orderApp', ['ngRoute','ui.bootstrap']);

app.config(function ($routeProvider) {
    $routeProvider
		.when('/', {
		templateUrl: 'Home/Partial/partial-cart',
		controller: 'cartCtrl'
	}).when('/result', {
		templateUrl: 'Home/Partial/partial-result',
		controller: 'resultCtrl'
	}).when('/error', {
		templateUrl: 'Home/Partial/partial-error',
		controller: 'errorCtrl'
	});
});

app.directive('ngStaticHeight', function () {
	return function ($scope, $elem,attr) {
		console.log(attr);
		$elem.height($(window).height() - 34 - 40);
	};
}).directive('convertToNumber', function() {
  return {
    require: 'ngModel',
    link: function(scope, element, attrs, ngModel) {
      ngModel.$parsers.push(function(val) {
        return parseInt(val, 10);
      });
      ngModel.$formatters.push(function(val) {
        return '' + val;
      });
    }
  };
});

app.factory('generateMenuSubClassPromise', ['$http', '$q', function ($http, $q) {
	return $q(function (resolve) {
		$http.get('/Home/GetMenuSubClass').success(function (data) {
			for (var i = 0; i < data.length; i++) {
				data[i].cart = {
					isSelected: false,
					ordered: 0,
				};
			}

			resolve(data);
		}).error(function (data, status) {
			alert(status);
		});
	});
}]).factory('generateMenuDetailPromise', ['$http', '$q', function ($http, $q) {
	return $q(function (resolve) {
		$http.get('/Home/GetMenuDetail').success(function (data) {
			for (var i = 0; i < data.length; i++) {
				// delete data[i].AutoId;
				// delete data[i].DisherCode;
				// delete data[i].DisherEnglishName;
				// delete data[i].DisherDescription;
				// delete data[i].DisherSubclassID2;
				// delete data[i].DisherStatus;
				// delete data[i].Usable;
				// delete data[i].DepartmentId;
				// delete data[i].SourIndex;
				// delete data[i].SweetIndex;
				// delete data[i].SaltyIndex;
				// delete data[i].SpicyIndex;
				// delete data[i].Evaluate;
				// delete data[i].Creator;
				// delete data[i].Updator;
				// delete data[i].Deletor;
				// delete data[i].CreateDate;
				// delete data[i].updateDate;
				// delete data[i].DeleteDate;
				if (data[i].DisherPoint == null) {
					data[i].DisherPoint = 0;
				}
				data[i].cart = {
					ordered: 0,
					notes: [],
					filteredNotes: [],
					isNoteCollapsed: false
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
	'$rootScope',
	'$filter',
	'$location',
	'generateMenuSubClassPromise',
	'generateMenuDetailPromise',
	'generateRemarkPromise',
	function ($scope, $rootScope, $filter, $location, GMSCP, GMDP, GRP) {
		$rootScope.customer = 1;
		$rootScope.redirect = function (href) {
			$location.path(href);
		};
		$rootScope.isCart = true;
		if ($rootScope.cart == undefined) {
			$rootScope.cart = {
				isInitialized: false,
				activeClass: null,
				sizeAll: null,
				priceAll: null,
				results: [],
				customer:1,
				bill:''
			};
		}
		var rootCart = $rootScope.cart;
		
		var param = $location.search();
		if (param.table == undefined) {
			$location.path('/');
			$location.search('table','1');
		} else {
			rootCart.table = parseInt(param.table);
		}
		
		var menuDetail;
		GMSCP.then(function (classes) {
			$scope.menuSubClass = classes;
			if (!rootCart.isInitialized) {
				rootCart.sizeAll = 0;
				rootCart.priceAll = 0;
				rootCart.activeClass = $scope.menuSubClass[0];
			}

			GMDP.then(function (menus) {
				menuDetail = menus;

				GRP.then(function (notes) {
					if (!rootCart.isInitialized) {
						for (var i = 0; i < menuDetail.length; i++) {
							menuDetail[i].cart.filteredNotes = angular.copy(notes);
						}
					}
					_filterMenu();
					rootCart.isInitialized = true;
				});
			});
		});

		var classMode = {
			search: 0,
			normal: 1,
			rank: 2
		};
		_changeMode(classMode.rank);
		$scope.isRankMode = function () {
			return $scope.currentMode == classMode.rank;
		};
		$scope.isSearchMode = function () {
			return $scope.currentMode == classMode.search;
		};
		function _changeMode(mode) {
			$scope.currentMode = mode;
			if (mode != classMode.search) {
				$scope.searchText = '';
			}
			if (mode != classMode.normal && rootCart.isInitialized) {
				rootCart.activeClass.cart.isSelected = false;
			}
		}

		// click subclass
		$scope.toggleSelected = function (c) {
			// enter the normalMode
			_changeMode(classMode.normal);

			rootCart.activeClass.cart.isSelected = false;
			c.cart.isSelected = true;
			rootCart.activeClass = c;
			_filterMenu();
		};

		$scope.enterRankMode = function () {
			_changeMode(classMode.rank);
			_filterMenu();
		};

		$scope.searchChange = function (searchText) {
			if (searchText == '') {
				// exit the search mode
				_changeMode(classMode.rank);
				_filterMenu();
			} else {
				// enter the search mode
				_changeMode(classMode.search);
				_filterMenu(searchText);
			}
		};
		
		// add and remove Meal
		$rootScope.addMenu = function (menu) {
			menu.cart.ordered++;
			rootCart.sizeAll++;
			rootCart.priceAll += menu.DisherPrice;
			for (var i = 0; i < $scope.menuSubClass.length; i++) {
				if ($scope.menuSubClass[i].SubClassId == menu.DisherSubclassID1) {
					$scope.menuSubClass[i].cart.ordered++;
					break;
				}
			}
			if (menu.cart.ordered == 1) {
				rootCart.results.push(menu);
			}
		};
		$rootScope.removeMenu = function (menu) {
			menu.cart.ordered--;
			rootCart.sizeAll--;
			rootCart.priceAll -= menu.DisherPrice;
			for (var i = 0; i < $scope.menuSubClass.length; i++) {
				if ($scope.menuSubClass[i].SubClassId == menu.DisherSubclassID1) {
					$scope.menuSubClass[i].cart.ordered--;
					break;
				}
			}
			if (menu.cart.ordered == 0) {
				for (var i = 0; i < rootCart.results.length; i++) {
					if (angular.equals(menu, rootCart.results[i])) {
						rootCart.results.splice(i, 1);
						break;
					}
				}
			}
		};
		// add and remove Remark
		$rootScope.addNote = function (menu, note, index) {
			menu.cart.notes.push(note);
			menu.cart.filteredNotes.splice(index, 1);
		};
		$rootScope.removeNote = function (menu, note, index) {
			menu.cart.filteredNotes.push(note);
			menu.cart.notes.splice(index, 1);
		};


		function _filterMenu(searchText) {
			var filteredArr = menuDetail;

			switch ($scope.currentMode) {
				case classMode.normal:
					filteredArr = $filter('filter')(filteredArr, {
						DisherSubclassID1: rootCart.activeClass.SubClassId
					}, true);
					filteredArr = $filter('orderBy')(filteredArr, '-DisherPoint');
					break;
				case classMode.rank:
					filteredArr = $filter('orderBy')(filteredArr, '-DisherPoint');
					filteredArr = $filter('limitTo')(filteredArr, 10);
					break;
				case classMode.search:
					filteredArr = $filter('filter')(menuDetail, {
						DisherName: searchText
					});
					break;
			}

			$scope.filteredMenuDetail = filteredArr;
		}
	}
]);

app.controller('resultCtrl', [
	'$scope',
	'$rootScope',
	'$location',
	function ($scope, $rootScope, $location) {
		if ($rootScope.cart == undefined) {
			$location.path('/');
			$location.search('table','1');
			return;
		}
		
		$rootScope.isCart = false;
		$rootScope.submit = function(){
			console.log($rootScope.cart);
		};

		angular.forEach($rootScope.cart.results, function (menu) {
			menu.cart.isNoteCollapsed = false;
		});
	}
]);