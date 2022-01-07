bl_info = {
    "name": "Test2",
    "blender": (2, 83, 0),
    "category": "Object",
}

import bpy
from bpy import context

scene = context.scene;

cursor = scene.cursor.location

obj = context.active_object

obj_new = obj.copy()

scene.collection.objects.link(obj_new)

obj_new.location = cursor

class ObjectMoveX(bpy.types.Operator):
    """Object moving script"""
    bl_idname = "object.move_x"
    bl_label = "Move X by one"
    bl_options = {'REGISTER', 'UNDO'}
    
    def execute(self, context):
        scene = context.scene
        for obj in scene.objects:
            obj.location.x += 1.0
        
        return {'FINISHED'}

def register():
    bpy.utils.register_class(ObjectMoveX)
    
def unregister():
    bpy.utils.unregister_class(ObjectMoveX)
    
# This allows you to run the script directly from Blender's Text editor
# to test the add-on without having to install it.
if __name__ == "__main__":
    register()