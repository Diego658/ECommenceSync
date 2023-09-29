import ls from 'local-storage'

var UserSesion = (function() {
    var configuracion = null;
    var currentUser = null;
    var onUpdateConfiguracion;

    var getConfiguracion = function() {
      configuracion = ls.get('configuracion') || null;
      return configuracion;    // Or pull this from cookie/localStorage
    };

    var getCurrentUser= function(){
      currentUser = ls.get('currentUser') || null;
      return currentUser;
    }
  
    var setConfiguracion = function(newConfig) {
      configuracion = newConfig;
      ls.set('configuracion', newConfig);
      if (onUpdateConfiguracion !==null && onUpdateConfiguracion !== undefined){
        onUpdateConfiguracion();     
      }
      
      // Also set this in cookie/localStorage
    };
  

    var setOnUpdate= function(onUpdte){
      onUpdateConfiguracion = onUpdte;
    };

    var removeOnUpdate= function(){
      onUpdateConfiguracion = null;
    };

    return {
        getConfiguracion: getConfiguracion,
        setConfiguracion: setConfiguracion,
        getCurrentUser,
        setOnUpdate: setOnUpdate,
        removeOnUpdate: removeOnUpdate
    }
  
  })();
  
  export default UserSesion;