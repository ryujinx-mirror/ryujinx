package org.ryujinx.android.viewmodels

import org.ryujinx.android.RyujinxNative
import java.util.Base64

class UserViewModel {
    var openedUser = UserModel()
    val userList = mutableListOf<UserModel>()

    init {
        refreshUsers()
    }

    fun refreshUsers() {
        userList.clear()
        val decoder = Base64.getDecoder()
        openedUser = UserModel()
        openedUser.id = RyujinxNative.jnaInstance.userGetOpenedUser()
        if (openedUser.id.isNotEmpty()) {
            openedUser.username = RyujinxNative.jnaInstance.userGetUserName(openedUser.id)
            openedUser.userPicture = decoder.decode(
                RyujinxNative.jnaInstance.userGetUserPicture(
                    openedUser.id
                )
            )
        }

        val users = RyujinxNative.jnaInstance.userGetAllUsers()
        for (user in users) {
            userList.add(
                UserModel(
                    user,
                    RyujinxNative.jnaInstance.userGetUserName(user),
                    decoder.decode(
                        RyujinxNative.jnaInstance.userGetUserPicture(user)
                    )
                )
            )
        }
    }

    fun openUser(userModel: UserModel) {
        RyujinxNative.jnaInstance.userOpenUser(userModel.id)

        refreshUsers()
    }
}


data class UserModel(
    var id: String = "",
    var username: String = "",
    var userPicture: ByteArray? = null
) {
    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (javaClass != other?.javaClass) return false

        other as UserModel

        if (id != other.id) return false
        if (username != other.username) return false
        if (userPicture != null) {
            if (other.userPicture == null) return false
            if (!userPicture.contentEquals(other.userPicture)) return false
        } else if (other.userPicture != null) return false

        return true
    }

    override fun hashCode(): Int {
        var result = id.hashCode()
        result = 31 * result + username.hashCode()
        result = 31 * result + (userPicture?.contentHashCode() ?: 0)
        return result
    }
}
