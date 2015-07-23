/* global angular */

app.controller('indexCtrl', [
	'$scope',
	'$rootScope',
	'$location',
	function ($scope, $rootScope, $location) {
		$rootScope.viewTitle = '店小二';
		$rootScope.hideBackBtn = true;
	}
]);


app.controller('cartCtrl', [
	'$scope',
	'$rootScope',
	'$filter',
	'$location',
	'getTable',
	'generateMenuSubClassPromise',
	'generateMenuDetailPromise',
	'generateRemarkPromise',
	'$modal',
	function ($scope, $rootScope, $filter, $location, GT, GMSCP, GMDP, GRP,$modal) {
		// test! if the url doesn't have qrCode data then redirect to the one has qrCode = 101
		if ($location.search().qrCode == undefined) {
			$location.search('qrCode', '101');
			return;
		}

		$rootScope.viewTitle = '菜单';
		$rootScope.hideBackBtn = true;

		var classMode = {
			search: 0,
			normal: 1,
			rank: 2,
			result: 3
		};
		var rootCart; // The abbr. of  $rootScope.cart
		var activeClass; // The current selected class
		
		if ($rootScope.cart == null) {
			$rootScope.cart = {
				SizeAll: 0,
				PriceAll: 0,
				Results: [],
				Table: null,
				Customer: 0,
				Bill: '',
				IsPaid: 0,
				PayKind: null
			};
			rootCart = $rootScope.cart;
			$rootScope.menuSubClass = null // The root data that contains subclass information
			$rootScope.menuDetail = null //The root data that contains menu information

			// Get Table Info
			GT($location.search().qrCode).then(function (deskInfo) {
				rootCart.Table = deskInfo;
			});
			
			// GetMenuSubClass
			GMSCP().then(function (classes) {
				$rootScope.menuSubClass = classes;
				activeClass = classes[0];

				_changeMode(classMode.rank);
				
				// GetMenuDetail
				GMDP().then(function (menus) {
					$rootScope.menuDetail = menus;
					_filterMenu();
					// GetRemark
					GRP().then(function (notes) {
						for (var i = 0; i < $rootScope.menuDetail.length; i++) {
							$rootScope.menuDetail[i].Additional.FilteredNotes = angular.copy(notes);
						}
						_hasHistoryCart();
						
					});
				});
			});
		} else {
			// If the $rootScope is defined
			rootCart = $rootScope.cart;
			activeClass = $rootScope.menuSubClass[0];

			_changeMode(classMode.rank);
			_filterMenu();
			
			_hasHistoryCart();
		}
		
		function _hasHistoryCart(){
			if ($rootScope.historyCart != null) {
				$rootScope.cart.Customer = $rootScope.historyCart.Customer;
				
				for (var i = 0; i < $rootScope.historyCart.Results.length; i++) {
					for (var j = 0; j < $rootScope.menuDetail.length; j++) {
						if ($rootScope.historyCart.Results[i].DisherId == $rootScope.menuDetail[j].DisherId) {
							for (var k = 0; k < $rootScope.historyCart.Results[i].Ordered; k++) {
								$rootScope.addMenu($rootScope.menuDetail[j]);
							}
							break;
						}
					}
				}
			}
			$rootScope.historyCart = null;
		}
		
		$scope.showModal = function(menu){
			var modalInstance = $modal.open({
				animation: $scope.animationsEnabled,
				templateUrl: 'myModalContent.html',
				controller: 'ModalInstanceCtrl',
			});

			$rootScope.currentImgSrc = menu.DisherPicture;
			$rootScope.currentDisherName = menu.DisherName;
		}

		$scope.isRankMode = function () {
			return $scope.currentMode == classMode.rank;
		};
		$scope.isSearchMode = function () {
			return $scope.currentMode == classMode.search;
		};
		$scope.isResultMode = function () {
			return $scope.currentMode == classMode.result;
		}
		function _changeMode(mode) {
			$scope.currentMode = mode;
			if (mode != classMode.search) {
				$scope.searchText = '';
			}
			if (mode != classMode.normal) {
				activeClass.Additional.IsSelected = false;
			}
		}

		// click subclass
		$scope.toggleSelected = function (c) {
			// enter the normalMode
			_changeMode(classMode.normal);

			activeClass.Additional.IsSelected = false;
			c.Additional.IsSelected = true;
			activeClass = c;
			_filterMenu();
		};

		$scope.enterRankMode = function () {
			_changeMode(classMode.rank);
			_filterMenu();
		};
		$scope.enterResultMode = function () {
			_changeMode(classMode.result);
			_filterMenu();
		}

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
			menu.Additional.Ordered++;
			rootCart.SizeAll++;
			rootCart.PriceAll += menu.DisherPrice;
			for (var i = 0; i < $scope.menuSubClass.length; i++) {
				if ($scope.menuSubClass[i].SubClassId == menu.DisherSubclassID1) {
					$scope.menuSubClass[i].Additional.Ordered++;
					break;
				}
			}
			if (menu.Additional.Ordered == 1) {
				rootCart.Results.push(menu);
			}
		};
		$rootScope.removeMenu = function (menu) {
			menu.Additional.Ordered--;
			rootCart.SizeAll--;
			rootCart.PriceAll -= menu.DisherPrice;
			for (var i = 0; i < $scope.menuSubClass.length; i++) {
				if ($scope.menuSubClass[i].SubClassId == menu.DisherSubclassID1) {
					$scope.menuSubClass[i].Additional.Ordered--;
					break;
				}
			}
			if (menu.Additional.Ordered == 0) {
				for (var i = 0; i < rootCart.Results.length; i++) {
					if (angular.equals(menu, rootCart.Results[i])) {
						rootCart.Results.splice(i, 1);
						break;
					}
				}
			}
		};
		angular.forEach($rootScope.menuDetail,function(menu){
			menu.Additional.IsNoteCollapsed = false;
		});
		var activeNote = null;
		$scope.toggleNote = function(menu){
			if(activeNote != null && activeNote != menu){
				activeNote.Additional.IsNoteCollapsed = false;
			}
			activeNote = menu;
			menu.Additional.IsNoteCollapsed = !menu.Additional.IsNoteCollapsed
		}
		// add and remove Remark
		$rootScope.addNote = function (menu, note, index) {
			menu.Additional.Notes.push(note);
			menu.Additional.FilteredNotes.splice(index, 1);
		};
		$rootScope.removeNote = function (menu, note, index) {
			menu.Additional.FilteredNotes.push(note);
			menu.Additional.Notes.splice(index, 1);
		};

		function _filterMenu(searchText) {
			var filteredArr = $rootScope.menuDetail;

			switch ($scope.currentMode) {
				case classMode.normal:
					filteredArr = $filter('filter')(filteredArr, {
						DisherSubclassID1: activeClass.SubClassId
					}, true);
					filteredArr = $filter('orderBy')(filteredArr, '-DisherPoint');
					break;
				case classMode.rank:
					filteredArr = $filter('orderBy')(filteredArr, '-DisherPoint');
					filteredArr = $filter('limitTo')(filteredArr, 10);
					break;
				case classMode.search:
					filteredArr = $filter('filter')($rootScope.menuDetail, searchText);
					break;
				case classMode.result:
					filteredArr = rootCart.Results
					break;
			}

			$scope.filteredMenuDetail = filteredArr;
		}
	}
]).controller('resultCtrl', [
	'$scope',
	'$rootScope',
	'$location',
	'$modal',
	function ($scope, $rootScope, $location,$modal) {
		$rootScope.viewTitle = '查看订单';
		$rootScope.hideBackBtn = false;
		if ($rootScope.cart == null) {
			$location.path('/cart');
			return;
		}
		$scope.showModal = function(menu){
			var modalInstance = $modal.open({
				animation: $scope.animationsEnabled,
				templateUrl: 'myModalContent.html',
				controller: 'ModalInstanceCtrl',
			});

			$rootScope.currentImgSrc = menu.DisherPicture;
			$rootScope.currentDisherName = menu.DisherName;
		}
		var activeNote = null;
		$scope.toggleNote = function(menu){
			if(activeNote != null && activeNote != menu){
				activeNote.Additional.IsNoteCollapsed = false;
			}
			activeNote = menu;
			menu.Additional.IsNoteCollapsed = !menu.Additional.IsNoteCollapsed
		}

		// unselect all the classes
		angular.forEach($rootScope.menuSubClass, function (menu) {
			menu.Additional.IsSelected = false;
		});
		angular.forEach($rootScope.menuDetail,function(menu){
			menu.Additional.IsNoteCollapsed = false;
		});
	}
]).controller('paymentCtrl', [
	'$scope',
	'$rootScope',
	'$location',
	'$http',
	function ($scope, $rootScope, $location, $http) {
		$rootScope.viewTitle = '结算';
		$rootScope.hideBackBtn = false;
		if ($rootScope.cart == null) {
			$location.path('/cart');
			return;
		}

		$rootScope.customersAll = [];
		for (var i = 0; i < 50; i++) {
			$rootScope.customersAll.push(i + 1);
		}
		$http.post('/Cart/GetPayName').success(function (data) {
			$rootScope.pays = data;
		});
		$http.post('/Cart/GetTablewareFee').success(function (data) {
			$rootScope.tablewareFee = parseInt(data.TablewareFee);
		});

		$scope.offlinePay = function () {
			$rootScope.isOfflinePayment = true;
			$location.path('/paycompleted');
		};

		$scope.onlinePay = function (pay) {
			$rootScope.isOfflinePayment = false;
			$rootScope.pay = pay;
			$location.path('/paycompleted');
		}
	}
]).controller('paycompletedCtrl', [
	'$scope',
	'$rootScope',
	'$location',
	'$http',
	'$sce',
	function ($scope, $rootScope, $location, $http,$sce) {
		$rootScope.viewTitle = '完成点单';
		$rootScope.hideBackBtn = false;
		if ($rootScope.cart == null) {
			$location.path('/cart');
			return;
		}
		
		$scope.isCompleted = false;
		$scope.table = angular.copy($rootScope.cart.Table);
		
		function filter(data){
			for(var i=0;i<data.length;i++){
				delete data[i].AutoId;
				delete data[i].DisherCode;
				delete data[i].DisherEnglishName;
				delete data[i].DisherDescription;
				delete data[i].DisherSubclassID2;
				delete data[i].DisherStatus;
				delete data[i].Usable;
				delete data[i].DepartmentId;
				delete data[i].SourIndex;
				delete data[i].SweetIndex;
				delete data[i].SaltyIndex;
				delete data[i].SpicyIndex;
				delete data[i].Evaluate;
				delete data[i].Creator;
				delete data[i].Updator;
				delete data[i].Deletor;
				delete data[i].CreateDate;
				delete data[i].updateDate;
				delete data[i].DeleteDate;
				delete data[i].DisherPicture;
				delete data[i].DisherPoint;
				delete data[i].DisherSize;
				delete data[i].DisherSubclassID1;
				delete data[i].PinYin;
				delete data[i].Additional.FilteredNotes;
				delete data[i].Additional.IsNoteCollapsed;
			}
			
		}
		
		$scope.offlinePay = function () {
			$scope.isCompleted = true;
			$rootScope.cart.PriceAll += $rootScope.cart.Customer * $rootScope.tablewareFee;
			filter($rootScope.cart.Results);
			$http.post('/Cart/Submit', $rootScope.cart).success(function (data) {
				delete $rootScope.cart;
			});
		};

		$scope.onlinePay = function () {
			$scope.isCompleted = true;
			$rootScope.cart.IsPaid = 1;
			$rootScope.cart.PayKind = $rootScope.pay.PayName;
			$rootScope.cart.PriceAll += $rootScope.cart.Customer * $rootScope.tablewareFee;
			filter($rootScope.cart.Results);
			$http.post('/Cart/Submit', $rootScope.cart).success(function (data) {
				delete $rootScope.cart;
				$scope.redirectHtml=$sce.trustAsHtml(data);
			});
		}
		if(!$rootScope.isOfflinePayment){
			$scope.onlinePay();
		}
	}
]);

app.controller('ModalInstanceCtrl',function($scope,$modalInstance){
	$scope.close = function () {
		$modalInstance.dismiss('cancel');
	};
});